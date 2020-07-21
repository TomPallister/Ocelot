namespace Ocelot.UnitTests.Consul
{
    using global::Consul;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Newtonsoft.Json;
    using Ocelot.Logging;
    using Provider.Consul;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using TestStack.BDDfy;
    using Values;
    using Xunit;

    public class ConsulServiceDiscoveryProviderTests : IDisposable
    {
        private IWebHost _fakeConsulBuilder;
        private readonly List<ServiceEntry> _serviceEntries;
        private Consul _provider;
        private readonly string _serviceName;
        private readonly int _port;
        private readonly string _consulHost;
        private readonly string _consulScheme;
        private readonly string _fakeConsulServiceDiscoveryUrl;
        private List<Service> _services;
        private readonly Mock<IOcelotLoggerFactory> _factory;
        private readonly Mock<IOcelotLogger> _logger;
        private string _receivedToken;
        private readonly IConsulClientFactory _clientFactory;

        public ConsulServiceDiscoveryProviderTests()
        {
            _serviceName = "test";
            _port = 8500;
            _consulHost = "localhost";
            _consulScheme = "http";
            _fakeConsulServiceDiscoveryUrl = $"{_consulScheme}://{_consulHost}:{_port}";
            _serviceEntries = new List<ServiceEntry>();
            _factory = new Mock<IOcelotLoggerFactory>();
            _clientFactory = new ConsulClientFactory();
            _logger = new Mock<IOcelotLogger>();
            _factory.Setup(x => x.CreateLogger<Consul>()).Returns(_logger.Object);
            _factory.Setup(x => x.CreateLogger<PollConsul>()).Returns(_logger.Object);
            var config = new ConsulRegistryConfiguration(_consulScheme, _consulHost, _port, _serviceName, null);
            _provider = new Consul(config, _factory.Object, _clientFactory);
        }

        [Fact]
        public void should_return_service_from_consul()
        {
            var serviceEntryOne = new ServiceEntry()
            {
                Service = new AgentService()
                {
                    Service = _serviceName,
                    Address = "localhost",
                    Port = 50881,
                    ID = Guid.NewGuid().ToString(),
                    Tags = new string[0]
                },
            };

            this.Given(x => GivenThereIsAFakeConsulServiceDiscoveryProvider(_fakeConsulServiceDiscoveryUrl, _serviceName))
                .And(x => GivenTheServicesAreRegisteredWithConsul(serviceEntryOne))
                .When(x => WhenIGetTheServices())
                .Then(x => ThenTheCountIs(1))
                .BDDfy();
        }

        [Fact]
        public void should_use_token()
        {
            var token = "test token";
            var config = new ConsulRegistryConfiguration(_consulScheme, _consulHost, _port, _serviceName, token);
            _provider = new Consul(config, _factory.Object, _clientFactory);

            var serviceEntryOne = new ServiceEntry()
            {
                Service = new AgentService()
                {
                    Service = _serviceName,
                    Address = "localhost",
                    Port = 50881,
                    ID = Guid.NewGuid().ToString(),
                    Tags = new string[0]
                },
            };

            this.Given(_ => GivenThereIsAFakeConsulServiceDiscoveryProvider(_fakeConsulServiceDiscoveryUrl, _serviceName))
                .And(_ => GivenTheServicesAreRegisteredWithConsul(serviceEntryOne))
                .When(_ => WhenIGetTheServices())
                .Then(_ => ThenTheCountIs(1))
                .And(_ => _receivedToken.ShouldBe(token))
                .BDDfy();
        }

        [Fact]
        public void should_not_return_services_with_invalid_address()
        {
            var serviceEntryOne = new ServiceEntry()
            {
                Service = new AgentService()
                {
                    Service = _serviceName,
                    Address = "http://localhost",
                    Port = 50881,
                    ID = Guid.NewGuid().ToString(),
                    Tags = new string[0]
                },
            };

            var serviceEntryTwo = new ServiceEntry()
            {
                Service = new AgentService()
                {
                    Service = _serviceName,
                    Address = "http://localhost",
                    Port = 50888,
                    ID = Guid.NewGuid().ToString(),
                    Tags = new string[0]
                },
            };

            this.Given(x => GivenThereIsAFakeConsulServiceDiscoveryProvider(_fakeConsulServiceDiscoveryUrl, _serviceName))
                .And(x => GivenTheServicesAreRegisteredWithConsul(serviceEntryOne, serviceEntryTwo))
                .When(x => WhenIGetTheServices())
                .Then(x => ThenTheCountIs(0))
                .And(x => ThenTheLoggerHasBeenCalledCorrectlyForInvalidAddress())
                .BDDfy();
        }

        [Fact]
        public void should_not_return_services_with_empty_address()
        {
            var serviceEntryOne = new ServiceEntry()
            {
                Service = new AgentService()
                {
                    Service = _serviceName,
                    Address = "",
                    Port = 50881,
                    ID = Guid.NewGuid().ToString(),
                    Tags = new string[0]
                },
            };

            var serviceEntryTwo = new ServiceEntry()
            {
                Service = new AgentService()
                {
                    Service = _serviceName,
                    Address = null,
                    Port = 50888,
                    ID = Guid.NewGuid().ToString(),
                    Tags = new string[0]
                },
            };

            this.Given(x => GivenThereIsAFakeConsulServiceDiscoveryProvider(_fakeConsulServiceDiscoveryUrl, _serviceName))
                .And(x => GivenTheServicesAreRegisteredWithConsul(serviceEntryOne, serviceEntryTwo))
                .When(x => WhenIGetTheServices())
                .Then(x => ThenTheCountIs(0))
                .And(x => ThenTheLoggerHasBeenCalledCorrectlyForEmptyAddress())
                .BDDfy();
        }

        [Fact]
        public void should_not_return_services_with_invalid_port()
        {
            var serviceEntryOne = new ServiceEntry()
            {
                Service = new AgentService()
                {
                    Service = _serviceName,
                    Address = "localhost",
                    Port = -1,
                    ID = Guid.NewGuid().ToString(),
                    Tags = new string[0]
                },
            };

            var serviceEntryTwo = new ServiceEntry()
            {
                Service = new AgentService()
                {
                    Service = _serviceName,
                    Address = "localhost",
                    Port = 0,
                    ID = Guid.NewGuid().ToString(),
                    Tags = new string[0]
                },
            };

            this.Given(x => GivenThereIsAFakeConsulServiceDiscoveryProvider(_fakeConsulServiceDiscoveryUrl, _serviceName))
                .And(x => GivenTheServicesAreRegisteredWithConsul(serviceEntryOne, serviceEntryTwo))
                .When(x => WhenIGetTheServices())
                .Then(x => ThenTheCountIs(0))
                .And(x => ThenTheLoggerHasBeenCalledCorrectlyForInvalidPorts())
                .BDDfy();
        }

        private void ThenTheLoggerHasBeenCalledCorrectlyForInvalidAddress()
        {
            _logger.Verify(
                x => x.LogWarning(
                    "Unable to use service Address: http://localhost and Port: 50881 as it is invalid. Address must contain host only e.g. localhost and port must be greater than 0"),
                Times.Once);

            _logger.Verify(
                x => x.LogWarning(
                    "Unable to use service Address: http://localhost and Port: 50888 as it is invalid. Address must contain host only e.g. localhost and port must be greater than 0"),
                Times.Once);
        }

        private void ThenTheLoggerHasBeenCalledCorrectlyForEmptyAddress()
        {
            _logger.Verify(
                x => x.LogWarning(
                    "Unable to use service Address:  and Port: 50881 as it is invalid. Address must contain host only e.g. localhost and port must be greater than 0"),
                Times.Once);

            _logger.Verify(
                x => x.LogWarning(
                    "Unable to use service Address:  and Port: 50888 as it is invalid. Address must contain host only e.g. localhost and port must be greater than 0"),
                Times.Once);
        }

        private void ThenTheLoggerHasBeenCalledCorrectlyForInvalidPorts()
        {
            _logger.Verify(
                x => x.LogWarning(
                    "Unable to use service Address: localhost and Port: -1 as it is invalid. Address must contain host only e.g. localhost and port must be greater than 0"),
                Times.Once);

            _logger.Verify(
                x => x.LogWarning(
                    "Unable to use service Address: localhost and Port: 0 as it is invalid. Address must contain host only e.g. localhost and port must be greater than 0"),
                Times.Once);
        }

        private void ThenTheCountIs(int count)
        {
            _services.Count.ShouldBe(count);
        }

        private void WhenIGetTheServices()
        {
            _services = _provider.Get().GetAwaiter().GetResult();
        }

        private void GivenTheServicesAreRegisteredWithConsul(params ServiceEntry[] serviceEntries)
        {
            foreach (var serviceEntry in serviceEntries)
            {
                _serviceEntries.Add(serviceEntry);
            }
        }

        private void GivenThereIsAFakeConsulServiceDiscoveryProvider(string url, string serviceName)
        {
            _fakeConsulBuilder = new WebHostBuilder()
                .UseUrls(url)
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseUrls(url)
                .Configure(app =>
                {
                    app.Run(async context =>
                    {
                        if (context.Request.Path.Value == $"/v1/health/service/{serviceName}")
                        {
                            if (context.Request.Headers.TryGetValue("X-Consul-Token", out var values))
                            {
                                _receivedToken = values.First();
                            }

                            var json = JsonConvert.SerializeObject(_serviceEntries);
                            context.Response.Headers.Add("Content-Type", "application/json");
                            await context.Response.WriteAsync(json);
                        }
                    });
                })
                .Build();

            _fakeConsulBuilder.Start();
        }

        public void Dispose()
        {
            _fakeConsulBuilder?.Dispose();
        }
    }
}

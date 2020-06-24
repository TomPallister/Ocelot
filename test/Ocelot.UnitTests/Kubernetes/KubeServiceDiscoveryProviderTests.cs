using KubeClient;
using KubeClient.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json;
using Ocelot.Logging;
using Ocelot.Provider.Kubernetes;
using Ocelot.Values;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.Kubernetes
{
    public class KubeServiceDiscoveryProviderTests : IDisposable
    {
        private IWebHost _fakeKubeBuilder;
        private readonly KubernetesServiceDiscoveryProvider _provider;
        private EndpointsV1 _endpointEntries;
        private readonly string _serviceName;
        private readonly string _namespaces;
        private readonly int _port;
        private readonly string _kubeHost;
        private readonly string _fakekubeServiceDiscoveryUrl;
        private List<Service> _services;
        private readonly Mock<IOcelotLoggerFactory> _factory;
        private readonly Mock<IOcelotLogger> _logger;
        private string _receivedToken;
        private readonly IKubeApiClient _clientFactory;

        public KubeServiceDiscoveryProviderTests()
        {
            _serviceName = "test";
            _namespaces = "dev";
            _port = 86;
            _kubeHost = "localhost";
            _fakekubeServiceDiscoveryUrl = $"http://{_kubeHost}:{_port}";
            _endpointEntries = new EndpointsV1();
            _factory = new Mock<IOcelotLoggerFactory>();

            var option = new KubeClientOptions
            {
                ApiEndPoint = new Uri(_fakekubeServiceDiscoveryUrl),
                AccessToken = "txpc696iUhbVoudg164r93CxDTrKRVWG",
                AuthStrategy = KubeClient.KubeAuthStrategy.BearerToken,
                AllowInsecure = true,
            };

            _clientFactory = KubeApiClient.Create(option);
            _logger = new Mock<IOcelotLogger>();
            _factory.Setup(x => x.CreateLogger<KubernetesServiceDiscoveryProvider>()).Returns(_logger.Object);
            var config = new KubeRegistryConfiguration()
            {
                KeyOfServiceInK8s = _serviceName,
                KubeNamespace = _namespaces,
            };
            _provider = new KubernetesServiceDiscoveryProvider(config, _factory.Object, _clientFactory);
        }

        [Fact]
        public void should_return_service_from_k8s()
        {
            var token = "Bearer txpc696iUhbVoudg164r93CxDTrKRVWG";
            var endPointEntryOne = new EndpointsV1
            {
                Kind = "endpoint",
                ApiVersion = "1.0",
                Metadata = new ObjectMetaV1()
                {
                    Namespace = "dev",
                },
            };
            var endpointSubsetV1 = new EndpointSubsetV1();
            endpointSubsetV1.Addresses.Add(new EndpointAddressV1()
            {
                Ip = "127.0.0.1",
                Hostname = "localhost",
            });
            endpointSubsetV1.Ports.Add(new EndpointPortV1()
            {
                Port = 80,
            });
            endPointEntryOne.Subsets.Add(endpointSubsetV1);

            this.Given(x => GivenThereIsAFakeKubeServiceDiscoveryProvider(_fakekubeServiceDiscoveryUrl, _serviceName, _namespaces))
                .And(x => GivenTheServicesAreRegisteredWithKube(endPointEntryOne))
                .When(x => WhenIGetTheServices())
                .Then(x => ThenTheCountIs(1))
                .And(_ => _receivedToken.ShouldBe(token))
                .BDDfy();
        }

        private void ThenTheCountIs(int count)
        {
            _services.Count.ShouldBe(count);
        }

        private void WhenIGetTheServices()
        {
            _services = _provider.Get().GetAwaiter().GetResult();
        }

        private void GivenTheServicesAreRegisteredWithKube(EndpointsV1 endpointEntries)
        {
            _endpointEntries = endpointEntries;
        }

        private void GivenThereIsAFakeKubeServiceDiscoveryProvider(string url, string serviceName, string namespaces)
        {
            _fakeKubeBuilder = new WebHostBuilder()
                .UseUrls(url)
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseUrls(url)
                .Configure(app =>
                {
                    app.Run(async context =>
                    {
                        if (context.Request.Path.Value == $"/api/v1/namespaces/{namespaces}/endpoints/{serviceName}")
                        {
                            if (context.Request.Headers.TryGetValue("Authorization", out var values))
                            {
                                _receivedToken = values.First();
                            }

                            var json = JsonConvert.SerializeObject(_endpointEntries);
                            context.Response.Headers.Add("Content-Type", "application/json");
                            await context.Response.WriteAsync(json);
                        }
                    });
                })
                .Build();

            _fakeKubeBuilder.Start();
        }

        public void Dispose()
        {
            _fakeKubeBuilder?.Dispose();
        }
    }
}

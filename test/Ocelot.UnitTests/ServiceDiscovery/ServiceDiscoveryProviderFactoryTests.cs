namespace Ocelot.UnitTests.ServiceDiscovery
{
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using Ocelot.Configuration;
    using Ocelot.Configuration.Builder;
    using Ocelot.Logging;
    using Ocelot.Responses;
    using Ocelot.ServiceDiscovery;
    using Ocelot.ServiceDiscovery.Providers;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TestStack.BDDfy;
    using Values;
    using Xunit;

    public class ServiceDiscoveryProviderFactoryTests
    {
        private ServiceProviderConfiguration _serviceConfig;
        private Response<IServiceDiscoveryProvider> _result;
        private ServiceDiscoveryProviderFactory _factory;
        private DownstreamRoute _route;
        private readonly Mock<IOcelotLoggerFactory> _loggerFactory;
        private Mock<IOcelotLogger> _logger;
        private IServiceProvider _provider;
        private readonly IServiceCollection _collection;

        public ServiceDiscoveryProviderFactoryTests()
        {
            _loggerFactory = new Mock<IOcelotLoggerFactory>();
            _logger = new Mock<IOcelotLogger>();
            _collection = new ServiceCollection();
            _provider = _collection.BuildServiceProvider();
            _factory = new ServiceDiscoveryProviderFactory(_loggerFactory.Object, _provider);
        }

        [Fact]
        public void should_return_no_service_provider()
        {
            var serviceConfig = new ServiceProviderConfigurationBuilder()
                .Build();

            var route = new DownstreamRouteBuilder().Build();

            this.Given(x => x.GivenTheRoute(serviceConfig, route))
                .When(x => x.WhenIGetTheServiceProvider())
                .Then(x => x.ThenTheServiceProviderIs<ConfigurationServiceProvider>())
                .BDDfy();
        }

        [Fact]
        public void should_return_list_of_configuration_services()
        {
            var serviceConfig = new ServiceProviderConfigurationBuilder()
                .Build();

            var downstreamAddresses = new List<DownstreamHostAndPort>()
            {
                new DownstreamHostAndPort("asdf.com", 80),
                new DownstreamHostAndPort("abc.com", 80)
            };

            var route = new DownstreamRouteBuilder().WithDownstreamAddresses(downstreamAddresses).Build();

            this.Given(x => x.GivenTheRoute(serviceConfig, route))
                .When(x => x.WhenIGetTheServiceProvider())
                .Then(x => x.ThenTheServiceProviderIs<ConfigurationServiceProvider>())
                .Then(x => ThenTheFollowingServicesAreReturned(downstreamAddresses))
                .BDDfy();
        }

        [Fact]
        public void should_return_provider_because_type_matches_reflected_type_from_delegate()
        {
            var route = new DownstreamRouteBuilder()
                .WithServiceName("product")
                .WithUseServiceDiscovery(true)
                .Build();

            var serviceConfig = new ServiceProviderConfigurationBuilder()
                .WithType(nameof(Fake))
                .Build();

            this.Given(x => x.GivenTheRoute(serviceConfig, route))
                .And(x => GivenAFakeDelegate())
                .When(x => x.WhenIGetTheServiceProvider())
                .Then(x => x.ThenTheDelegateIsCalled())
                .BDDfy();
        }

        [Fact]
        public void should_not_return_provider_because_type_doesnt_match_reflected_type_from_delegate()
        {
            var route = new DownstreamRouteBuilder()
                .WithServiceName("product")
                .WithUseServiceDiscovery(true)
                .Build();

            var serviceConfig = new ServiceProviderConfigurationBuilder()
                .WithType("Wookie")
                .Build();

            this.Given(x => x.GivenTheRoute(serviceConfig, route))
                .And(x => GivenAFakeDelegate())
                .When(x => x.WhenIGetTheServiceProvider())
                .Then(x => x.ThenTheResultIsError())
                .BDDfy();
        }

        [Fact]
        public void should_return_service_fabric_provider()
        {
            var route = new DownstreamRouteBuilder()
                .WithServiceName("product")
                .WithUseServiceDiscovery(true)
                .Build();

            var serviceConfig = new ServiceProviderConfigurationBuilder()
                .WithType("ServiceFabric")
                .Build();

            this.Given(x => x.GivenTheRoute(serviceConfig, route))
                .When(x => x.WhenIGetTheServiceProvider())
                .Then(x => x.ThenTheServiceProviderIs<ServiceFabricServiceDiscoveryProvider>())
                .BDDfy();
        }

        private void GivenAFakeDelegate()
        {
            ServiceDiscoveryFinderDelegate fake = (provider, config, name) => new Fake();
            _collection.AddSingleton(fake);
            _provider = _collection.BuildServiceProvider();
            _factory = new ServiceDiscoveryProviderFactory(_loggerFactory.Object, _provider);
        }

        private class Fake : IServiceDiscoveryProvider
        {
            public Task<List<Service>> Get()
            {
                return null;
            }
        }

        private void ThenTheDelegateIsCalled()
        {
            _result.Data.GetType().Name.ShouldBe("Fake");
        }

        private void ThenTheResultIsError()
        {
            _result.IsError.ShouldBeTrue();
        }

        private void ThenTheFollowingServicesAreReturned(List<DownstreamHostAndPort> downstreamAddresses)
        {
            var result = (ConfigurationServiceProvider)_result.Data;
            var services = result.Get().Result;

            for (int i = 0; i < services.Count; i++)
            {
                var service = services[i];
                var downstreamAddress = downstreamAddresses[i];

                service.HostAndPort.DownstreamHost.ShouldBe(downstreamAddress.Host);
                service.HostAndPort.DownstreamPort.ShouldBe(downstreamAddress.Port);
            }
        }

        private void GivenTheRoute(ServiceProviderConfiguration serviceConfig, DownstreamRoute route)
        {
            _serviceConfig = serviceConfig;
            _route = route;
        }

        private void WhenIGetTheServiceProvider()
        {
            _result = _factory.Get(_serviceConfig, _route);
        }

        private void ThenTheServiceProviderIs<T>()
        {
            _result.Data.ShouldBeOfType<T>();
        }
    }
}

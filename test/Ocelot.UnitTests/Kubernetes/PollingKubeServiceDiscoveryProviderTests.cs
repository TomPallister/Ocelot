using Moq;
using Ocelot.Infrastructure;
using Ocelot.Logging;
using Ocelot.Provider.Kubernetes;
using Ocelot.ServiceDiscovery.Providers;
using Ocelot.Values;
using Shouldly;
using System;
using System.Collections.Generic;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.Kubernetes
{
    public class PollingKubeServiceDiscoveryProviderTests
    {
        private readonly int _delay;
        private PollKubernetes _provider;
        private readonly List<Service> _services;
        private readonly Mock<IOcelotLoggerFactory> _factory;
        private readonly Mock<IOcelotLogger> _logger;
        private readonly Mock<IServiceDiscoveryProvider> _kubeServiceDiscoveryProvider;
        private List<Service> _result;

        public PollingKubeServiceDiscoveryProviderTests()
        {
            _services = new List<Service>();
            _delay = 1;
            _factory = new Mock<IOcelotLoggerFactory>();
            _logger = new Mock<IOcelotLogger>();
            _factory.Setup(x => x.CreateLogger<PollKubernetes>()).Returns(_logger.Object);
            _kubeServiceDiscoveryProvider = new Mock<IServiceDiscoveryProvider>();
        }

        [Fact]
        public void should_return_service_from_kube()
        {
            var service = new Service("", new ServiceHostAndPort("", 0), "", "", new List<string>());

            this.Given(x => GivenKubeReturns(service))
                .When(x => WhenIGetTheServices(1))
                .Then(x => ThenTheCountIs(1))
                .BDDfy();
        }

        private void GivenKubeReturns(Service service)
        {
            _services.Add(service);
            _kubeServiceDiscoveryProvider.Setup(x => x.Get()).ReturnsAsync(_services);
        }

        private void ThenTheCountIs(int count)
        {
            _result.Count.ShouldBe(count);
        }

        private void WhenIGetTheServices(int expected)
        {
            _provider = new PollKubernetes(_delay, _factory.Object, _kubeServiceDiscoveryProvider.Object);

            var result = Wait.WaitFor(3000).Until(() =>
            {
                try
                {
                    _result = _provider.Get().GetAwaiter().GetResult();
                    if (_result.Count == expected)
                    {
                        return true;
                    }

                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            });

            result.ShouldBeTrue();
        }
    }
}

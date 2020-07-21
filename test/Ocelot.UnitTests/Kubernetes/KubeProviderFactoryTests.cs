using Microsoft.Extensions.DependencyInjection;
using Moq;
using Ocelot.Configuration;
using Ocelot.Logging;
using Ocelot.Provider.Kubernetes;
using Shouldly;
using System;
using Xunit;

namespace Ocelot.UnitTests.Kubernetes
{
    public class KubeProviderFactoryTests
    {
        private readonly IServiceProvider _provider;

        public KubeProviderFactoryTests()
        {
            var services = new ServiceCollection();
            var loggerFactory = new Mock<IOcelotLoggerFactory>();
            var logger = new Mock<IOcelotLogger>();
            loggerFactory.Setup(x => x.CreateLogger<Kube>()).Returns(logger.Object);
            var kubeFactory = new Mock<IKubeApiClientFactory>();
            services.AddSingleton(kubeFactory.Object);
            services.AddSingleton(loggerFactory.Object);
            _provider = services.BuildServiceProvider();
        }

        [Fact]
        public void should_return_KubeServiceDiscoveryProvider()
        {
            var provider = KubernetesProviderFactory.Get(_provider, new ServiceProviderConfiguration("kube", "localhost", 443, "", "", 1,"dev"), "");
            provider.ShouldBeOfType<Kube>();
        }
    }
}

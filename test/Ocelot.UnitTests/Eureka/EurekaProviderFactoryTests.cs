namespace Ocelot.UnitTests.Eureka
{
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using Ocelot.Configuration.Builder;
    using Provider.Eureka;
    using Shouldly;
    using Steeltoe.Common.Discovery;
    using Xunit;

    public class EurekaProviderFactoryTests
    {
        [Fact]
        public void should_not_get()
        {
            var config = new ServiceProviderConfigurationBuilder().Build();
            var sp = new ServiceCollection().BuildServiceProvider();
            var provider = EurekaProviderFactory.Get(sp, config, null);
            provider.ShouldBeNull();
        }

        [Fact]
        public void should_get()
        {
            var config = new ServiceProviderConfigurationBuilder().WithType("eureka").Build();
            var client = new Mock<IDiscoveryClient>();
            var services = new ServiceCollection();
            services.AddSingleton<IDiscoveryClient>(client.Object);
            var sp = services.BuildServiceProvider();
            var route = new DownstreamRouteBuilder()
                .WithServiceName("")
                .Build();
            var provider = EurekaProviderFactory.Get(sp, config, route);
            provider.ShouldBeOfType<Eureka>();
        }
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ocelot.Multiplexer;
using System;
using System.Net.Http;
using Ocelot.Configuration;
using Ocelot.LoadBalancer.LoadBalancers;
using Ocelot.ServiceDiscovery.Providers;

namespace Ocelot.DependencyInjection
{
    public interface IOcelotBuilder
    {
        IServiceCollection Services { get; }

        IConfiguration Configuration { get; }

        IMvcCoreBuilder MvcCoreBuilder { get; }

        IOcelotBuilder AddDelegatingHandler(Type type, bool global = false);

        IOcelotBuilder AddDelegatingHandler<T>(bool global = false)
            where T : DelegatingHandler;

        IOcelotBuilder AddSingletonDefinedAggregator<T>()
            where T : class, IDefinedAggregator;

        IOcelotBuilder AddTransientDefinedAggregator<T>()
            where T : class, IDefinedAggregator;

        IOcelotBuilder AddCustomLoadBalancer<T>()
            where T : ILoadBalancer, new();
        
        IOcelotBuilder AddCustomLoadBalancer<T>(Func<T> loadBalancerFactoryFunc)
            where T : ILoadBalancer;

        IOcelotBuilder AddCustomLoadBalancer<T>(Func<IServiceProvider, T> loadBalancerFactoryFunc)
            where T : ILoadBalancer;

        IOcelotBuilder AddCustomLoadBalancer<T>(
            Func<DownstreamRoute, IServiceDiscoveryProvider, T> loadBalancerFactoryFunc)
            where T : ILoadBalancer;

        IOcelotBuilder AddCustomLoadBalancer<T>(
            Func<IServiceProvider, DownstreamRoute, IServiceDiscoveryProvider, T> loadBalancerFactoryFunc)
            where T : ILoadBalancer;

        IOcelotBuilder AddConfigPlaceholders();
    }
}

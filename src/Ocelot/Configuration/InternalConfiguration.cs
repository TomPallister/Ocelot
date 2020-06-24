using System.Collections.Generic;

namespace Ocelot.Configuration
{
    using System;

    public class InternalConfiguration : IInternalConfiguration
    {
        public InternalConfiguration(
            List<Route> routes,
            string administrationPath,
            ServiceProviderConfiguration serviceProviderConfiguration,
            string requestId,
            LoadBalancerOptions loadBalancerOptions,
            string downstreamScheme,
            QoSOptions qoSOptions,
            HttpHandlerOptions httpHandlerOptions,
            Version downstreamHttpVersion)
        {
            Routes = routes;
            AdministrationPath = administrationPath;
            ServiceProviderConfiguration = serviceProviderConfiguration;
            RequestId = requestId;
            LoadBalancerOptions = loadBalancerOptions;
            DownstreamScheme = downstreamScheme;
            QoSOptions = qoSOptions;
            HttpHandlerOptions = httpHandlerOptions;
            DownstreamHttpVersion = downstreamHttpVersion;
        }

        public List<Route> Routes { get; }
        public string AdministrationPath { get; }
        public ServiceProviderConfiguration ServiceProviderConfiguration { get; }
        public string RequestId { get; }
        public LoadBalancerOptions LoadBalancerOptions { get; }
        public string DownstreamScheme { get; }
        public QoSOptions QoSOptions { get; }
        public HttpHandlerOptions HttpHandlerOptions { get; }

        public Version DownstreamHttpVersion { get; }
    }
}

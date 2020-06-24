using System.Collections.Generic;

namespace Ocelot.Configuration
{
    using System;

    public interface IInternalConfiguration
    {
        List<Route> Routes { get; }

        string AdministrationPath { get; }

        ServiceProviderConfiguration ServiceProviderConfiguration { get; }

        string RequestId { get; }

        LoadBalancerOptions LoadBalancerOptions { get; }

        string DownstreamScheme { get; }

        QoSOptions QoSOptions { get; }

        HttpHandlerOptions HttpHandlerOptions { get; }

        Version DownstreamHttpVersion { get;  }
    }
}

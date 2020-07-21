namespace Ocelot.Provider.Polly
{
    using Configuration;
    using DependencyInjection;
    using Errors;
    using global::Polly.CircuitBreaker;
    using global::Polly.Timeout;
    using Logging;
    using Microsoft.Extensions.DependencyInjection;
    using Requester;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    public static class OcelotBuilderExtensions
    {
        public static IOcelotBuilder AddPolly(this IOcelotBuilder builder)
        {
            var errorMapping = new Dictionary<Type, Func<Exception, Error>>
            {
                {typeof(TaskCanceledException), e => new RequestTimedOutError(e)},
                {typeof(TimeoutRejectedException), e => new RequestTimedOutError(e)},
                {typeof(BrokenCircuitException), e => new RequestTimedOutError(e)}
            };

            builder.Services.AddSingleton(errorMapping);

            DelegatingHandler QosDelegatingHandlerDelegate(DownstreamRoute route, IOcelotLoggerFactory logger)
            {
                return new PollyCircuitBreakingDelegatingHandler(new PollyQoSProvider(route, logger), logger);
            }

            builder.Services.AddSingleton((QosDelegatingHandlerDelegate)QosDelegatingHandlerDelegate);
            return builder;
        }
    }
}

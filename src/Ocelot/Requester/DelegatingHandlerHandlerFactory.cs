namespace Ocelot.Requester
{
    using Logging;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Ocelot.Configuration;
    using Ocelot.Responses;
    using QoS;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;

    public class DelegatingHandlerHandlerFactory : IDelegatingHandlerHandlerFactory
    {
        private readonly ITracingHandlerFactory _tracingFactory;
        private readonly IQoSFactory _qoSFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly IOcelotLogger _logger;

        public DelegatingHandlerHandlerFactory(
            ITracingHandlerFactory tracingFactory,
            IQoSFactory qoSFactory,
            IServiceProvider serviceProvider,
            IOcelotLoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<DelegatingHandlerHandlerFactory>();
            _serviceProvider = serviceProvider;
            _tracingFactory = tracingFactory;
            _qoSFactory = qoSFactory;
        }

        public Response<List<Func<DelegatingHandler>>> Get(DownstreamRoute downstreamRoute, HttpContext httpContext)
        {
            var globalDelegatingHandlers = _serviceProvider
                .GetServices<GlobalDelegatingHandler>()
                .ToList();

            var routeSpecificHandlers = _serviceProvider
                .GetServices<DelegatingHandler>()
                .ToList();

            var handlers = new List<Func<DelegatingHandler>>();

            foreach (var handler in globalDelegatingHandlers)
            {
                if (GlobalIsInHandlersConfig(downstreamRoute, handler))
                {
                    routeSpecificHandlers.Add(handler.DelegatingHandler);
                }
                else
                {
                    handlers.Add(() => handler.DelegatingHandler);
                }
            }

            if (downstreamRoute.DelegatingHandlers.Any())
            {
                var sorted = SortByConfigOrder(downstreamRoute, routeSpecificHandlers);

                foreach (var handler in sorted)
                {
                    handlers.Add(() => handler);
                }
            }

            if (downstreamRoute.HttpHandlerOptions.UseTracing)
            {
                handlers.Add(() => (DelegatingHandler)_tracingFactory.Get());
            }

            if (downstreamRoute.QosOptions.UseQos)
            {
                var handler = _qoSFactory.Get(downstreamRoute);

                if (handler != null && !handler.IsError)
                {
                    handlers.Add(() => handler.Data);
                }
                else
                {
                    _logger.LogWarning($"Route {downstreamRoute.UpstreamPathTemplate} specifies use QoS but no QosHandler found in DI container. Will use not use a QosHandler, please check your setup!");
                    handlers.Add(() => new NoQosDelegatingHandler());
                }
            }

            // attach HttpContext if necessary
            var finalHandlers = handlers.Select<Func<DelegatingHandler>, Func<DelegatingHandler>>(originalHandlerFunc => () =>
            {
                DelegatingHandler delegatingHandler = originalHandlerFunc();
                if (delegatingHandler is IDelegatingHandlerWithHttpContext innerContext)
                {
                    innerContext.HttpContext = httpContext;
                }

                return delegatingHandler;
            }).ToList();

            return new OkResponse<List<Func<DelegatingHandler>>>(finalHandlers);
        }

        private List<DelegatingHandler> SortByConfigOrder(DownstreamRoute request, List<DelegatingHandler> routeSpecificHandlers)
        {
            return routeSpecificHandlers
                .Where(x => request.DelegatingHandlers.Contains(x.GetType().Name))
                .OrderBy(d =>
                {
                    var type = d.GetType().Name;
                    var pos = request.DelegatingHandlers.IndexOf(type);
                    return pos;
                }).ToList();
        }

        private bool GlobalIsInHandlersConfig(DownstreamRoute request, GlobalDelegatingHandler handler)
        {
            return request.DelegatingHandlers.Contains(handler.DelegatingHandler.GetType().Name);
        }
    }
}

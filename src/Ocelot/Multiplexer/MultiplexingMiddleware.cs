namespace Ocelot.Multiplexer
{
    using Microsoft.AspNetCore.Http;
    using Ocelot.Configuration;
    using Ocelot.DownstreamRouteFinder.UrlMatcher;
    using Ocelot.Logging;
    using Ocelot.Middleware;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class MultiplexingMiddleware : OcelotMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IResponseAggregatorFactory _factory;

        public MultiplexingMiddleware(RequestDelegate next,
            IOcelotLoggerFactory loggerFactory,
            IResponseAggregatorFactory factory
            )
                : base(loggerFactory.CreateLogger<MultiplexingMiddleware>())
        {
            _factory = factory;
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.WebSockets.IsWebSocketRequest)
            {
                //todo this is obviously stupid
                httpContext.Items.UpsertDownstreamRoute(httpContext.Items.DownstreamRouteHolder().Route.DownstreamRoute[0]);
                await _next.Invoke(httpContext);
                return;
            }

            var routeKeysConfigs = httpContext.Items.DownstreamRouteHolder().Route.DownstreamRouteConfig;
            if (routeKeysConfigs == null || !routeKeysConfigs.Any())
            {
                var downstreamRouteHolder = httpContext.Items.DownstreamRouteHolder();

                var tasks = new Task<HttpContext>[downstreamRouteHolder.Route.DownstreamRoute.Count];

                for (var i = 0; i < downstreamRouteHolder.Route.DownstreamRoute.Count; i++)
                {
                    var newHttpContext = Copy(httpContext);

                    newHttpContext.Items
                        .Add("RequestId", httpContext.Items["RequestId"]);
                    newHttpContext.Items
                        .SetIInternalConfiguration(httpContext.Items.IInternalConfiguration());
                    newHttpContext.Items
                        .UpsertTemplatePlaceholderNameAndValues(httpContext.Items.TemplatePlaceholderNameAndValues());
                    newHttpContext.Items
                        .UpsertDownstreamRoute(downstreamRouteHolder.Route.DownstreamRoute[i]);

                    tasks[i] = Fire(newHttpContext, _next);
                }

                await Task.WhenAll(tasks);

                var contexts = new List<HttpContext>();

                foreach (var task in tasks)
                {
                    var finished = await task;
                    contexts.Add(finished);
                }

                await Map(httpContext, downstreamRouteHolder.Route, contexts);
            }
            else
            {
                httpContext.Items.UpsertDownstreamRoute(httpContext.Items.DownstreamRouteHolder().Route.DownstreamRoute[0]);
                var mainResponse = await Fire(httpContext, _next);

                if (httpContext.Items.DownstreamRouteHolder().Route.DownstreamRoute.Count == 1)
                {
                    MapNotAggregate(httpContext, new List<HttpContext>() { mainResponse });
                    return;
                }

                var tasks = new List<Task<HttpContext>>();

                if (mainResponse.Items.DownstreamResponse() == null)
                {
                    return;
                }

                var content = await mainResponse.Items.DownstreamResponse().Content.ReadAsStringAsync();

                var jObject = Newtonsoft.Json.Linq.JToken.Parse(content);

                for (var i = 1; i < httpContext.Items.DownstreamRouteHolder().Route.DownstreamRoute.Count; i++)
                {
                    var templatePlaceholderNameAndValues = httpContext.Items.TemplatePlaceholderNameAndValues();

                    var downstreamRoute = httpContext.Items.DownstreamRouteHolder().Route.DownstreamRoute[i];

                    var matchAdvancedAgg = routeKeysConfigs
                        .FirstOrDefault(q => q.RouteKey == downstreamRoute.Key);

                    if (matchAdvancedAgg != null)
                    {
                        var values = jObject.SelectTokens(matchAdvancedAgg.JsonPath).Select(s => s.ToString()).Distinct().ToList();

                        foreach (var value in values)
                        {
                            var newHttpContext = Copy(httpContext);

                            var tPNV = httpContext.Items.TemplatePlaceholderNameAndValues();
                            tPNV.Add(new PlaceholderNameAndValue("{" + matchAdvancedAgg.Parameter + "}", value.ToString()));

                            newHttpContext.Items
                                .Add("RequestId", httpContext.Items["RequestId"]);

                            newHttpContext.Items
                                .SetIInternalConfiguration(httpContext.Items.IInternalConfiguration());

                            newHttpContext.Items
                                .UpsertTemplatePlaceholderNameAndValues(tPNV);

                            newHttpContext.Items
                                .UpsertDownstreamRoute(downstreamRoute);

                            tasks.Add(Fire(newHttpContext, _next));
                        }
                    }
                    else
                    {
                        var newHttpContext = Copy(httpContext);

                        newHttpContext.Items
                               .Add("RequestId", httpContext.Items["RequestId"]);

                        newHttpContext.Items
                            .SetIInternalConfiguration(httpContext.Items.IInternalConfiguration());

                        newHttpContext.Items
                            .UpsertTemplatePlaceholderNameAndValues(templatePlaceholderNameAndValues);

                        newHttpContext.Items
                            .UpsertDownstreamRoute(downstreamRoute);

                        tasks.Add(Fire(newHttpContext, _next));
                    }
                }

                await Task.WhenAll(tasks);

                var contexts = new List<HttpContext>() { mainResponse };

                foreach (var task in tasks)
                {
                    var finished = await task;
                    contexts.Add(finished);
                }

                await Map(httpContext, httpContext.Items.DownstreamRouteHolder().Route, contexts);
            }
        }

        private HttpContext Copy(HttpContext source)
        {
            var target = new DefaultHttpContext();

            foreach (var header in source.Request.Headers)
            {
                target.Request.Headers.TryAdd(header.Key, header.Value);
            }

            target.Request.Body = source.Request.Body;
            target.Request.ContentLength = source.Request.ContentLength;
            target.Request.ContentType = source.Request.ContentType;
            target.Request.Host = source.Request.Host;
            target.Request.Method = source.Request.Method;
            target.Request.Path = source.Request.Path;
            target.Request.PathBase = source.Request.PathBase;
            target.Request.Protocol = source.Request.Protocol;
            target.Request.Query = source.Request.Query;
            target.Request.QueryString = source.Request.QueryString;
            target.Request.Scheme = source.Request.Scheme;
            target.Request.IsHttps = source.Request.IsHttps;
            target.Request.RouteValues = source.Request.RouteValues;
            target.Connection.RemoteIpAddress = source.Connection.RemoteIpAddress;
            target.RequestServices = source.RequestServices;
            return target;
        }

        private async Task Map(HttpContext httpContext, Route route, List<HttpContext> contexts)
        {
            if (route.DownstreamRoute.Count > 1)
            {
                var aggregator = _factory.Get(route);
                await aggregator.Aggregate(route, httpContext, contexts);
            }
            else
            {
                MapNotAggregate(httpContext, contexts);
            }
        }

        private void MapNotAggregate(HttpContext httpContext, List<HttpContext> downstreamContexts)
        {
            //assume at least one..if this errors then it will be caught by global exception handler
            var finished = downstreamContexts.First();

            httpContext.Items.UpsertErrors(finished.Items.Errors());

            httpContext.Items.UpsertDownstreamRequest(finished.Items.DownstreamRequest());

            httpContext.Items.UpsertDownstreamResponse(finished.Items.DownstreamResponse());
        }

        private async Task<HttpContext> Fire(HttpContext httpContext, RequestDelegate next)
        {
            await next.Invoke(httpContext);
            return httpContext;
        }
    }
}

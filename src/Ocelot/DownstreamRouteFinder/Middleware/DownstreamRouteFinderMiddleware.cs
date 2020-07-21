namespace Ocelot.DownstreamRouteFinder.Middleware
{
    using Microsoft.AspNetCore.Http;
    using Ocelot.DownstreamRouteFinder.Finder;
    using Ocelot.Infrastructure.Extensions;
    using Ocelot.Logging;
    using Ocelot.Middleware;
    using System.Linq;
    using System.Threading.Tasks;

    public class DownstreamRouteFinderMiddleware : OcelotMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDownstreamRouteProviderFactory _factory;

        public DownstreamRouteFinderMiddleware(RequestDelegate next,
            IOcelotLoggerFactory loggerFactory,
            IDownstreamRouteProviderFactory downstreamRouteFinder
            )
                : base(loggerFactory.CreateLogger<DownstreamRouteFinderMiddleware>())
        {
            _next = next;
            _factory = downstreamRouteFinder;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var upstreamUrlPath = httpContext.Request.Path.ToString();

            var upstreamQueryString = httpContext.Request.QueryString.ToString();

            var upstreamHost = httpContext.Request.Headers["Host"];

            Logger.LogDebug($"Upstream url path is {upstreamUrlPath}");

            var internalConfiguration = httpContext.Items.IInternalConfiguration();

            var provider = _factory.Get(internalConfiguration);

            var response = provider.Get(upstreamUrlPath, upstreamQueryString, httpContext.Request.Method, internalConfiguration, upstreamHost);

            if (response.IsError)
            {
                Logger.LogWarning($"{MiddlewareName} setting pipeline errors. IDownstreamRouteFinder returned {response.Errors.ToErrorString()}");

                httpContext.Items.UpsertErrors(response.Errors);
                return;
            }

            var downstreamPathTemplates = string.Join(", ", response.Data.Route.DownstreamRoute.Select(r => r.DownstreamPathTemplate.Value));
            Logger.LogDebug($"downstream templates are {downstreamPathTemplates}");

            // why set both of these on HttpContext
            httpContext.Items.UpsertTemplatePlaceholderNameAndValues(response.Data.TemplatePlaceholderNameAndValues);

            httpContext.Items.UpsertDownstreamRoute(response.Data);

            await _next.Invoke(httpContext);
        }
    }
}

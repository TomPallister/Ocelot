namespace Ocelot.Headers.Middleware
{
    using Microsoft.AspNetCore.Http;
    using Ocelot.DownstreamRouteFinder.Middleware;
    using Ocelot.Logging;
    using Ocelot.Middleware;
    using System.Linq;
    using System.Threading.Tasks;

    public class ClaimsToHeadersMiddleware : OcelotMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAddHeadersToRequest _addHeadersToRequest;

        public ClaimsToHeadersMiddleware(RequestDelegate next,
            IOcelotLoggerFactory loggerFactory,
            IAddHeadersToRequest addHeadersToRequest)
                : base(loggerFactory.CreateLogger<ClaimsToHeadersMiddleware>())
        {
            _next = next;
            _addHeadersToRequest = addHeadersToRequest;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var downstreamRoute = httpContext.Items.DownstreamRoute();

            if (downstreamRoute.ClaimsToHeaders.Any())
            {
                Logger.LogInformation($"{downstreamRoute.DownstreamPathTemplate.Value} has instructions to convert claims to headers");

                var downstreamRequest = httpContext.Items.DownstreamRequest();

                var response = _addHeadersToRequest.SetHeadersOnDownstreamRequest(downstreamRoute.ClaimsToHeaders, httpContext.User.Claims, downstreamRequest);

                if (response.IsError)
                {
                    Logger.LogWarning("Error setting headers on context, setting pipeline error");

                    httpContext.Items.UpsertErrors(response.Errors);
                    return;
                }

                Logger.LogInformation("headers have been set on context");
            }

            await _next.Invoke(httpContext);
        }
    }
}

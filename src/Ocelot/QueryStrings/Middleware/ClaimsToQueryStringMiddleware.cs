namespace Ocelot.QueryStrings.Middleware
{
    using Ocelot.Logging;
    using Ocelot.Middleware;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Ocelot.DownstreamRouteFinder.Middleware;

    public class ClaimsToQueryStringMiddleware : OcelotMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAddQueriesToRequest _addQueriesToRequest;

        public ClaimsToQueryStringMiddleware(RequestDelegate next,
            IOcelotLoggerFactory loggerFactory,
            IAddQueriesToRequest addQueriesToRequest)
                : base(loggerFactory.CreateLogger<ClaimsToQueryStringMiddleware>())
        {
            _next = next;
            _addQueriesToRequest = addQueriesToRequest;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var downstreamRoute = httpContext.Items.DownstreamRoute();

            if (downstreamRoute.ClaimsToQueries.Any())
            {
                Logger.LogInformation($"{downstreamRoute.DownstreamPathTemplate.Value} has instructions to convert claims to queries");

                var downstreamRequest = httpContext.Items.DownstreamRequest();

                var response = _addQueriesToRequest.SetQueriesOnDownstreamRequest(downstreamRoute.ClaimsToQueries, httpContext.User.Claims, downstreamRequest);

                if (response.IsError)
                {
                    Logger.LogWarning("there was an error setting queries on context, setting pipeline error");

                    httpContext.Items.UpsertErrors(response.Errors);
                    return;
                }
            }

            await _next.Invoke(httpContext);
        }
    }
}

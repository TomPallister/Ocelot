namespace Ocelot.DownstreamPathManipulation.Middleware
{
    using System.Linq;
    using System.Threading.Tasks;
    using Ocelot.Logging;
    using Microsoft.AspNetCore.Http;
    using Ocelot.Middleware;
    using Ocelot.PathManipulation;
    using Ocelot.DownstreamRouteFinder.Middleware;

    public class ClaimsToDownstreamPathMiddleware : OcelotMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IChangeDownstreamPathTemplate _changeDownstreamPathTemplate;

        public ClaimsToDownstreamPathMiddleware(RequestDelegate next,
            IOcelotLoggerFactory loggerFactory,
            IChangeDownstreamPathTemplate changeDownstreamPathTemplate)
                : base(loggerFactory.CreateLogger<ClaimsToDownstreamPathMiddleware>())
        {
            _next = next;
            _changeDownstreamPathTemplate = changeDownstreamPathTemplate;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var downstreamRoute = httpContext.Items.DownstreamRoute();

            if (downstreamRoute.ClaimsToPath.Any())
            {
                Logger.LogInformation($"{downstreamRoute.DownstreamPathTemplate.Value} has instructions to convert claims to path");

                var templatePlaceholderNameAndValues = httpContext.Items.TemplatePlaceholderNameAndValues();

                var response = _changeDownstreamPathTemplate.ChangeDownstreamPath(downstreamRoute.ClaimsToPath, httpContext.User.Claims,
                    downstreamRoute.DownstreamPathTemplate, templatePlaceholderNameAndValues);

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

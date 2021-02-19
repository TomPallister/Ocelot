namespace Ocelot.Authorization.Middleware
{
    using Ocelot.Configuration;
    using Ocelot.Logging;
    using Ocelot.Middleware;
    using Ocelot.Responses;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Ocelot.DownstreamRouteFinder.Middleware;

    public class AuthorizationMiddleware : OcelotMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IClaimsAuthorizer _claimsAuthorizer;
        private readonly IScopesAuthorizer _scopesAuthorizer;

        public AuthorizationMiddleware(RequestDelegate next,
            IClaimsAuthorizer claimsAuthorizer,
            IScopesAuthorizer scopesAuthorizer,
            IOcelotLoggerFactory loggerFactory)
            : base(loggerFactory.CreateLogger<AuthorizationMiddleware>())
        {
            _next = next;
            _claimsAuthorizer = claimsAuthorizer;
            _scopesAuthorizer = scopesAuthorizer;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var downstreamRoute = httpContext.Items.DownstreamRoute();

            if (!IsOptionsHttpMethod(httpContext) && IsAuthenticatedRoute(downstreamRoute))
            {
                Logger.LogInformation("route is authenticated scopes must be checked");

                var authorized = _scopesAuthorizer.Authorize(httpContext.User, downstreamRoute.AuthenticationOptions.AllowedScopes);

                if (authorized.IsError)
                {
                    Logger.LogWarning("error authorizing user scopes");

                    httpContext.Items.UpsertErrors(authorized.Errors);
                    return;
                }

                if (IsAuthorized(authorized))
                {
                    Logger.LogInformation("user scopes is authorized calling next authorization checks");
                }
                else
                {
                    Logger.LogWarning("user scopes is not authorized setting pipeline error");

                    httpContext.Items.SetError(new UnauthorizedError(
                            $"{httpContext.User.Identity.Name} unable to access {downstreamRoute.UpstreamPathTemplate.OriginalValue}"));
                }
            }

            if (!IsOptionsHttpMethod(httpContext) && IsAuthorizedRoute(downstreamRoute))
            {
                Logger.LogInformation("route is authorized");

                var authorized = _claimsAuthorizer.Authorize(httpContext.User, downstreamRoute.RouteClaimsRequirement, httpContext.Items.TemplatePlaceholderNameAndValues());

                if (authorized.IsError)
                {
                    Logger.LogWarning($"Error whilst authorizing {httpContext.User.Identity.Name}. Setting pipeline error");

                    httpContext.Items.UpsertErrors(authorized.Errors);
                    return;
                }

                if (IsAuthorized(authorized))
                {
                    Logger.LogInformation($"{httpContext.User.Identity.Name} has succesfully been authorized for {downstreamRoute.UpstreamPathTemplate.OriginalValue}.");
                    await _next.Invoke(httpContext);
                }
                else
                {
                    Logger.LogWarning($"{httpContext.User.Identity.Name} is not authorized to access {downstreamRoute.UpstreamPathTemplate.OriginalValue}. Setting pipeline error");

                    httpContext.Items.SetError(new UnauthorizedError($"{httpContext.User.Identity.Name} is not authorized to access {downstreamRoute.UpstreamPathTemplate.OriginalValue}"));
                }
            }
            else
            {
                Logger.LogInformation($"{downstreamRoute.DownstreamPathTemplate.Value} route does not require user to be authorized");
                await _next.Invoke(httpContext);
            }
        }

        private static bool IsAuthorized(Response<bool> authorized)
        {
            return authorized.Data;
        }

        private static bool IsAuthenticatedRoute(DownstreamRoute route)
        {
            return route.IsAuthenticated;
        }

        private static bool IsAuthorizedRoute(DownstreamRoute route)
        {
            return route.IsAuthorized;
        }

        private static bool IsOptionsHttpMethod(HttpContext httpContext)
        {
            return httpContext.Request.Method.ToUpper() == "OPTIONS";
        }
    }
}

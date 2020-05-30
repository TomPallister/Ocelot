using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Ocelot.Configuration;
using Ocelot.Logging;
using Ocelot.Middleware;

namespace Ocelot.Authentication.Middleware
{
    public class AuthenticationMiddleware : OcelotMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _memoryCache;

        public AuthenticationMiddleware(RequestDelegate next,
            IOcelotLoggerFactory loggerFactory, IMemoryCache memoryCache)
            : base(loggerFactory.CreateLogger<AuthenticationMiddleware>())
        {
            _next = next;
            _memoryCache = memoryCache;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var downstreamRoute = httpContext.Items.DownstreamRoute();

            if (httpContext.Request.Method.ToUpper() != "OPTIONS" && IsAuthenticatedRoute(downstreamRoute))
            {
                Logger.LogInformation(
                    $"{httpContext.Request.Path} is an authenticated route. {MiddlewareName} checking if client is authenticated");

                var token = httpContext.Request.Headers
                    .FirstOrDefault(r => r.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase));

                var cacheKey = $"identityToken.{token}";

                if (!_memoryCache.TryGetValue(cacheKey, out ClaimsPrincipal userClaim))
                {
                    var result =
                        await httpContext.AuthenticateAsync(downstreamRoute.AuthenticationOptions
                            .AuthenticationProviderKey);

                    userClaim = result.Principal;

                    _memoryCache.Set(cacheKey, userClaim, TimeSpan.FromMinutes(10));
                }

                httpContext.User = userClaim;

                if (httpContext.User.Identity.IsAuthenticated)
                {
                    Logger.LogInformation($"Client has been authenticated for {httpContext.Request.Path}");
                    await _next.Invoke(httpContext);
                }
                else
                {
                    var error = new UnauthenticatedError(
                        $"Request for authenticated route {httpContext.Request.Path} by {httpContext.User.Identity.Name} was unauthenticated");

                    Logger.LogWarning(
                        $"Client has NOT been authenticated for {httpContext.Request.Path} and pipeline error set. {error}");

                    httpContext.Items.SetError(error);
                }
            }
            else
            {
                Logger.LogInformation($"No authentication needed for {httpContext.Request.Path}");

                await _next.Invoke(httpContext);
            }
        }

        private static bool IsAuthenticatedRoute(DownstreamRoute route)
        {
            return route.IsAuthenticated;
        }
    }
}

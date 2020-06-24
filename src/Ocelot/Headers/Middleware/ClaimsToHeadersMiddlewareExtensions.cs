using Microsoft.AspNetCore.Builder;

namespace Ocelot.Headers.Middleware
{
    public static class ClaimsToHeadersMiddlewareExtensions
    {
        public static IApplicationBuilder UseClaimsToHeadersMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ClaimsToHeadersMiddleware>();
        }
    }
}

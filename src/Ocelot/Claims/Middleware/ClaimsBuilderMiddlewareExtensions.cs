namespace Ocelot.Claims.Middleware
{
    using Microsoft.AspNetCore.Builder;

    public static class ClaimsBuilderMiddlewareExtensions
    {
        public static IApplicationBuilder UseClaimsToClaimsMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ClaimsToClaimsMiddleware>();
        }
    }
}

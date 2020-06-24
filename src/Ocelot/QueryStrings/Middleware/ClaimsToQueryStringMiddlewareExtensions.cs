namespace Ocelot.QueryStrings.Middleware
{
    using Microsoft.AspNetCore.Builder;

    public static class ClaimsToQueryStringMiddlewareExtensions
    {
        public static IApplicationBuilder UseClaimsToQueryStringMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ClaimsToQueryStringMiddleware>();
        }
    }
}

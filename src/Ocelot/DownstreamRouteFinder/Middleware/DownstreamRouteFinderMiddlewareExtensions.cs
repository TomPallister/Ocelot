namespace Ocelot.DownstreamRouteFinder.Middleware
{
    using Microsoft.AspNetCore.Builder;

    public static class DownstreamRouteFinderMiddlewareExtensions
    {
        public static IApplicationBuilder UseDownstreamRouteFinderMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<DownstreamRouteFinderMiddleware>();
        }
    }
}

namespace Ocelot.Cache.Middleware
{
    using Microsoft.AspNetCore.Builder;

    public static class OutputCacheMiddlewareExtensions
    {
        public static IApplicationBuilder UseOutputCacheMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<OutputCacheMiddleware>();
        }
    }
}

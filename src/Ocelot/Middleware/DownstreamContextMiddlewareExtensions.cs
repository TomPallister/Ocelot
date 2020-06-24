namespace Ocelot.Middleware
{
    using Microsoft.AspNetCore.Builder;

    public static class DownstreamContextMiddlewareExtensions
    {
        public static IApplicationBuilder UseDownstreamContextMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ConfigurationMiddleware>();
        }
    }
}

namespace Ocelot.Multiplexer
{
    using Microsoft.AspNetCore.Builder;

    public static class MultiplexingMiddlewareExtensions
    {
        public static IApplicationBuilder UseMultiplexingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MultiplexingMiddleware>();
        }
    }
}

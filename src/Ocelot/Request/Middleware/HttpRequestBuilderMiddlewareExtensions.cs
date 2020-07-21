namespace Ocelot.Request.Middleware
{
    using Microsoft.AspNetCore.Builder;

    public static class HttpRequestBuilderMiddlewareExtensions
    {
        public static IApplicationBuilder UseDownstreamRequestInitialiser(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<DownstreamRequestInitialiserMiddleware>();
        }
    }
}

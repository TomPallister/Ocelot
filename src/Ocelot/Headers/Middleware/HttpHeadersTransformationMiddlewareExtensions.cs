namespace Ocelot.Headers.Middleware
{
    using Microsoft.AspNetCore.Builder;

    public static class HttpHeadersTransformationMiddlewareExtensions
    {
        public static IApplicationBuilder UseHttpHeadersTransformationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<HttpHeadersTransformationMiddleware>();
        }
    }
}

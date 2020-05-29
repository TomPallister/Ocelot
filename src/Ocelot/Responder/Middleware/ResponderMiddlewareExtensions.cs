namespace Ocelot.Responder.Middleware
{
    using Microsoft.AspNetCore.Builder;

    public static class ResponderMiddlewareExtensions
    {
        public static IApplicationBuilder UseResponderMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ResponderMiddleware>();
        }
    }
}

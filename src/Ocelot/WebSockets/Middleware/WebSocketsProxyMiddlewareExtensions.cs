namespace Ocelot.WebSockets.Middleware
{
    using Microsoft.AspNetCore.Builder;

    public static class WebSocketsProxyMiddlewareExtensions
    {
        public static IApplicationBuilder UseWebSocketsProxyMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<WebSocketsProxyMiddleware>();
        }
    }
}

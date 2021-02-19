namespace Ocelot.Authorization.Middleware
{
    using Microsoft.AspNetCore.Builder;

    public static class AuthorizationMiddlewareMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthorizationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthorizationMiddleware>();
        }
    }
}

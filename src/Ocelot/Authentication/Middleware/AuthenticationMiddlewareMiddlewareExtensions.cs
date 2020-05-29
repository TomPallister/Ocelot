namespace Ocelot.Authentication.Middleware
{
    using Microsoft.AspNetCore.Builder;

    public static class AuthenticationMiddlewareMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthenticationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthenticationMiddleware>();
        }
    }
}

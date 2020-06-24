namespace Ocelot.Authorisation.Middleware
{
    using Microsoft.AspNetCore.Builder;

    public static class AuthorisationMiddlewareMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthorisationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthorisationMiddleware>();
        }
    }
}

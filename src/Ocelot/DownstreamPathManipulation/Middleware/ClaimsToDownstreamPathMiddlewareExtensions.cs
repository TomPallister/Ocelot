namespace Ocelot.DownstreamPathManipulation.Middleware
{
    using Microsoft.AspNetCore.Builder;

    public static class ClaimsToDownstreamPathMiddlewareExtensions
    {
        public static IApplicationBuilder UseClaimsToDownstreamPathMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ClaimsToDownstreamPathMiddleware>();
        }
    }
}

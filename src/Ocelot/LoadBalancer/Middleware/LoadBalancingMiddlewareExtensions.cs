namespace Ocelot.LoadBalancer.Middleware
{
    using Microsoft.AspNetCore.Builder;

    public static class LoadBalancingMiddlewareExtensions
    {
        public static IApplicationBuilder UseLoadBalancingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LoadBalancingMiddleware>();
        }
    }
}

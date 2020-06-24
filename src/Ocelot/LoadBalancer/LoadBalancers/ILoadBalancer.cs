namespace Ocelot.LoadBalancer.LoadBalancers
{
    using Microsoft.AspNetCore.Http;
    using Ocelot.Responses;
    using Ocelot.Values;
    using System.Threading.Tasks;

    public interface ILoadBalancer
    {
        Task<Response<ServiceHostAndPort>> Lease(HttpContext httpContext);

        void Release(ServiceHostAndPort hostAndPort);
    }
}

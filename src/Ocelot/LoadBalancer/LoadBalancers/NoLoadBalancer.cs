namespace Ocelot.LoadBalancer.LoadBalancers
{
    using Microsoft.AspNetCore.Http;
    using Ocelot.Middleware;
    using Ocelot.Responses;
    using Ocelot.Values;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class NoLoadBalancer : ILoadBalancer
    {
        private readonly Func<Task<List<Service>>> _services;

        public NoLoadBalancer(Func<Task<List<Service>>> services)
        {
            _services = services;
        }

        public async Task<Response<ServiceHostAndPort>> Lease(HttpContext httpContext)
        {
            var services = await _services();

            if (services == null || services.Count == 0)
            {
                return new ErrorResponse<ServiceHostAndPort>(new ServicesAreEmptyError("There were no services in NoLoadBalancer"));
            }

            var service = await Task.FromResult(services.FirstOrDefault());
            return new OkResponse<ServiceHostAndPort>(service.HostAndPort);
        }

        public void Release(ServiceHostAndPort hostAndPort)
        {
        }
    }
}

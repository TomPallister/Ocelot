namespace Ocelot.LoadBalancer.LoadBalancers
{
    using Ocelot.Middleware;
    using Ocelot.Responses;
    using Ocelot.Values;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    public class RoundRobin : ILoadBalancer
    {
        private readonly Func<Task<List<Service>>> _services;
        private readonly object _lock = new object();

        private int _last;

        public RoundRobin(Func<Task<List<Service>>> services)
        {
            _services = services;
        }

        public async Task<Response<ServiceHostAndPort>> Lease(HttpContext httpContext)
        {
            var services = await _services();
            lock (_lock)
            {
                if (services.Count < 1)
                {
                    //When the downstream service is not found, LeastConnection prompts Warn and RoundRobin throws an exception。eg:/favicon.ico
                    return new ErrorResponse<ServiceHostAndPort>(new ServicesAreEmptyError($"services were empty for {httpContext.Request.Path}"));
                }

                if (_last >= services.Count)
                {
                    _last = 0;
                }

                var next = services[_last];
                _last++;
                return new OkResponse<ServiceHostAndPort>(next.HostAndPort);
            }
        }

        public void Release(ServiceHostAndPort hostAndPort)
        {
        }
    }
}

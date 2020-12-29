namespace Ocelot.LoadBalancer.LoadBalancers
{
    using Ocelot.Infrastructure;
    using Ocelot.Middleware;
    using Ocelot.Responses;
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Ocelot.Values;

    public class CookieStickySessions : ILoadBalancer
    {
        private readonly int _keyExpiryInMs;
        private readonly string _key;
        private readonly ILoadBalancer _loadBalancer;
        private readonly IStickySessionStorage _storage;
        private readonly IBus<StickySession> _bus;
        private readonly object _lock = new object();

        public CookieStickySessions(ILoadBalancer loadBalancer, string key, int keyExpiryInMs, IBus<StickySession> bus, IStickySessionStorage storage)
        {
            _bus = bus;
            _key = key;
            _keyExpiryInMs = keyExpiryInMs;
            _loadBalancer = loadBalancer;
            _storage = storage;
            _bus.Subscribe(ss =>
            {
                //todo - get test coverage for this.
                if (_storage.TryGetSession(ss.Key, out var stickySession))
                {
                    lock (_lock)
                    {
                        if (stickySession.Expiry < DateTime.UtcNow)
                        {
                            _storage.TryRemove(stickySession.Key, out _);
                            _loadBalancer.Release(stickySession.HostAndPort);
                        }
                    }
                }
            });
        }

        public async Task<Response<ServiceHostAndPort>> Lease(HttpContext httpContext)
        {
            var key = httpContext.Request.Cookies[_key];

            lock (_lock)
            {
                if (!string.IsNullOrEmpty(key) && _storage.Contains(key))
                {
                    var cached = _storage.GetSession(key);

                    var updated = new StickySession(cached.HostAndPort, DateTime.UtcNow.AddMilliseconds(_keyExpiryInMs), key);

                    _storage.SetSession(key, updated);

                    _bus.Publish(updated, _keyExpiryInMs);

                    return new OkResponse<ServiceHostAndPort>(updated.HostAndPort);
                }
            }

            var next = await _loadBalancer.Lease(httpContext);

            if (next.IsError)
            {
                return new ErrorResponse<ServiceHostAndPort>(next.Errors);
            }

            lock (_lock)
            {
                if (!string.IsNullOrEmpty(key) && !_storage.Contains(key))
                {
                    var ss = new StickySession(next.Data, DateTime.UtcNow.AddMilliseconds(_keyExpiryInMs), key);
                    _storage.SetSession(key, ss);
                    _bus.Publish(ss, _keyExpiryInMs);
                }
            }

            return new OkResponse<ServiceHostAndPort>(next.Data);
        }

        public void Release(ServiceHostAndPort hostAndPort)
        {
        }
    }
}

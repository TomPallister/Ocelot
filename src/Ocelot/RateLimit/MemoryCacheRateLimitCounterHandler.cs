using Microsoft.Extensions.Caching.Memory;
using System;

namespace Ocelot.RateLimit
{
    public class MemoryCacheRateLimitCounterHandler : IRateLimitCounterHandler
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheRateLimitCounterHandler(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public void Set(string id, RateLimitCounter counter, TimeSpan expirationTime)
        {
            _memoryCache.Set(id, counter, new MemoryCacheEntryOptions().SetAbsoluteExpiration(expirationTime));
        }

        public bool Exists(string id)
        {
            RateLimitCounter counter;
            return _memoryCache.TryGetValue(id, out counter);
        }

        public RateLimitCounter? Get(string id)
        {
            RateLimitCounter counter;
            if (_memoryCache.TryGetValue(id, out counter))
            {
                return counter;
            }

            return null;
        }

        public void Remove(string id)
        {
            _memoryCache.Remove(id);
        }
    }
}

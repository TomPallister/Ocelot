namespace Ocelot.Cache
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Caching.Memory;

    public class AspMemoryCache<T> : IOcelotCache<T>
    {
        private readonly IMemoryCache _memoryCache;
        private readonly Dictionary<string, List<string>> _regions;

        public AspMemoryCache(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            _regions = new Dictionary<string, List<string>>();
        }

        public void Add(string key, T value, TimeSpan ttl, string region)
        {
            if (ttl.TotalMilliseconds <= 0)
            {
                return;
            }

            _memoryCache.Set(key, value, ttl);

            SetRegion(region, key);
        }

        public T Get(string key, string region)
        {   
            if (_memoryCache.TryGetValue(key, out T value))
            {
                return value;
            }

            return default(T);
        }

        public void ClearRegion(string region)
        {
            if (_regions.ContainsKey(region))
            {
                var keys = _regions[region];
                foreach (var key in keys)
                {
                    _memoryCache.Remove(key);
                }
            }
        }

        public void AddAndDelete(string key, T value, TimeSpan ttl, string region)
        {
            if (_memoryCache.TryGetValue(key, out T oldValue))
            {
                _memoryCache.Remove(key);
            }

            Add(key, value, ttl, region);
        }

        private void SetRegion(string region, string key)
        {
            if (_regions.ContainsKey(region))
            {
                var current = _regions[region];
                if (!current.Contains(key))
                {
                    current.Add(key);
                }
            }
            else
            {
                _regions.Add(region, new List<string> { key });
            }
        }
    }
}

namespace Ocelot.AcceptanceTests.Caching
{
    using CacheManager.Core;
    using CacheManager.Core.Internal;
    using CacheManager.Core.Logging;
    using CacheManager.Core.Utility;
    using System;
    using System.Collections.Concurrent;
    using System.Linq;

    public class InMemoryJsonHandle<TCacheValue> : BaseCacheHandle<TCacheValue>
    {
        private readonly ICacheSerializer _serializer;
        private readonly ConcurrentDictionary<string, Tuple<Type, byte[]>> _cache;

        public InMemoryJsonHandle(
            ICacheManagerConfiguration managerConfiguration,
            CacheHandleConfiguration configuration,
            ICacheSerializer serializer,
            ILoggerFactory loggerFactory) : base(managerConfiguration, configuration)
        {
            _cache = new ConcurrentDictionary<string, Tuple<Type, byte[]>>();
            _serializer = serializer;
            Logger = loggerFactory.CreateLogger(this);
        }

        public override int Count => _cache.Count;

        protected override ILogger Logger { get; }

        public override void Clear() => _cache.Clear();

        public override void ClearRegion(string region)
        {
            Guard.NotNullOrWhiteSpace(region, nameof(region));

            var key = string.Concat(region, ":");
            foreach (var item in _cache.Where(p => p.Key.StartsWith(key, StringComparison.OrdinalIgnoreCase)))
            {
                _cache.TryRemove(item.Key, out Tuple<Type, byte[]> val);
            }
        }

        public override bool Exists(string key)
        {
            Guard.NotNullOrWhiteSpace(key, nameof(key));

            return _cache.ContainsKey(key);
        }

        public override bool Exists(string key, string region)
        {
            Guard.NotNullOrWhiteSpace(region, nameof(region));
            var fullKey = GetKey(key, region);
            return _cache.ContainsKey(fullKey);
        }

        protected override bool AddInternalPrepared(CacheItem<TCacheValue> item)
        {
            Guard.NotNull(item, nameof(item));

            var key = GetKey(item.Key, item.Region);

            var serializedItem = _serializer.SerializeCacheItem(item);

            return _cache.TryAdd(key, new Tuple<Type, byte[]>(item.Value.GetType(), serializedItem));
        }

        protected override CacheItem<TCacheValue> GetCacheItemInternal(string key) => GetCacheItemInternal(key, null);

        protected override CacheItem<TCacheValue> GetCacheItemInternal(string key, string region)
        {
            var fullKey = GetKey(key, region);

            CacheItem<TCacheValue> deserializedResult = null;

            if (_cache.TryGetValue(fullKey, out Tuple<Type, byte[]> result))
            {
                deserializedResult = _serializer.DeserializeCacheItem<TCacheValue>(result.Item2, result.Item1);

                if (deserializedResult.ExpirationMode != ExpirationMode.None && IsExpired(deserializedResult, DateTime.UtcNow))
                {
                    _cache.TryRemove(fullKey, out Tuple<Type, byte[]> removeResult);
                    TriggerCacheSpecificRemove(key, region, CacheItemRemovedReason.Expired, deserializedResult.Value);
                    return null;
                }
            }

            return deserializedResult;
        }

        protected override void PutInternalPrepared(CacheItem<TCacheValue> item)
        {
            Guard.NotNull(item, nameof(item));

            var serializedItem = _serializer.SerializeCacheItem<TCacheValue>(item);

            _cache[GetKey(item.Key, item.Region)] = new Tuple<Type, byte[]>(item.Value.GetType(), serializedItem);
        }

        protected override bool RemoveInternal(string key) => RemoveInternal(key, null);

        protected override bool RemoveInternal(string key, string region)
        {
            var fullKey = GetKey(key, region);
            return _cache.TryRemove(fullKey, out Tuple<Type, byte[]> val);
        }

        private static string GetKey(string key, string region)
        {
            Guard.NotNullOrWhiteSpace(key, nameof(key));

            if (string.IsNullOrWhiteSpace(region))
            {
                return key;
            }

            return string.Concat(region, ":", key);
        }

        private static bool IsExpired(CacheItem<TCacheValue> item, DateTime now)
        {
            if (item.ExpirationMode == ExpirationMode.Absolute
                && item.CreatedUtc.Add(item.ExpirationTimeout) < now)
            {
                return true;
            }
            else if (item.ExpirationMode == ExpirationMode.Sliding
                && item.LastAccessedUtc.Add(item.ExpirationTimeout) < now)
            {
                return true;
            }

            return false;
        }
    }
}

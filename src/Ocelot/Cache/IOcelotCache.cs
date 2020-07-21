using System;

namespace Ocelot.Cache
{
    public interface IOcelotCache<T>
    {
        void Add(string key, T value, TimeSpan ttl, string region);

        T Get(string key, string region);

        void ClearRegion(string region);

        void AddAndDelete(string key, T value, TimeSpan ttl, string region);
    }
}

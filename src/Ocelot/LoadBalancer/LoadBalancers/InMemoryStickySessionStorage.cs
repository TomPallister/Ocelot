using System.Collections.Concurrent;

namespace Ocelot.LoadBalancer.LoadBalancers
{
    public class InMemoryStickySessionStorage : IStickySessionStorage
    {
        private readonly ConcurrentDictionary<string, StickySession> _storage;

        public InMemoryStickySessionStorage()
        {
            _storage = new ConcurrentDictionary<string, StickySession>();
        }

        public bool TryGetSession(string key, out StickySession session)
        {
            return _storage.TryGetValue(key, out session);
        }

        public StickySession GetSession(string key)
        {
            return _storage.TryGetValue(key, out var session) ? session : null;
        }

        public void SetSession(string key, StickySession session)
        {
            _storage[key] = session;
        }

        public bool TryRemove(string key, out StickySession session)
        {
            return _storage.TryRemove(key, out session);
        }

        public bool Contains(string key)
        {
            return _storage.ContainsKey(key);
        }
    }
}

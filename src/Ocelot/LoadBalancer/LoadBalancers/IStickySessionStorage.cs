namespace Ocelot.LoadBalancer.LoadBalancers
{
    public interface IStickySessionStorage
    {
        bool TryGetSession(string key, out StickySession session);
        bool TryRemove(string key, out StickySession session);
        StickySession GetSession(string key);
        void SetSession(string key, StickySession session);
        bool Contains(string key);
    }
}

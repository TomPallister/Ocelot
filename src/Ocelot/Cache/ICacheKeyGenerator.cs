namespace Ocelot.Cache
{
    using Ocelot.Request.Middleware;

    public interface ICacheKeyGenerator
    {
        string GenerateRequestCacheKey(DownstreamRequest downstreamRequest);
    }
}

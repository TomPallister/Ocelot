namespace Ocelot.Cache.Middleware
{
    using Ocelot.Logging;
    using Ocelot.Middleware;
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    public class OutputCacheMiddleware : OcelotMiddleware
    {
        private readonly OcelotRequestDelegate _next;
        private readonly IOcelotCache<CachedResponse> _outputCache;
        private readonly ICacheKeyGenerator _cacheGeneratot;

        public OutputCacheMiddleware(OcelotRequestDelegate next,
            IOcelotLoggerFactory loggerFactory,
            IOcelotCache<CachedResponse> outputCache,
            ICacheKeyGenerator cacheGeneratot)
                : base(loggerFactory.CreateLogger<OutputCacheMiddleware>())
        {
            _next = next;
            _outputCache = outputCache;
            _cacheGeneratot = cacheGeneratot;
        }

        public async Task Invoke(DownstreamContext context)
        {
            if (!context.DownstreamReRoute.IsCached)
            {
                await _next.Invoke(context);
                return;
            }

            var downstreamUrlKey = $"{context.DownstreamRequest.Method}-{context.DownstreamRequest.OriginalString}";
            string downStreamRequestCacheKey = _cacheGeneratot.GenerateRequestCacheKey(context);

            Logger.LogDebug($"Started checking cache for {downstreamUrlKey}");

            var cached = _outputCache.Get(downStreamRequestCacheKey, context.DownstreamReRoute.CacheOptions.Region);

            if (cached != null)
            {
                Logger.LogDebug($"cache entry exists for {downstreamUrlKey}");

                var response = CreateHttpResponseMessage(cached);
                SetHttpResponseMessageThisRequest(context, response);

                Logger.LogDebug($"finished returned cached response for {downstreamUrlKey}");

                return;
            }

            Logger.LogDebug($"no resonse cached for {downstreamUrlKey}");

            await _next.Invoke(context);

            if (context.IsError)
            {
                Logger.LogDebug($"there was a pipeline error for {downstreamUrlKey}");

                return;
            }

            if (context.DownstreamResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                cached = await CreateCachedResponse(context.DownstreamResponse);

                _outputCache.Add(downStreamRequestCacheKey, cached, TimeSpan.FromSeconds(context.DownstreamReRoute.CacheOptions.TtlSeconds), context.DownstreamReRoute.CacheOptions.Region);

                Logger.LogDebug($"finished response added to cache for {downstreamUrlKey}");
            }
            else
            {

                Logger.LogDebug($"http request failed,  could not create cache for {downstreamUrlKey}");
            }
        }

        private void SetHttpResponseMessageThisRequest(DownstreamContext context,
                                                       DownstreamResponse response)
        {
            context.DownstreamResponse = response;
        }

        internal DownstreamResponse CreateHttpResponseMessage(CachedResponse cached)
        {
            if (cached == null)
            {
                return null;
            }

            var content = new MemoryStream(Convert.FromBase64String(cached.Body));

            var streamContent = new StreamContent(content);

            foreach (var header in cached.ContentHeaders)
            {
                streamContent.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return new DownstreamResponse(streamContent, cached.StatusCode, cached.Headers.ToList(), cached.ReasonPhrase);
        }

        internal async Task<CachedResponse> CreateCachedResponse(DownstreamResponse response)
        {
            if (response == null)
            {
                return null;
            }

            var statusCode = response.StatusCode;
            var headers = response.Headers.ToDictionary(v => v.Key, v => v.Values);
            string body = null;

            if (response.Content != null)
            {
                var content = await response.Content.ReadAsByteArrayAsync();
                body = Convert.ToBase64String(content);
            }

            var contentHeaders = response?.Content?.Headers.ToDictionary(v => v.Key, v => v.Value);

            var cached = new CachedResponse(statusCode, headers, body, contentHeaders, response.ReasonPhrase);
            return cached;
        }
    }
}

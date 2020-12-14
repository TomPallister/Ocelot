namespace Ocelot.RateLimit.Middleware
{
    using Microsoft.AspNetCore.Http;
    using Ocelot.Configuration;
    using Ocelot.DownstreamRouteFinder.Middleware;
    using Ocelot.Logging;
    using Ocelot.Middleware;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class ClientRateLimitMiddleware : OcelotMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ClientRateLimitProcessor _processor;
        private IHttpContextAccessor _httpContextAccessor;

        public ClientRateLimitMiddleware(RequestDelegate next,
            IOcelotLoggerFactory loggerFactory,
            IRateLimitCounterHandler counterHandler, 
            IHttpContextAccessor httpContextAccessor)
                : base(loggerFactory.CreateLogger<ClientRateLimitMiddleware>())
        {
            _next = next;
            _processor = new ClientRateLimitProcessor(counterHandler);
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var downstreamRoute = httpContext.Items.DownstreamRoute();

            var options = downstreamRoute.RateLimitOptions;

            // check if rate limiting is enabled
            if (!downstreamRoute.EnableEndpointEndpointRateLimiting)
            {
                Logger.LogInformation($"EndpointRateLimiting is not enabled for {downstreamRoute.DownstreamPathTemplate.Value}");
                await _next.Invoke(httpContext);
                return;
            }

            // compute identity from request
            var identity = SetIdentity(httpContext, options);

            // check white list
            if (IsWhitelisted(identity, options))
            {
                Logger.LogInformation($"{downstreamRoute.DownstreamPathTemplate.Value} is white listed from rate limiting");
                await _next.Invoke(httpContext);
                return;
            }

            var rule = options.RateLimitRule;
            if (rule.Limit > 0)
            {
                // increment counter
                var counter = _processor.ProcessRequest(identity, options);

                // check if limit is reached
                if (counter.TotalRequests > rule.Limit)
                {
                    //compute retry after value
                    var retryAfter = _processor.RetryAfterFrom(counter.Timestamp, rule);

                    // log blocked request
                    LogBlockedRequest(httpContext, identity, counter, rule, downstreamRoute);

                    var retrystring = retryAfter.ToString(System.Globalization.CultureInfo.InvariantCulture);

                    // break execution
                    var ds = ReturnQuotaExceededResponse(httpContext, options, retrystring);
                    httpContext.Items.UpsertDownstreamResponse(ds);

                    // Set Error
                    httpContext.Items.SetError(new QuotaExceededError(this.GetResponseMessage(options), options.HttpStatusCode));

                    return;
                }
            }

            //set X-Rate-Limit headers for the longest period
            if (!options.DisableRateLimitHeaders)
            {
                var originalHttpContext = _httpContextAccessor.HttpContext;
                var headers = _processor.GetRateLimitHeaders(originalHttpContext, identity, options);
                originalHttpContext.Response.OnStarting(SetRateLimitHeaders, state: headers);
            }

            await _next.Invoke(httpContext);
        }

        public virtual ClientRequestIdentity SetIdentity(HttpContext httpContext, RateLimitOptions option)
        {
            var clientId = "client";
            if (httpContext.Request.Headers.Keys.Contains(option.ClientIdHeader))
            {
                clientId = httpContext.Request.Headers[option.ClientIdHeader].First();
            }

            return new ClientRequestIdentity(
                clientId,
                httpContext.Request.Path.ToString().ToLowerInvariant(),
                httpContext.Request.Method.ToLowerInvariant()
                );
        }

        public bool IsWhitelisted(ClientRequestIdentity requestIdentity, RateLimitOptions option)
        {
            if (option.ClientWhitelist.Contains(requestIdentity.ClientId))
            {
                return true;
            }

            return false;
        }

        public virtual void LogBlockedRequest(HttpContext httpContext, ClientRequestIdentity identity, RateLimitCounter counter, RateLimitRule rule, DownstreamRoute downstreamRoute)
        {
            Logger.LogInformation(
                $"Request {identity.HttpVerb}:{identity.Path} from ClientId {identity.ClientId} has been blocked, quota {rule.Limit}/{rule.Period} exceeded by {counter.TotalRequests}. Blocked by rule { downstreamRoute.UpstreamPathTemplate.OriginalValue }, TraceIdentifier {httpContext.TraceIdentifier}.");
        }

        public virtual DownstreamResponse ReturnQuotaExceededResponse(HttpContext httpContext, RateLimitOptions option, string retryAfter)
        {
            var message = GetResponseMessage(option);

            var http = new HttpResponseMessage((HttpStatusCode)option.HttpStatusCode);

            http.Content = new StringContent(message);

            if (!option.DisableRateLimitHeaders)
            {
                http.Headers.TryAddWithoutValidation("Retry-After", retryAfter);
            }

            return new DownstreamResponse(http);
        }

        private string GetResponseMessage(RateLimitOptions option)
        {
            var message = string.IsNullOrEmpty(option.QuotaExceededMessage)
                ? $"API calls quota exceeded! maximum admitted {option.RateLimitRule.Limit} per {option.RateLimitRule.Period}."
                : option.QuotaExceededMessage;
            return message;
        }

        private Task SetRateLimitHeaders(object rateLimitHeaders)
        {
            var headers = (RateLimitHeaders)rateLimitHeaders;

            headers.Context.Response.Headers["X-Rate-Limit-Limit"] = headers.Limit;
            headers.Context.Response.Headers["X-Rate-Limit-Remaining"] = headers.Remaining;
            headers.Context.Response.Headers["X-Rate-Limit-Reset"] = headers.Reset;

            return Task.CompletedTask;
        }
    }
}

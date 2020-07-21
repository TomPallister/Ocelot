using Microsoft.AspNetCore.Http;
using Ocelot.Configuration;
using System;

namespace Ocelot.RateLimit
{
    public class ClientRateLimitProcessor
    {
        private readonly IRateLimitCounterHandler _counterHandler;
        private readonly RateLimitCore _core;

        public ClientRateLimitProcessor(IRateLimitCounterHandler counterHandler)
        {
            _counterHandler = counterHandler;
            _core = new RateLimitCore(_counterHandler);
        }

        public RateLimitCounter ProcessRequest(ClientRequestIdentity requestIdentity, RateLimitOptions option)
        {
            return _core.ProcessRequest(requestIdentity, option);
        }

        public int RetryAfterFrom(DateTime timestamp, RateLimitRule rule)
        {
            return _core.RetryAfterFrom(timestamp, rule);
        }

        public RateLimitHeaders GetRateLimitHeaders(HttpContext context, ClientRequestIdentity requestIdentity, RateLimitOptions option)
        {
            return _core.GetRateLimitHeaders(context, requestIdentity, option);
        }

        public TimeSpan ConvertToTimeSpan(string timeSpan)
        {
            return _core.ConvertToTimeSpan(timeSpan);
        }
    }
}

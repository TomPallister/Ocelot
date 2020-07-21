using Newtonsoft.Json;
using System;

namespace Ocelot.RateLimit
{
    /// <summary>
    /// Stores the initial access time and the numbers of calls made from that point
    /// </summary>
    public struct RateLimitCounter
    {
        [JsonConstructor]
        public RateLimitCounter(DateTime timestamp, long totalRequests)
        {
            Timestamp = timestamp;
            TotalRequests = totalRequests;
        }

        public DateTime Timestamp { get; private set; }

        public long TotalRequests { get; private set; }
    }
}

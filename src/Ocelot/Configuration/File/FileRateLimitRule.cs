using Ocelot.Infrastructure.Extensions;
using System.Collections.Generic;
using System.Text;

namespace Ocelot.Configuration.File
{
    public class FileRateLimitRule
    {
        public FileRateLimitRule()
        {
            ClientWhitelist = new List<string>();
        }

        public List<string> ClientWhitelist { get; set; }

        /// <summary>
        /// Enables endpoint rate limiting based URL path and HTTP verb
        /// </summary>
        public bool EnableRateLimiting { get; set; }

        /// <summary>
        /// Rate limit period as in 1s, 1m, 1h
        /// </summary>
        public string Period { get; set; }

        public double PeriodTimespan { get; set; }

        /// <summary>
        /// Maximum number of requests that a client can make in a defined period
        /// </summary>
        public long Limit { get; set; }

        public override string ToString()
        {
            if (!EnableRateLimiting)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            sb.Append(
                $"{nameof(Period)}:{Period},{nameof(PeriodTimespan)}:{PeriodTimespan:F},{nameof(Limit)}:{Limit},{nameof(ClientWhitelist)}:[");

            sb.AppendJoin(',', ClientWhitelist);
            sb.Append(']');
            return sb.ToString();
        }
    }
}

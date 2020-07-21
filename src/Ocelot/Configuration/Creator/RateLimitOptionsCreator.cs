using Ocelot.Configuration.Builder;
using Ocelot.Configuration.File;

namespace Ocelot.Configuration.Creator
{
    public class RateLimitOptionsCreator : IRateLimitOptionsCreator
    {
        public RateLimitOptions Create(FileRateLimitRule fileRateLimitRule, FileGlobalConfiguration globalConfiguration)
        {
            if (fileRateLimitRule != null && fileRateLimitRule.EnableRateLimiting)
            {
                return new RateLimitOptionsBuilder()
                    .WithClientIdHeader(globalConfiguration.RateLimitOptions.ClientIdHeader)
                    .WithClientWhiteList(() => fileRateLimitRule.ClientWhitelist)
                    .WithDisableRateLimitHeaders(globalConfiguration.RateLimitOptions.DisableRateLimitHeaders)
                    .WithEnableRateLimiting(fileRateLimitRule.EnableRateLimiting)
                    .WithHttpStatusCode(globalConfiguration.RateLimitOptions.HttpStatusCode)
                    .WithQuotaExceededMessage(globalConfiguration.RateLimitOptions.QuotaExceededMessage)
                    .WithRateLimitCounterPrefix(globalConfiguration.RateLimitOptions.RateLimitCounterPrefix)
                    .WithRateLimitRule(new RateLimitRule(fileRateLimitRule.Period,
                        fileRateLimitRule.PeriodTimespan,
                        fileRateLimitRule.Limit))
                    .Build();
            }

            return new RateLimitOptionsBuilder().WithEnableRateLimiting(false).Build();
        }
    }
}

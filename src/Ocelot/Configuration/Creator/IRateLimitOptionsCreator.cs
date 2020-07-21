using Ocelot.Configuration.File;

namespace Ocelot.Configuration.Creator
{
    public interface IRateLimitOptionsCreator
    {
        RateLimitOptions Create(FileRateLimitRule fileRateLimitRule, FileGlobalConfiguration globalConfiguration);
    }
}

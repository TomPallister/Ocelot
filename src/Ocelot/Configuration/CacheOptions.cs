namespace Ocelot.Configuration
{
    public class CacheOptions
    {
        public CacheOptions(int ttlSeconds, string region)
        {
            TtlSeconds = ttlSeconds;
            Region = region;
        }

        public int TtlSeconds { get; private set; }

        public string Region { get; private set; }
    }
}

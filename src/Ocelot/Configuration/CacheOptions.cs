namespace Ocelot.Configuration
{
    public class CacheOptions
    {
        public CacheOptions(int ttlSeconds, string region, string header)
        {
            TtlSeconds = ttlSeconds;
            Region = region;
            Header = header;
        }

        public int TtlSeconds { get; private set; }

        public string Region { get; private set; }

        public string Header { get; set; }
    }
}

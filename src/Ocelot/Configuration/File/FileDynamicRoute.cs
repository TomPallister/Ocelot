namespace Ocelot.Configuration.File
{
    public class FileDynamicRoute
    {
        public string ServiceName { get; set; }
        public FileRateLimitRule RateLimitRule { get; set; }
        public string DownstreamHttpVersion { get; set; }
    }
}

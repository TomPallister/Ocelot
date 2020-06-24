namespace Ocelot.Configuration.File
{
    using System.Collections.Generic;

    public class FileRoute : IRoute
    {
        public FileRoute()
        {
            UpstreamHttpMethod = new List<string>();
            AddHeadersToRequest = new Dictionary<string, string>();
            AddClaimsToRequest = new Dictionary<string, string>();
            RouteClaimsRequirement = new Dictionary<string, string>();
            AddQueriesToRequest = new Dictionary<string, string>();
            ChangeDownstreamPathTemplate = new Dictionary<string, string>();
            DownstreamHeaderTransform = new Dictionary<string, string>();
            FileCacheOptions = new FileCacheOptions();
            QoSOptions = new FileQoSOptions();
            RateLimitOptions = new FileRateLimitRule();
            AuthenticationOptions = new FileAuthenticationOptions();
            HttpHandlerOptions = new FileHttpHandlerOptions();
            UpstreamHeaderTransform = new Dictionary<string, string>();
            DownstreamHostAndPorts = new List<FileHostAndPort>();
            DelegatingHandlers = new List<string>();
            LoadBalancerOptions = new FileLoadBalancerOptions();
            SecurityOptions = new FileSecurityOptions();
            Priority = 1;
        }

        public string DownstreamPathTemplate { get; set; }
        public string UpstreamPathTemplate { get; set; }
        public List<string> UpstreamHttpMethod { get; set; }
        public string DownstreamHttpMethod { get; set; }
        public Dictionary<string, string> AddHeadersToRequest { get; set; }
        public Dictionary<string, string> UpstreamHeaderTransform { get; set; }
        public Dictionary<string, string> DownstreamHeaderTransform { get; set; }
        public Dictionary<string, string> AddClaimsToRequest { get; set; }
        public Dictionary<string, string> RouteClaimsRequirement { get; set; }
        public Dictionary<string, string> AddQueriesToRequest { get; set; }
        public Dictionary<string, string> ChangeDownstreamPathTemplate { get; set; }
        public string RequestIdKey { get; set; }
        public FileCacheOptions FileCacheOptions { get; set; }
        public bool RouteIsCaseSensitive { get; set; }
        public string ServiceName { get; set; }
        public string ServiceNamespace { get; set; }
        public string DownstreamScheme { get; set; }
        public FileQoSOptions QoSOptions { get; set; }
        public FileLoadBalancerOptions LoadBalancerOptions { get; set; }
        public FileRateLimitRule RateLimitOptions { get; set; }
        public FileAuthenticationOptions AuthenticationOptions { get; set; }
        public FileHttpHandlerOptions HttpHandlerOptions { get; set; }
        public List<FileHostAndPort> DownstreamHostAndPorts { get; set; }
        public string UpstreamHost { get; set; }
        public string Key { get; set; }
        public List<string> DelegatingHandlers { get; set; }
        public int Priority { get; set; }
        public int Timeout { get; set; }
        public bool DangerousAcceptAnyServerCertificateValidator { get; set; }
        public FileSecurityOptions SecurityOptions { get; set; }
        public string DownstreamHttpVersion { get; set; }
    }
}

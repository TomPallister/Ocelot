namespace Ocelot.Configuration
{
    /// <summary>
    /// Describes configuration parameters for http handler,
    /// that is created to handle a request to service
    /// </summary>
    public class HttpHandlerOptions
    {
        public HttpHandlerOptions(bool allowAutoRedirect, bool useCookieContainer, bool useTracing, bool useProxy, int maxConnectionsPerServer)
        {
            AllowAutoRedirect = allowAutoRedirect;
            UseCookieContainer = useCookieContainer;
            UseTracing = useTracing;
            UseProxy = useProxy;
            MaxConnectionsPerServer = maxConnectionsPerServer;
        }


        /// <summary>
        /// Specify if auto redirect is enabled
        /// </summary>
        /// <value>AllowAutoRedirect</value>
        public bool AllowAutoRedirect { get; private set; }

        /// <summary>
        /// Specify is handler has to use a cookie container
        /// </summary>
        /// <value>UseCookieContainer</value>
        public bool UseCookieContainer { get; private set; }

        /// <summary>
        /// Specify is handler has to use a opentracing
        /// </summary>
        /// <value>UseTracing</value>
        public bool UseTracing { get; private set; }

        /// <summary>
        /// Specify if handler has to use a proxy
        /// </summary>
        /// <value>UseProxy</value>
        public bool UseProxy { get; private set; }

        /// <summary>
        /// Specify the maximum of concurrent connection to a network endpoint
        /// </summary>
        /// <value>MaxConnectionsPerServer</value>
        public int MaxConnectionsPerServer { get; private set; }
    }
}

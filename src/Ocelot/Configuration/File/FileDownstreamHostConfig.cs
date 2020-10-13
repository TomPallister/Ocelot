namespace Ocelot.Configuration.File
{
    public class FileDownstreamHostConfig
    {
        /// <summary>
        /// Key to reference downstream host config from global configuration.
        /// </summary>
        /// <value>
        /// Key reference from <see cref="FileGlobalConfiguration.DownstreamHosts"/>
        /// </value>
        public string GlobalHostKey { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
    }
}

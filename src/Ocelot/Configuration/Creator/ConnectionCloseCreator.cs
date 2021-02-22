using Ocelot.Configuration.File;

namespace Ocelot.Configuration.Creator
{
    public class ConnectionCloseCreator : IConnectionCloseCreator
    {
        public bool Create(bool fileRouteConnectionClose, FileGlobalConfiguration globalConfiguration)
        {
            var globalConnectionClose = globalConfiguration.ConnectionClose;

            return fileRouteConnectionClose || globalConnectionClose;
        }
    }
}

using Ocelot.Configuration.File;

namespace Ocelot.Configuration.Creator
{
    public interface IConnectionCloseCreator
    {
        bool Create(bool fileRouteConnectionClose, FileGlobalConfiguration globalConfiguration);
    }
}

using Ocelot.Configuration.File;

namespace Ocelot.Configuration.Creator
{
    public interface IRouteOptionsCreator
    {
        RouteOptions Create(FileRoute fileRoute);
    }
}

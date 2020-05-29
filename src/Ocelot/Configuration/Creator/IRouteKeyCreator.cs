using Ocelot.Configuration.File;

namespace Ocelot.Configuration.Creator
{
    public interface IRouteKeyCreator
    {
        string Create(FileRoute fileRoute);
    }
}

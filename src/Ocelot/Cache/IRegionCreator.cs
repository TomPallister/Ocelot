using Ocelot.Configuration.File;

namespace Ocelot.Cache
{
    public interface IRegionCreator
    {
        string Create(FileRoute route);
    }
}
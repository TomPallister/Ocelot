using Ocelot.Configuration.File;

namespace Ocelot.Configuration.Creator
{
    public interface IRequestIdKeyCreator
    {
        string Create(FileRoute fileRoute, FileGlobalConfiguration globalConfiguration);
    }
}
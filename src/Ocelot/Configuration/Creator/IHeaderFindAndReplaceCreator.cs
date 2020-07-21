using Ocelot.Configuration.File;

namespace Ocelot.Configuration.Creator
{
    public interface IHeaderFindAndReplaceCreator
    {
        HeaderTransformations Create(FileRoute fileRoute);
    }
}

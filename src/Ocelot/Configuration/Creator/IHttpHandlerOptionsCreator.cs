using Ocelot.Configuration.File;

namespace Ocelot.Configuration.Creator
{
    /// <summary>
    /// Describes creation of HttpHandlerOptions
    /// </summary>
    public interface IHttpHandlerOptionsCreator
    {
        HttpHandlerOptions Create(FileHttpHandlerOptions fileRoute);
    }
}

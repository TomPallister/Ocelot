using Ocelot.Configuration.File;
using Ocelot.Responses;
using System.Threading.Tasks;

namespace Ocelot.Configuration.Setter
{
    public interface IFileConfigurationSetter
    {
        Task<Response> Set(FileConfiguration config);
    }
}

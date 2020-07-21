using Ocelot.Configuration.File;
using Ocelot.Responses;
using System.Threading.Tasks;

namespace Ocelot.Configuration.Repository
{
    public interface IFileConfigurationRepository
    {
        Task<Response<FileConfiguration>> Get();

        Task<Response> Set(FileConfiguration fileConfiguration);
    }
}

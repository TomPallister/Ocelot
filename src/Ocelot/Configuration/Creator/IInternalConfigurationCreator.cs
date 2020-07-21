using Ocelot.Configuration.File;
using Ocelot.Responses;
using System.Threading.Tasks;

namespace Ocelot.Configuration.Creator
{
    public interface IInternalConfigurationCreator
    {
        Task<Response<IInternalConfiguration>> Create(FileConfiguration fileConfiguration);
    }
}

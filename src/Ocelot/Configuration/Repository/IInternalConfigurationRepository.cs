using Ocelot.Responses;

namespace Ocelot.Configuration.Repository
{
    public interface IInternalConfigurationRepository
    {
        Response<IInternalConfiguration> Get();

        Response AddOrReplace(IInternalConfiguration internalConfiguration);
    }
}

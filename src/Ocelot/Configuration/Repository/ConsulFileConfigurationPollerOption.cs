using Ocelot.Responses;
using System.Threading.Tasks;

namespace Ocelot.Configuration.Repository
{
    public class ConsulFileConfigurationPollerOption : IFileConfigurationPollerOptions
    {
        private readonly IInternalConfigurationRepository _internalConfigRepo;
        private readonly IFileConfigurationRepository _fileConfigurationRepository;

        public ConsulFileConfigurationPollerOption(IInternalConfigurationRepository internalConfigurationRepository,
                                                   IFileConfigurationRepository fileConfigurationRepository)
        {
            _internalConfigRepo = internalConfigurationRepository;
            _fileConfigurationRepository = fileConfigurationRepository;
        }

        public int Delay => GetDelay();

        private int GetDelay()
        {
            int delay = 1000;

            Response<File.FileConfiguration> fileConfig = Task.Run(async () => await _fileConfigurationRepository.Get()).Result;
            if (fileConfig?.Data?.GlobalConfiguration?.ServiceDiscoveryProvider != null &&
                    !fileConfig.IsError &&
                    fileConfig.Data.GlobalConfiguration.ServiceDiscoveryProvider.PollingInterval > 0)
            {
                delay = fileConfig.Data.GlobalConfiguration.ServiceDiscoveryProvider.PollingInterval;
            }
            else
            {
                Response<IInternalConfiguration> internalConfig = _internalConfigRepo.Get();
                if (internalConfig?.Data?.ServiceProviderConfiguration != null &&
                !internalConfig.IsError &&
                internalConfig.Data.ServiceProviderConfiguration.PollingInterval > 0)
                {
                    delay = internalConfig.Data.ServiceProviderConfiguration.PollingInterval;
                }
            }

            return delay;
        }
    }
}

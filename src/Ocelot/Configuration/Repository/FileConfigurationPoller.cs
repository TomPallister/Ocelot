using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Ocelot.Configuration.Creator;
using Ocelot.Configuration.File;
using Ocelot.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ocelot.Configuration.Repository
{
    public class FileConfigurationPoller : IHostedService, IDisposable
    {
        private readonly IOcelotLogger _logger;
        private readonly IFileConfigurationRepository _repo;
        private string _previousAsJson;
        private Timer _timer;
        private bool _polling;
        private readonly IFileConfigurationPollerOptions _options;
        private readonly IInternalConfigurationRepository _internalConfigRepo;
        private readonly IInternalConfigurationCreator _internalConfigCreator;

        public FileConfigurationPoller(
            IOcelotLoggerFactory factory,
            IFileConfigurationRepository repo,
            IFileConfigurationPollerOptions options,
            IInternalConfigurationRepository internalConfigRepo,
            IInternalConfigurationCreator internalConfigCreator)
        {
            _internalConfigRepo = internalConfigRepo;
            _internalConfigCreator = internalConfigCreator;
            _options = options;
            _logger = factory.CreateLogger<FileConfigurationPoller>();
            _repo = repo;
            _previousAsJson = "";
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(FileConfigurationPoller)} is starting.");

            _timer = new Timer(async x =>
            {
                if (_polling)
                {
                    return;
                }

                _polling = true;
                await Poll();
                _polling = false;
            }, null, _options.Delay, _options.Delay);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(FileConfigurationPoller)} is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        private async Task Poll()
        {
            _logger.LogInformation("Started polling");

            var fileConfig = await _repo.Get();

            if (fileConfig.IsError)
            {
                _logger.LogWarning($"error geting file config, errors are {string.Join(",", fileConfig.Errors.Select(x => x.Message))}");
                return;
            }

            var asJson = ToJson(fileConfig.Data);

            if (!fileConfig.IsError && asJson != _previousAsJson)
            {
                var config = await _internalConfigCreator.Create(fileConfig.Data);

                if (!config.IsError)
                {
                    _internalConfigRepo.AddOrReplace(config.Data);
                }

                _previousAsJson = asJson;
            }

            _logger.LogInformation("Finished polling");
        }

        /// <summary>
        /// We could do object comparison here but performance isnt really a problem. This might be an issue one day!
        /// </summary>
        /// <returns>hash of the config</returns>
        private string ToJson(FileConfiguration config)
        {
            var currentHash = JsonConvert.SerializeObject(config);
            return currentHash;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _timer = null;
        }
    }
}

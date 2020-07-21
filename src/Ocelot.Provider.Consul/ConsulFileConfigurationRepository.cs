namespace Ocelot.Provider.Consul
{
    using Configuration.File;
    using Configuration.Repository;
    using global::Consul;
    using Logging;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Responses;
    using System;
    using System.Text;
    using System.Threading.Tasks;

    public class ConsulFileConfigurationRepository : IFileConfigurationRepository
    {
        private readonly IConsulClient _consul;
        private readonly string _configurationKey;
        private readonly Cache.IOcelotCache<FileConfiguration> _cache;
        private readonly IOcelotLogger _logger;

        public ConsulFileConfigurationRepository(
            IOptions<FileConfiguration> fileConfiguration,
            Cache.IOcelotCache<FileConfiguration> cache,
            IConsulClientFactory factory,
            IOcelotLoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ConsulFileConfigurationRepository>();
            _cache = cache;

            var serviceDiscoveryProvider = fileConfiguration.Value.GlobalConfiguration.ServiceDiscoveryProvider;
            _configurationKey = string.IsNullOrWhiteSpace(serviceDiscoveryProvider.ConfigurationKey) ? "InternalConfiguration" :
                serviceDiscoveryProvider.ConfigurationKey;

            var config = new ConsulRegistryConfiguration(serviceDiscoveryProvider.Scheme, serviceDiscoveryProvider.Host,
                serviceDiscoveryProvider.Port, _configurationKey, serviceDiscoveryProvider.Token);

            _consul = factory.Get(config);
        }

        public async Task<Response<FileConfiguration>> Get()
        {
            var config = _cache.Get(_configurationKey, _configurationKey);

            if (config != null)
            {
                return new OkResponse<FileConfiguration>(config);
            }

            var queryResult = await _consul.KV.Get(_configurationKey);

            if (queryResult.Response == null)
            {
                return new OkResponse<FileConfiguration>(null);
            }

            var bytes = queryResult.Response.Value;

            var json = Encoding.UTF8.GetString(bytes);

            var consulConfig = JsonConvert.DeserializeObject<FileConfiguration>(json);

            return new OkResponse<FileConfiguration>(consulConfig);
        }

        public async Task<Response> Set(FileConfiguration ocelotConfiguration)
        {
            var json = JsonConvert.SerializeObject(ocelotConfiguration, Formatting.Indented);

            var bytes = Encoding.UTF8.GetBytes(json);

            var kvPair = new KVPair(_configurationKey)
            {
                Value = bytes
            };

            var result = await _consul.KV.Put(kvPair);

            if (result.Response)
            {
                _cache.AddAndDelete(_configurationKey, ocelotConfiguration, TimeSpan.FromSeconds(3), _configurationKey);

                return new OkResponse();
            }

            return new ErrorResponse(new UnableToSetConfigInConsulError($"Unable to set FileConfiguration in consul, response status code from consul was {result.StatusCode}"));
        }
    }
}

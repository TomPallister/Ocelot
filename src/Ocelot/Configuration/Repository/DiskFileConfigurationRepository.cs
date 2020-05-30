using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using Ocelot.Configuration.File;
using Ocelot.Responses;
using System;
using System.Threading.Tasks;
using Ocelot.Cache;
using Ocelot.Configuration.ChangeTracking;

namespace Ocelot.Configuration.Repository
{
    public class DiskFileConfigurationRepository : IFileConfigurationRepository
    {
        private readonly IOcelotConfigurationChangeTokenSource _changeTokenSource;
        private readonly IOcelotCache<FileConfiguration> _cache;
        private readonly string _environmentFilePath;
        private readonly string _ocelotFilePath;
        private readonly string _cacheKey;
        private static readonly object _lock = new object();
        private const string ConfigurationFileName = "ocelot";

        public DiskFileConfigurationRepository(IWebHostEnvironment hostingEnvironment,
            IOcelotConfigurationChangeTokenSource changeTokenSource, Cache.IOcelotCache<FileConfiguration> cache)
        {
            _changeTokenSource = changeTokenSource;
            _cache = cache;
            _environmentFilePath = $"{AppContext.BaseDirectory}{ConfigurationFileName}{(string.IsNullOrEmpty(hostingEnvironment.EnvironmentName) ? string.Empty : ".")}{hostingEnvironment.EnvironmentName}.json";
            _cacheKey = "InternalDiskFileConfigurationRepository";

            _ocelotFilePath = $"{AppContext.BaseDirectory}{ConfigurationFileName}.json";
        }

        public Task<Response<FileConfiguration>> Get()
        {
            var configuration = _cache.Get(_cacheKey, _cacheKey);

            if (configuration != null)
                return Task.FromResult<Response<FileConfiguration>>(new OkResponse<FileConfiguration>(configuration));

            string jsonConfiguration;

            lock (_lock)
            {
                jsonConfiguration = System.IO.File.ReadAllText(_environmentFilePath);
            }

            var fileConfiguration = JsonConvert.DeserializeObject<FileConfiguration>(jsonConfiguration);

            return Task.FromResult<Response<FileConfiguration>>(new OkResponse<FileConfiguration>(fileConfiguration));
        }

        public Task<Response> Set(FileConfiguration fileConfiguration)
        {
            string jsonConfiguration = JsonConvert.SerializeObject(fileConfiguration, Formatting.Indented);

            lock (_lock)
            {
                if (System.IO.File.Exists(_environmentFilePath))
                {
                    System.IO.File.Delete(_environmentFilePath);
                }

                System.IO.File.WriteAllText(_environmentFilePath, jsonConfiguration);

                if (System.IO.File.Exists(_ocelotFilePath))
                {
                    System.IO.File.Delete(_ocelotFilePath);
                }

                System.IO.File.WriteAllText(_ocelotFilePath, jsonConfiguration);
            }

            _changeTokenSource.Activate();

            _cache.AddAndDelete(_cacheKey, fileConfiguration, TimeSpan.FromMinutes(5), _cacheKey);

            return Task.FromResult<Response>(new OkResponse());
        }
    }
}

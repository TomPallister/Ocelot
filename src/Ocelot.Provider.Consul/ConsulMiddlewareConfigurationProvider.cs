namespace Ocelot.Provider.Consul
{
    using Configuration.Creator;
    using Configuration.File;
    using Configuration.Repository;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Middleware;
    using Responses;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public static class ConsulMiddlewareConfigurationProvider
    {
        public static OcelotMiddlewareConfigurationDelegate Get = async builder =>
        {
            var fileConfigRepo = builder.ApplicationServices.GetService<IFileConfigurationRepository>();
            var fileConfig = builder.ApplicationServices.GetService<IOptionsMonitor<FileConfiguration>>();
            var internalConfigCreator = builder.ApplicationServices.GetService<IInternalConfigurationCreator>();
            var internalConfigRepo = builder.ApplicationServices.GetService<IInternalConfigurationRepository>();

            if (UsingConsul(fileConfigRepo))
            {
                await SetFileConfigInConsul(builder, fileConfigRepo, fileConfig, internalConfigCreator, internalConfigRepo);
            }
        };

        private static bool UsingConsul(IFileConfigurationRepository fileConfigRepo)
        {
            return fileConfigRepo.GetType() == typeof(ConsulFileConfigurationRepository);
        }

        private static async Task SetFileConfigInConsul(IApplicationBuilder builder,
            IFileConfigurationRepository fileConfigRepo, IOptionsMonitor<FileConfiguration> fileConfig,
            IInternalConfigurationCreator internalConfigCreator, IInternalConfigurationRepository internalConfigRepo)
        {
            // get the config from consul.
            var fileConfigFromConsul = await fileConfigRepo.Get();

            if (IsError(fileConfigFromConsul))
            {
                ThrowToStopOcelotStarting(fileConfigFromConsul);
            }
            else if (ConfigNotStoredInConsul(fileConfigFromConsul))
            {
                //there was no config in consul set the file in config in consul
                await fileConfigRepo.Set(fileConfig.CurrentValue);
            }
            else
            {
                // create the internal config from consul data
                var internalConfig = await internalConfigCreator.Create(fileConfigFromConsul.Data);

                if (IsError(internalConfig))
                {
                    ThrowToStopOcelotStarting(internalConfig);
                }
                else
                {
                    // add the internal config to the internal repo
                    var response = internalConfigRepo.AddOrReplace(internalConfig.Data);

                    if (IsError(response))
                    {
                        ThrowToStopOcelotStarting(response);
                    }
                }

                if (IsError(internalConfig))
                {
                    ThrowToStopOcelotStarting(internalConfig);
                }
            }
        }

        private static void ThrowToStopOcelotStarting(Response config)
        {
            throw new Exception($"Unable to start Ocelot, errors are: {string.Join(",", config.Errors.Select(x => x.ToString()))}");
        }

        private static bool IsError(Response response)
        {
            return response == null || response.IsError;
        }

        private static bool ConfigNotStoredInConsul(Response<FileConfiguration> fileConfigFromConsul)
        {
            return fileConfigFromConsul.Data == null;
        }
    }
}

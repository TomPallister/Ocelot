namespace Ocelot.Middleware
{
    using Ocelot.DependencyInjection;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Ocelot.Configuration;
    using Ocelot.Configuration.Creator;
    using Ocelot.Configuration.File;
    using Ocelot.Configuration.Repository;
    using Ocelot.Configuration.Setter;
    using Ocelot.Logging;
    using Ocelot.Responses;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    public static class OcelotMiddlewareExtensions
    {
        public static async Task<IApplicationBuilder> UseOcelot(this IApplicationBuilder builder)
        {
            await builder.UseOcelot(new OcelotPipelineConfiguration());
            return builder;
        }

        public static async Task<IApplicationBuilder> UseOcelot(this IApplicationBuilder builder, Action<OcelotPipelineConfiguration> pipelineConfiguration)
        {
            var config = new OcelotPipelineConfiguration();
            pipelineConfiguration?.Invoke(config);
            return await builder.UseOcelot(config);
        }

        public static async Task<IApplicationBuilder> UseOcelot(this IApplicationBuilder builder, OcelotPipelineConfiguration pipelineConfiguration)
        {
            var configuration = await CreateConfiguration(builder);

            ConfigureDiagnosticListener(builder);

            return CreateOcelotPipeline(builder, pipelineConfiguration);
        }

        public static Task<IApplicationBuilder> UseOcelot(this IApplicationBuilder app, Action<IApplicationBuilder, OcelotPipelineConfiguration> builderAction)
            => UseOcelot(app, builderAction, new OcelotPipelineConfiguration());

        public static async Task<IApplicationBuilder> UseOcelot(this IApplicationBuilder app, Action<IApplicationBuilder, OcelotPipelineConfiguration> builderAction, OcelotPipelineConfiguration configuration)
        {
            await CreateConfiguration(app);

            ConfigureDiagnosticListener(app);

            builderAction?.Invoke(app, configuration ?? new OcelotPipelineConfiguration());

            app.Properties["analysis.NextMiddlewareName"] = "TransitionToOcelotMiddleware";

            return app;
        }

        private static IApplicationBuilder CreateOcelotPipeline(IApplicationBuilder builder, OcelotPipelineConfiguration pipelineConfiguration)
        {
            builder.BuildOcelotPipeline(pipelineConfiguration);

            /*
            inject first delegate into first piece of asp.net middleware..maybe not like this
            then because we are updating the http context in ocelot it comes out correct for
            rest of asp.net..
            */

            builder.Properties["analysis.NextMiddlewareName"] = "TransitionToOcelotMiddleware";

            return builder;
        }

        private static async Task<IInternalConfiguration> CreateConfiguration(IApplicationBuilder builder)
        {
            // make configuration from file system?
            // earlier user needed to add ocelot files in startup configuration stuff, asp.net will map it to this
            var fileConfig = builder.ApplicationServices.GetService<IOptionsMonitor<FileConfiguration>>();

            // now create the config
            var internalConfigCreator = builder.ApplicationServices.GetService<IInternalConfigurationCreator>();
            var internalConfig = await internalConfigCreator.Create(fileConfig.CurrentValue);

            //Configuration error, throw error message
            if (internalConfig.IsError)
            {
                ThrowToStopOcelotStarting(internalConfig);
            }

            // now save it in memory
            var internalConfigRepo = builder.ApplicationServices.GetService<IInternalConfigurationRepository>();
            internalConfigRepo.AddOrReplace(internalConfig.Data);

            fileConfig.OnChange(async (config) =>
            {
                var newInternalConfig = await internalConfigCreator.Create(config);
                internalConfigRepo.AddOrReplace(newInternalConfig.Data);
            });

            var adminPath = builder.ApplicationServices.GetService<IAdministrationPath>();

            var configurations = builder.ApplicationServices.GetServices<OcelotMiddlewareConfigurationDelegate>();

            // Todo - this has just been added for consul so far...will there be an ordering problem in the future? Should refactor all config into this pattern?
            foreach (var configuration in configurations)
            {
                await configuration(builder);
            }

            if (AdministrationApiInUse(adminPath))
            {
                //We have to make sure the file config is set for the ocelot.env.json and ocelot.json so that if we pull it from the
                //admin api it works...boy this is getting a spit spags boll.
                var fileConfigSetter = builder.ApplicationServices.GetService<IFileConfigurationSetter>();

                await SetFileConfig(fileConfigSetter, fileConfig);
            }

            return GetOcelotConfigAndReturn(internalConfigRepo);
        }

        private static bool AdministrationApiInUse(IAdministrationPath adminPath)
        {
            return adminPath != null;
        }

        private static async Task SetFileConfig(IFileConfigurationSetter fileConfigSetter, IOptionsMonitor<FileConfiguration> fileConfig)
        {
            var response = await fileConfigSetter.Set(fileConfig.CurrentValue);

            if (IsError(response))
            {
                ThrowToStopOcelotStarting(response);
            }
        }

        private static bool IsError(Response response)
        {
            return response == null || response.IsError;
        }

        private static IInternalConfiguration GetOcelotConfigAndReturn(IInternalConfigurationRepository provider)
        {
            var ocelotConfiguration = provider.Get();

            if (ocelotConfiguration?.Data == null || ocelotConfiguration.IsError)
            {
                ThrowToStopOcelotStarting(ocelotConfiguration);
            }

            return ocelotConfiguration.Data;
        }

        private static void ThrowToStopOcelotStarting(Response config)
        {
            throw new Exception($"Unable to start Ocelot, errors are: {string.Join(",", config.Errors.Select(x => x.ToString()))}");
        }

        private static void ConfigureDiagnosticListener(IApplicationBuilder builder)
        {
            var env = builder.ApplicationServices.GetService<IWebHostEnvironment>();
            var listener = builder.ApplicationServices.GetService<OcelotDiagnosticListener>();
            var diagnosticListener = builder.ApplicationServices.GetService<DiagnosticListener>();
            diagnosticListener.SubscribeWithAdapter(listener);
        }
    }
}

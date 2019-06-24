﻿using Ocelot.Routing.ServiceFabric;
using System;

namespace Microsoft.Extensions.Configuration
{
    public static class ConfigurationBuilderExtensions
    {
        /// <summary>
        /// Adds service fabric configuration provider extension for Ocelot. 
        /// </summary>
        /// <param name="builder">Configuration builder.</param>
        /// <returns>Updated configuration builder.</returns>
        /// <remarks>The service fabric client configuration must be under section ServiceFabricClientConfiguration. For schema see <see cref="ServiceFabricClientFactoryOptions"/>.</remarks>
        public static IConfigurationBuilder AddServiceFabricExtension(this IConfigurationBuilder builder)
        {
            ServiceFabricClientFactoryOptions clientFactoryOptions = builder.Build().GetSection("ServiceFabricClientConfiguration").Get<ServiceFabricClientFactoryOptions>();
            return builder.AddServiceFabricExtension(clientFactoryOptions);
        }

        /// <summary>
        /// Adds service fabric configuration provider extension for Ocelot. 
        /// </summary>
        /// <param name="builder">Configuration builder.</param>
        /// <param name="configureClientFactoryOptions">Client factory configuration action.</param>
        /// <returns>Updated configuration builder.</returns>
        public static IConfigurationBuilder AddServiceFabricExtension(this IConfigurationBuilder builder, Action<ServiceFabricClientFactoryOptions> configureClientFactoryOptions)
        {
            ServiceFabricClientFactoryOptions clientFactoryOptions = new ServiceFabricClientFactoryOptions();
            configureClientFactoryOptions(clientFactoryOptions);

            return builder.AddServiceFabricExtension(clientFactoryOptions);
        }

        /// <summary>
        /// Adds service fabric configuration provider extension for Ocelot. 
        /// </summary>
        /// <param name="builder">Configuration builder.</param>
        /// <param name="clientFactoryOptions">Client factory configuration.</param>
        /// <returns>Updated configuration builder.</returns>
        public static IConfigurationBuilder AddServiceFabricExtension(this IConfigurationBuilder builder, ServiceFabricClientFactoryOptions clientFactoryOptions)
        {
            return builder.Add(new ServiceFabricRouteConfigurationSource
            {
                ClientFactoryOptions = clientFactoryOptions,
            });
        }
    }
}

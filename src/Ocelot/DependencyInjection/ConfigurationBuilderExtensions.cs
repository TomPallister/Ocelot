namespace Ocelot.DependencyInjection
{
    using Configuration.File;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Configuration.Memory;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    public static class ConfigurationBuilderExtensions
    {
        [Obsolete("Please set BaseUrl in ocelot.json GlobalConfiguration.BaseUrl")]
        public static IConfigurationBuilder AddOcelotBaseUrl(this IConfigurationBuilder builder, string baseUrl)
        {
            var memorySource = new MemoryConfigurationSource
            {
                InitialData = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("BaseUrl", baseUrl)
                }
            };

            builder.Add(memorySource);

            return builder;
        }

        public static IConfigurationBuilder AddOcelot(this IConfigurationBuilder builder, IWebHostEnvironment env)
        {
            return builder.AddOcelot(".", env);
        }

        public static IConfigurationBuilder AddOcelot(this IConfigurationBuilder builder, string folder, IWebHostEnvironment env)
        {
            const string primaryConfigFile = "ocelot.json";

            const string globalConfigFile = "ocelot.global.json";

            const string subConfigPattern = @"^ocelot\.(.*?)\.json$";

            string excludeConfigName = env?.EnvironmentName != null ? $"ocelot.{env.EnvironmentName}.json" : string.Empty;

            var reg = new Regex(subConfigPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var files = new DirectoryInfo(folder)
                .EnumerateFiles()
                .Where(fi => reg.IsMatch(fi.Name) && (fi.Name != excludeConfigName))
                .ToList();

            dynamic fileConfiguration = new JObject();
            fileConfiguration.GlobalConfiguration = new JObject();
            fileConfiguration.Aggregates = new JArray();
            fileConfiguration.ReRoutes = new JArray();

            foreach (var file in files)
            {
                if (files.Count > 1 && file.Name.Equals(primaryConfigFile, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var lines = File.ReadAllText(file.FullName);
                dynamic config = JToken.Parse(lines);

                if (file.Name.Equals(globalConfigFile, StringComparison.OrdinalIgnoreCase))
                {
                    TryAddSection(fileConfiguration, config, nameof(FileConfiguration.GlobalConfiguration));
                }

                TryAddSection(fileConfiguration, config, nameof(FileConfiguration.Aggregates));
                TryAddSection(fileConfiguration, config, nameof(FileConfiguration.ReRoutes));
            }

            var json = JsonConvert.SerializeObject(fileConfiguration);

            File.WriteAllText(primaryConfigFile, json);

            builder.AddJsonFile(primaryConfigFile, false, false);

            return builder;
        }

        private static void TryAddSection(JToken mergedConfig, JToken config, string sectionName)
        {
            var mergedConfigSection = mergedConfig[sectionName];
            var configSection = config[sectionName];

            if (configSection != null)
            {
                if (configSection is JObject)
                {
                    mergedConfig[sectionName] = configSection;
                }
                else if (configSection is JArray)
                {
                    (mergedConfigSection as JArray).Merge(configSection);
                }
            }            
        }        
    }
}

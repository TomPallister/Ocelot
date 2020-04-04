using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ocelot.Configuration.File;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.AcceptanceTests
{
    public class ConfigurationMergeTests
    {
        private Steps _steps;
        private IWebHostBuilder _webHostBuilder;
        private TestServer _ocelotServer;
        private HttpClient _ocelotClient;

        public ConfigurationMergeTests()
        {
            _steps = new Steps();
        }

        [Fact]
        public void should_merge_reroutes_custom_properties()
        {
            this.Given(x => GivenOcelotIsRunningWithMultipleConfigs())
                .And(x => ThenConfigContentShouldBeMerged())
                .BDDfy();
        }

        private void GivenOcelotIsRunningWithMultipleConfigs()
        {
            _webHostBuilder = new WebHostBuilder();

            _webHostBuilder
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath);
                    var env = hostingContext.HostingEnvironment;
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: false);
                    config.AddOcelot("MergeConfiguration", hostingContext.HostingEnvironment);
                    config.AddEnvironmentVariables();
                })
                .ConfigureServices(s =>
                {
                    s.AddOcelot();
                })
                .Configure(app =>
                {
                    app.UseOcelot().Wait();
                });

            _ocelotServer = new TestServer(_webHostBuilder);

            _ocelotClient = _ocelotServer.CreateClient();
        }

        private void ThenConfigContentShouldBeMerged()
        {
            var mergedConfigFileName = "ocelot.json";
            File.Exists(mergedConfigFileName).ShouldBeTrue();
            var lines = File.ReadAllText(mergedConfigFileName);
            var config = JObject.Parse(lines);

            config[nameof(FileConfiguration.ReRoutes)].ShouldNotBeNull();
            config[nameof(FileConfiguration.ReRoutes)].Children().Count().ShouldBe(3);

            var routeWithCustomProperty = config[nameof(FileConfiguration.ReRoutes)].Children().SingleOrDefault(c => c["CustomStrategyProperty"] != null);
            routeWithCustomProperty.ShouldNotBeNull();
            var customProperty = routeWithCustomProperty["CustomStrategyProperty"];
            customProperty["GET"].ShouldNotBeNull();
            customProperty["GET"].Children().Count().ShouldBe(1);
            customProperty["GET"].Children().FirstOrDefault().ShouldBe("SomeCustomStrategyMethodA");
            customProperty["POST"].ShouldNotBeNull();
            customProperty["POST"].Children().Count().ShouldBe(1);
            customProperty["POST"].Children().FirstOrDefault().ShouldBe("SomeCustomStrategyMethodB");

            var routeWithCustomProperty2 = config[nameof(FileConfiguration.ReRoutes)].Children().SingleOrDefault(c => c["somethingmore"] != null);
            routeWithCustomProperty2.ShouldNotBeNull();
            routeWithCustomProperty2["somethingmore"].ShouldBe("something");
        }
    }
}

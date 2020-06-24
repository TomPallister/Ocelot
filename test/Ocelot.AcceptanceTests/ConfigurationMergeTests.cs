using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Ocelot.Configuration.File;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Shouldly;
using System.IO;
using System.Linq;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.AcceptanceTests
{
    public class ConfigurationMergeTests
    {        
        private IWebHostBuilder _webHostBuilder;
        private TestServer _ocelotServer;

        [Fact]
        public void should_merge_reroutes_custom_properties()
        {
            this.Given(x => GivenOcelotIsRunningWithMultipleConfigs())
                .And(x => ThenConfigContentShouldBeMergedWithReRoutesCustomProperties())
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

            var ocelotClient = _ocelotServer.CreateClient();
        }

        private void ThenConfigContentShouldBeMergedWithReRoutesCustomProperties()
        {
            var mergedConfigFileName = "ocelot.json";
            File.Exists(mergedConfigFileName).ShouldBeTrue();
            var lines = File.ReadAllText(mergedConfigFileName);
            var config = JObject.Parse(lines);

            config[nameof(FileConfiguration.Routes)].ShouldNotBeNull();
            config[nameof(FileConfiguration.Routes)].Children().Count().ShouldBe(3);

            var routeWithCustomPropertyX = config[nameof(FileConfiguration.Routes)].Children()
                .SingleOrDefault(c => c["CustomStrategyProperty"] != null);
            routeWithCustomPropertyX.ShouldNotBeNull();
            var customPropertyX = routeWithCustomPropertyX["CustomStrategyProperty"];
            customPropertyX["GET"].ShouldNotBeNull();
            customPropertyX["GET"].Children().Count().ShouldBe(1);
            customPropertyX["GET"].Children().FirstOrDefault().ShouldBe("SomeCustomStrategyMethodA");
            customPropertyX["POST"].ShouldNotBeNull();
            customPropertyX["POST"].Children().Count().ShouldBe(1);
            customPropertyX["POST"].Children().FirstOrDefault().ShouldBe("SomeCustomStrategyMethodB");

            var routeWithCustomPropertyGlobal = config[nameof(FileConfiguration.Routes)].Children()
                .SingleOrDefault(c => c["somethingmore"] != null);
            routeWithCustomPropertyGlobal.ShouldNotBeNull();
            routeWithCustomPropertyGlobal["somethingmore"].ShouldBe("something");

            var routeWithCustomPropertyY = config[nameof(FileConfiguration.Routes)].Children()
                .SingleOrDefault(c => c["MyCustomProperty"] != null);
            routeWithCustomPropertyY.ShouldNotBeNull();
            routeWithCustomPropertyY["MyCustomProperty"].ShouldBeAssignableTo(typeof(JArray));
            routeWithCustomPropertyY["MyCustomProperty"].Count().ShouldBe(1);
            routeWithCustomPropertyY["MyCustomProperty"].First().ShouldBe("myValue");
        }
    }
}

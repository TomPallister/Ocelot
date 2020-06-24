using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Ocelot.IntegrationTests
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using Ocelot.Configuration.File;
    using Ocelot.DependencyInjection;
    using Ocelot.Middleware;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using TestStack.BDDfy;

    public class HeaderTests : IDisposable
    {
        private readonly HttpClient _httpClient;
        private IWebHost _builder;
        private IWebHostBuilder _webHostBuilder;
        private readonly string _ocelotBaseUrl;
        private IWebHost _downstreamBuilder;
        private HttpResponseMessage _response;

        public HeaderTests()
        {
            _httpClient = new HttpClient();
            _ocelotBaseUrl = "http://localhost:5010";
            _httpClient.BaseAddress = new Uri(_ocelotBaseUrl);
        }

        [Fact]
        public void should_pass_remote_ip_address_if_as_x_forwarded_for_header()
        {
            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                {
                    new FileRoute
                    {
                        DownstreamPathTemplate = "/",
                        DownstreamScheme = "http",
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "localhost",
                                Port = 6773,
                            }
                        },
                        UpstreamPathTemplate = "/",
                        UpstreamHttpMethod = new List<string> { "Get" },
                        UpstreamHeaderTransform = new Dictionary<string,string>
                        {
                            {"X-Forwarded-For", "{RemoteIpAddress}"}
                        },
                        HttpHandlerOptions = new FileHttpHandlerOptions
                        {
                            AllowAutoRedirect = false
                        }
                    }
                }
            };

            this.Given(x => GivenThereIsAServiceRunningOn("http://localhost:6773", 200, "X-Forwarded-For"))
                .And(x => GivenThereIsAConfiguration(configuration))
                .And(x => GivenOcelotIsRunning())
                .When(x => WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => ThenXForwardedForIsSet())
                .BDDfy();
        }

        private void GivenThereIsAServiceRunningOn(string url, int statusCode, string headerKey)
        {
            _downstreamBuilder = new WebHostBuilder()
                .UseUrls(url)
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseUrls(url)
                .Configure(app =>
                {
                    app.Run(async context =>
                    {
                        if (context.Request.Headers.TryGetValue(headerKey, out var values))
                        {
                            var result = values.First();
                            context.Response.StatusCode = statusCode;
                            await context.Response.WriteAsync(result);
                        }
                    });
                })
                .Build();

            _downstreamBuilder.Start();
        }

        private void GivenOcelotIsRunning()
        {
            _webHostBuilder = new WebHostBuilder()
                .UseUrls(_ocelotBaseUrl)
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath);
                    var env = hostingContext.HostingEnvironment;
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: false);
                    config.AddJsonFile("ocelot.json", false, false);
                    config.AddEnvironmentVariables();
                })
                .ConfigureServices(x =>
                {
                    x.AddOcelot();
                })
                .Configure(app =>
                {
                    app.UseOcelot().Wait();
                });

            _builder = _webHostBuilder.Build();

            _builder.Start();
        }

        private void GivenThereIsAConfiguration(FileConfiguration fileConfiguration)
        {
            var configurationPath = $"{Directory.GetCurrentDirectory()}/ocelot.json";

            var jsonConfiguration = JsonConvert.SerializeObject(fileConfiguration);

            if (File.Exists(configurationPath))
            {
                File.Delete(configurationPath);
            }

            File.WriteAllText(configurationPath, jsonConfiguration);

            var text = File.ReadAllText(configurationPath);

            configurationPath = $"{AppContext.BaseDirectory}/ocelot.json";

            if (File.Exists(configurationPath))
            {
                File.Delete(configurationPath);
            }

            File.WriteAllText(configurationPath, jsonConfiguration);

            text = File.ReadAllText(configurationPath);
        }

        private async Task WhenIGetUrlOnTheApiGateway(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            _response = await _httpClient.SendAsync(request);
        }

        private void ThenTheStatusCodeShouldBe(HttpStatusCode code)
        {
            _response.StatusCode.ShouldBe(code);
        }

        private void ThenXForwardedForIsSet()
        {
            var windowsOrMac = "::1";
            var linux = "127.0.0.1";

            var header = _response.Content.ReadAsStringAsync().Result;

            bool passed = false;

            if (header == windowsOrMac || header == linux)
            {
                passed = true;
            }

            passed.ShouldBeTrue();
        }

        public void Dispose()
        {
            _builder?.Dispose();
            _httpClient?.Dispose();
            _downstreamBuilder?.Dispose();
        }
    }
}

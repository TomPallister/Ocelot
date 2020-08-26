using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Validators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ocelot.Configuration.File;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ocelot.Benchmarks
{
    [Config(typeof(AllTheThingsBenchmarks))]
    public class AllTheThingsBenchmarks : ManualConfig
    {
        private IWebHost _service;
        private IWebHost _ocelot;
        private HttpClient _httpClient;

        public AllTheThingsBenchmarks()
        {
            Add(StatisticColumn.AllStatistics);
            Add(MemoryDiagnoser.Default);
            Add(BaselineValidator.FailOnError);
        }

        [GlobalSetup]
        public void SetUp()
        {
            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                    {
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/",
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = 51879,
                                }
                            },
                            DownstreamScheme = "http",
                            UpstreamPathTemplate = "/",
                            UpstreamHttpMethod = new List<string> { "Get" },
                        }
                    }
            };

            GivenThereIsAServiceRunningOn("http://localhost:51879", "/", 201, string.Empty);
            GivenThereIsAConfiguration(configuration);
            GivenOcelotIsRunning("http://localhost:5000");

            _httpClient = new HttpClient();
        }

        [Benchmark(Baseline = true)]
        public async Task Baseline()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:5000/");
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        /*        * Summary*
                 BenchmarkDotNet = v0.10.13, OS = macOS 10.12.6 (16G1212) [Darwin 16.7.0]
                Intel Core i5-4278U CPU 2.60GHz(Haswell), 1 CPU, 4 logical cores and 2 physical cores
               .NET Core SDK = 2.1.4

                 [Host]     : .NET Core 2.0.6 (CoreCLR 4.6.0.0, CoreFX 4.6.26212.01), 64bit RyuJIT
                   DefaultJob : .NET Core 2.0.6 (CoreCLR 4.6.0.0, CoreFX 4.6.26212.01), 64bit RyuJIT
                    Method |     Mean |     Error |    StdDev |    StdErr |      Min |       Q1 |   Median |       Q3 |      Max |  Op/s | Scaled |   Gen 0 |  Gen 1 | Allocated |
                 --------- |---------:|----------:|----------:|----------:|---------:|---------:|---------:|---------:|---------:|------:|-------:|--------:|-------:|----------:|
                Baseline | 2.102 ms | 0.0292 ms | 0.0273 ms | 0.0070 ms | 2.063 ms | 2.080 ms | 2.093 ms | 2.122 ms | 2.152 ms | 475.8 |   1.00 | 31.2500 | 3.9063 |   1.63 KB |*/

        private void GivenOcelotIsRunning(string url)
        {
            _ocelot = new WebHostBuilder()
                .UseKestrel()
                .UseUrls(url)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config
                        .SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
                        .AddJsonFile("appsettings.json", true, true)
                        .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", true, true)
                        .AddJsonFile("ocelot.json", false, false)
                        .AddEnvironmentVariables();
                })
                .ConfigureServices(s =>
                {
                    s.AddOcelot();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                })
                .UseIISIntegration()
                .Configure(app =>
                {
                    app.UseOcelot().Wait();
                })
                .Build();

            _ocelot.Start();
        }

        public void GivenThereIsAConfiguration(FileConfiguration fileConfiguration)
        {
            var configurationPath = Path.Combine(AppContext.BaseDirectory, "ocelot.json");

            var jsonConfiguration = JsonConvert.SerializeObject(fileConfiguration);

            if (File.Exists(configurationPath))
            {
                File.Delete(configurationPath);
            }

            File.WriteAllText(configurationPath, jsonConfiguration);
        }

        private void GivenThereIsAServiceRunningOn(string baseUrl, string basePath, int statusCode, string responseBody)
        {
            _service = new WebHostBuilder()
                .UseUrls(baseUrl)
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .Configure(app =>
                {
                    app.UsePathBase(basePath);
                    app.Run(async context =>
                    {
                        context.Response.StatusCode = statusCode;
                        await context.Response.WriteAsync(responseBody);
                    });
                })
                .Build();

            _service.Start();
        }
    }
}

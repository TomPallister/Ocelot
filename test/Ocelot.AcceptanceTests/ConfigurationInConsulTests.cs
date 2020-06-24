namespace Ocelot.AcceptanceTests
{
    using Configuration.File;
    using Consul;
    using IdentityServer4.Extensions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Hosting;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text;
    using TestStack.BDDfy;
    using Xunit;

    public class ConfigurationInConsulTests : IDisposable
    {
        private IHost _builder;
        private readonly Steps _steps;
        private IHost _fakeConsulBuilder;
        private FileConfiguration _config;
        private readonly List<ServiceEntry> _consulServices;

        public ConfigurationInConsulTests()
        {
            _consulServices = new List<ServiceEntry>();
            _steps = new Steps();
        }

        [Fact]
        public void should_return_response_200_with_simple_url_when_using_jsonserialized_cache()
        {
            int consulPort = RandomPortFinder.GetRandomPort();
            int servicePort = RandomPortFinder.GetRandomPort();

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
                                    Port = servicePort,
                                }
                            },
                            UpstreamPathTemplate = "/",
                            UpstreamHttpMethod = new List<string> { "Get" },
                        }
                    },
                GlobalConfiguration = new FileGlobalConfiguration()
                {
                    ServiceDiscoveryProvider = new FileServiceDiscoveryProvider()
                    {
                        Scheme = "http",
                        Host = "localhost",
                        Port = consulPort
                    }
                }
            };

            var fakeConsulServiceDiscoveryUrl = $"http://localhost:{consulPort}";

            this.Given(x => GivenThereIsAFakeConsulServiceDiscoveryProvider(fakeConsulServiceDiscoveryUrl, ""))
                .And(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{servicePort}", "", 200, "Hello from Laura"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunningUsingConsulToStoreConfigAndJsonSerializedCache())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from Laura"))
                .BDDfy();
        }

        private void GivenThereIsAFakeConsulServiceDiscoveryProvider(string url, string serviceName)
        {
            _fakeConsulBuilder = Host.CreateDefaultBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder.UseUrls(url)
                            .UseKestrel()
                            .UseContentRoot(Directory.GetCurrentDirectory())
                            .UseIISIntegration()
                            .UseUrls(url)
                            .Configure(app =>
                            {
                                app.Run(async context =>
                                {
                                    if (context.Request.Method.ToLower() == "get" && context.Request.Path.Value == "/v1/kv/InternalConfiguration")
                                    {
                                        var json = JsonConvert.SerializeObject(_config);

                                        var bytes = Encoding.UTF8.GetBytes(json);

                                        var base64 = Convert.ToBase64String(bytes);

                                        var kvp = new FakeConsulGetResponse(base64);

                                        await context.Response.WriteJsonAsync(new FakeConsulGetResponse[] { kvp });
                                    }
                                    else if (context.Request.Method.ToLower() == "put" && context.Request.Path.Value == "/v1/kv/InternalConfiguration")
                                    {
                                        try
                                        {
                                            var reader = new StreamReader(context.Request.Body);

                                            // Synchronous operations are disallowed. Call ReadAsync or set AllowSynchronousIO to true instead.
                                            // var json = reader.ReadToEnd();                                            
                                            var json = await reader.ReadToEndAsync();

                                            _config = JsonConvert.DeserializeObject<FileConfiguration>(json);

                                            var response = JsonConvert.SerializeObject(true);

                                            await context.Response.WriteAsync(response);
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e);
                                            throw;
                                        }
                                    }
                                    else if (context.Request.Path.Value == $"/v1/health/service/{serviceName}")
                                    {
                                        await context.Response.WriteJsonAsync(_consulServices);
                                    }
                                });
                            });
                }).Build();

            _fakeConsulBuilder.Start();
        }

        public class FakeConsulGetResponse
        {
            public FakeConsulGetResponse(string value)
            {
                Value = value;
            }

            public int CreateIndex => 100;
            public int ModifyIndex => 200;
            public int LockIndex => 200;
            public string Key => "InternalConfiguration";
            public int Flags => 0;
            public string Value { get; private set; }
            public string Session => "adf4238a-882b-9ddc-4a9d-5b6758e4159e";
        }

        private void GivenThereIsAServiceRunningOn(string url, string basePath, int statusCode, string responseBody)
        {
            _builder = Host.CreateDefaultBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder.UseUrls(url)
                    .UseKestrel()
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseIISIntegration()
                    .UseUrls(url)
                    .Configure(app =>
                    {
                        app.UsePathBase(basePath);
                        app.Run(async context =>
                        {
                            context.Response.StatusCode = statusCode;
                            await context.Response.WriteAsync(responseBody);
                        });
                    });
                })
                .Build();

            _builder.Start();
        }

        public void Dispose()
        {
            _builder?.Dispose();
            _steps.Dispose();
        }
    }
}

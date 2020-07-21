namespace Ocelot.AcceptanceTests
{
    using Cache;
    using Configuration.File;
    using Consul;
    using Infrastructure;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text;
    using TestStack.BDDfy;
    using Xunit;

    public class ConsulConfigurationInConsulTests : IDisposable
    {
        private IWebHost _builder;
        private readonly Steps _steps;
        private IWebHost _fakeConsulBuilder;
        private FileConfiguration _config;
        private readonly List<ServiceEntry> _consulServices;

        public ConsulConfigurationInConsulTests()
        {
            _consulServices = new List<ServiceEntry>();
            _steps = new Steps();
        }

        [Fact]
        public void should_return_response_200_with_simple_url()
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
                .And(x => _steps.GivenOcelotIsRunningUsingConsulToStoreConfig())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from Laura"))
                .BDDfy();
        }

        [Fact]
        public void should_load_configuration_out_of_consul()
        {
            var consulPort = RandomPortFinder.GetRandomPort();
            int servicePort = RandomPortFinder.GetRandomPort();

            var configuration = new FileConfiguration
            {
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

            var consulConfig = new FileConfiguration
            {
                Routes = new List<FileRoute>
                {
                    new FileRoute
                    {
                        DownstreamPathTemplate = "/status",
                        DownstreamScheme = "http",
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "localhost",
                                Port = servicePort,
                            }
                        },
                        UpstreamPathTemplate = "/cs/status",
                        UpstreamHttpMethod = new List<string> {"Get"}
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

            this.Given(x => GivenTheConsulConfigurationIs(consulConfig))
                .And(x => GivenThereIsAFakeConsulServiceDiscoveryProvider(fakeConsulServiceDiscoveryUrl, ""))
                .And(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{servicePort}", "/status", 200, "Hello from Laura"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunningUsingConsulToStoreConfig())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/cs/status"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from Laura"))
                .BDDfy();
        }

        [Fact]
        public void should_load_configuration_out_of_consul_if_it_is_changed()
        {
            var consulPort = RandomPortFinder.GetRandomPort();
            int servicePort = RandomPortFinder.GetRandomPort();

            var configuration = new FileConfiguration
            {
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

            var consulConfig = new FileConfiguration
            {
                Routes = new List<FileRoute>
                {
                    new FileRoute
                    {
                        DownstreamPathTemplate = "/status",
                        DownstreamScheme = "http",
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "localhost",
                                Port = servicePort,
                            }
                        },
                        UpstreamPathTemplate = "/cs/status",
                        UpstreamHttpMethod = new List<string> {"Get"}
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

            var secondConsulConfig = new FileConfiguration
            {
                Routes = new List<FileRoute>
                {
                    new FileRoute
                    {
                        DownstreamPathTemplate = "/status",
                        DownstreamScheme = "http",
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "localhost",
                                Port = servicePort,
                            }
                        },
                        UpstreamPathTemplate = "/cs/status/awesome",
                        UpstreamHttpMethod = new List<string> {"Get"}
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

            this.Given(x => GivenTheConsulConfigurationIs(consulConfig))
                .And(x => GivenThereIsAFakeConsulServiceDiscoveryProvider(fakeConsulServiceDiscoveryUrl, ""))
                .And(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{servicePort}", "/status", 200, "Hello from Laura"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunningUsingConsulToStoreConfig())
                .And(x => _steps.WhenIGetUrlOnTheApiGateway("/cs/status"))
                .And(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from Laura"))
                .When(x => GivenTheConsulConfigurationIs(secondConsulConfig))
                .Then(x => ThenTheConfigIsUpdatedInOcelot())
                .BDDfy();
        }

        [Fact]
        public void should_handle_request_to_consul_for_downstream_service_and_make_request_no_re_routes_and_rate_limit()
        {
            int consulPort = RandomPortFinder.GetRandomPort();
            const string serviceName = "web";
            int downstreamServicePort = RandomPortFinder.GetRandomPort();
            var downstreamServiceOneUrl = $"http://localhost:{downstreamServicePort}";
            var fakeConsulServiceDiscoveryUrl = $"http://localhost:{consulPort}";
            var serviceEntryOne = new ServiceEntry()
            {
                Service = new AgentService()
                {
                    Service = serviceName,
                    Address = "localhost",
                    Port = downstreamServicePort,
                    ID = "web_90_0_2_224_8080",
                    Tags = new[] { "version-v1" }
                },
            };

            var consulConfig = new FileConfiguration
            {
                DynamicRoutes = new List<FileDynamicRoute>
                {
                    new FileDynamicRoute
                    {
                        ServiceName = serviceName,
                        RateLimitRule = new FileRateLimitRule()
                        {
                            EnableRateLimiting = true,
                            ClientWhitelist = new List<string>(),
                            Limit = 3,
                            Period = "1s",
                            PeriodTimespan = 1000
                        }
                    }
                },
                GlobalConfiguration = new FileGlobalConfiguration
                {
                    ServiceDiscoveryProvider = new FileServiceDiscoveryProvider
                    {
                        Scheme = "http",
                        Host = "localhost",
                        Port = consulPort
                    },
                    RateLimitOptions = new FileRateLimitOptions()
                    {
                        ClientIdHeader = "ClientId",
                        DisableRateLimitHeaders = false,
                        QuotaExceededMessage = "",
                        RateLimitCounterPrefix = "",
                        HttpStatusCode = 428
                    },
                    DownstreamScheme = "http",
                }
            };

            var configuration = new FileConfiguration
            {
                GlobalConfiguration = new FileGlobalConfiguration
                {
                    ServiceDiscoveryProvider = new FileServiceDiscoveryProvider
                    {
                        Scheme = "http",
                        Host = "localhost",
                        Port = consulPort
                    }
                }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn(downstreamServiceOneUrl, "/something", 200, "Hello from Laura"))
            .And(x => GivenTheConsulConfigurationIs(consulConfig))
            .And(x => x.GivenThereIsAFakeConsulServiceDiscoveryProvider(fakeConsulServiceDiscoveryUrl, serviceName))
            .And(x => x.GivenTheServicesAreRegisteredWithConsul(serviceEntryOne))
            .And(x => _steps.GivenThereIsAConfiguration(configuration))
            .And(x => _steps.GivenOcelotIsRunningUsingConsulToStoreConfig())
            .When(x => _steps.WhenIGetUrlOnTheApiGatewayMultipleTimesForRateLimit("/web/something", 1))
            .Then(x => _steps.ThenTheStatusCodeShouldBe(200))
            .When(x => _steps.WhenIGetUrlOnTheApiGatewayMultipleTimesForRateLimit("/web/something", 2))
            .Then(x => _steps.ThenTheStatusCodeShouldBe(200))
            .When(x => _steps.WhenIGetUrlOnTheApiGatewayMultipleTimesForRateLimit("/web/something", 1))
            .Then(x => _steps.ThenTheStatusCodeShouldBe(428))
            .BDDfy();
        }

        private void ThenTheConfigIsUpdatedInOcelot()
        {
            var result = Wait.WaitFor(20000).Until(() =>
            {
                try
                {
                    _steps.WhenIGetUrlOnTheApiGateway("/cs/status/awesome");
                    _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK);
                    _steps.ThenTheResponseBodyShouldBe("Hello from Laura");
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            });
            result.ShouldBeTrue();
        }

        private void GivenTheConsulConfigurationIs(FileConfiguration config)
        {
            _config = config;
        }

        private void GivenTheServicesAreRegisteredWithConsul(params ServiceEntry[] serviceEntries)
        {
            foreach (var serviceEntry in serviceEntries)
            {
                _consulServices.Add(serviceEntry);
            }
        }

        private void GivenThereIsAFakeConsulServiceDiscoveryProvider(string url, string serviceName)
        {
            _fakeConsulBuilder = new WebHostBuilder()
                            .UseUrls(url)
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
                                        json = JsonConvert.SerializeObject(new FakeConsulGetResponse[] { kvp });
                                        context.Response.Headers.Add("Content-Type", "application/json");
                                        await context.Response.WriteAsync(json);
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
                                        var json = JsonConvert.SerializeObject(_consulServices);
                                        context.Response.Headers.Add("Content-Type", "application/json");
                                        await context.Response.WriteAsync(json);
                                    }
                                });
                            })
                            .Build();

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
            _builder = new WebHostBuilder()
                .UseUrls(url)
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
                })
                .Build();

            _builder.Start();
        }

        public void Dispose()
        {
            _builder?.Dispose();
            _steps.Dispose();
        }

        private class FakeCache : IOcelotCache<FileConfiguration>
        {
            public void Add(string key, FileConfiguration value, TimeSpan ttl, string region)
            {
                throw new NotImplementedException();
            }

            public FileConfiguration Get(string key, string region)
            {
                throw new NotImplementedException();
            }

            public void ClearRegion(string region)
            {
                throw new NotImplementedException();
            }

            public void AddAndDelete(string key, FileConfiguration value, TimeSpan ttl, string region)
            {
                throw new NotImplementedException();
            }
        }
    }
}

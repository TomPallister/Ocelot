namespace Ocelot.AcceptanceTests
{
    using Microsoft.AspNetCore.Http;
    using Ocelot.Configuration.File;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TestStack.BDDfy;
    using Xunit;

    public class ClientRateLimitTests : IDisposable
    {
        private readonly Steps _steps;
        private int _counterOne;
        private readonly ServiceHandler _serviceHandler;

        public ClientRateLimitTests()
        {
            _serviceHandler = new ServiceHandler();
            _steps = new Steps();
        }

        [Fact]
        public void should_call_withratelimiting()
        {
            int port = RandomPortFinder.GetRandomPort();

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                    {
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/api/ClientRateLimit",
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = port,
                                }
                            },
                            DownstreamScheme = "http",
                            UpstreamPathTemplate = "/api/ClientRateLimit",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            RequestIdKey = _steps.RequestIdKey,
                            RateLimitOptions = new FileRateLimitRule()
                            {
                                EnableRateLimiting = true,
                                ClientWhitelist = new List<string>(),
                                Limit = 3,
                                Period = "1s",
                                PeriodTimespan = 1000
                            }
                        }
                },
                GlobalConfiguration = new FileGlobalConfiguration()
                {
                    RateLimitOptions = new FileRateLimitOptions()
                    {
                        ClientIdHeader = "ClientId",
                        DisableRateLimitHeaders = false,
                        QuotaExceededMessage = "",
                        RateLimitCounterPrefix = "",
                        HttpStatusCode = 428
                    },
                    RequestIdKey = "oceclientrequest"
                }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/api/ClientRateLimit"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .When(x => _steps.WhenIGetUrlOnTheApiGatewayMultipleTimesForRateLimit("/api/ClientRateLimit", 1))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(200))
                .When(x => _steps.WhenIGetUrlOnTheApiGatewayMultipleTimesForRateLimit("/api/ClientRateLimit", 2))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(200))
                .When(x => _steps.WhenIGetUrlOnTheApiGatewayMultipleTimesForRateLimit("/api/ClientRateLimit", 1))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(428))
                .BDDfy();
        }

        [Fact]
        public void should_wait_for_period_timespan_to_elapse_before_making_next_request()
        {
            int port = RandomPortFinder.GetRandomPort();

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                    {
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/api/ClientRateLimit",
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = port,
                                }
                            },
                            DownstreamScheme = "http",
                            UpstreamPathTemplate = "/api/ClientRateLimit",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            RequestIdKey = _steps.RequestIdKey,

                            RateLimitOptions = new FileRateLimitRule()
                            {
                                EnableRateLimiting = true,
                                ClientWhitelist = new List<string>(),
                                Limit = 3,
                                Period = "1s",
                                PeriodTimespan = 2
                            }
                        }
                },
                GlobalConfiguration = new FileGlobalConfiguration()
                {
                    RateLimitOptions = new FileRateLimitOptions()
                    {
                        ClientIdHeader = "ClientId",
                        DisableRateLimitHeaders = false,
                        QuotaExceededMessage = "",
                        RateLimitCounterPrefix = "",
                        HttpStatusCode = 428
                    },
                    RequestIdKey = "oceclientrequest"
                }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/api/ClientRateLimit"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .When(x => _steps.WhenIGetUrlOnTheApiGatewayMultipleTimesForRateLimit("/api/ClientRateLimit", 1))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(200))
                .When(x => _steps.WhenIGetUrlOnTheApiGatewayMultipleTimesForRateLimit("/api/ClientRateLimit", 2))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(200))
                .When(x => _steps.WhenIGetUrlOnTheApiGatewayMultipleTimesForRateLimit("/api/ClientRateLimit", 1))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(428))
                .And(x => _steps.GivenIWait(1000))
                .When(x => _steps.WhenIGetUrlOnTheApiGatewayMultipleTimesForRateLimit("/api/ClientRateLimit", 1))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(428))
                .And(x => _steps.GivenIWait(1000))
                .When(x => _steps.WhenIGetUrlOnTheApiGatewayMultipleTimesForRateLimit("/api/ClientRateLimit", 1))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(200))
                .BDDfy();
        }

        [Fact]
        public void should_call_middleware_withWhitelistClient()
        {
            int port = RandomPortFinder.GetRandomPort();

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                    {
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/api/ClientRateLimit",
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = port,
                                }
                            },
                            DownstreamScheme = "http",
                            UpstreamPathTemplate = "/api/ClientRateLimit",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            RequestIdKey = _steps.RequestIdKey,

                            RateLimitOptions = new FileRateLimitRule()
                            {
                                EnableRateLimiting = true,
                                ClientWhitelist = new List<string>() { "ocelotclient1"},
                                Limit = 3,
                                Period = "1s",
                                PeriodTimespan = 100
                            }
                        }
                },
                GlobalConfiguration = new FileGlobalConfiguration()
                {
                    RateLimitOptions = new FileRateLimitOptions()
                    {
                        ClientIdHeader = "ClientId",
                        DisableRateLimitHeaders = false,
                        QuotaExceededMessage = "",
                        RateLimitCounterPrefix = ""
                    },
                    RequestIdKey = "oceclientrequest"
                }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/api/ClientRateLimit"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .When(x => _steps.WhenIGetUrlOnTheApiGatewayMultipleTimesForRateLimit("/api/ClientRateLimit", 4))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(200))
                .BDDfy();
        }

        private void GivenThereIsAServiceRunningOn(string baseUrl, string basePath)
        {
            _serviceHandler.GivenThereIsAServiceRunningOn(baseUrl, basePath, context =>
            {
                _counterOne++;
                context.Response.StatusCode = 200;
                context.Response.WriteAsync(_counterOne.ToString());
                return Task.CompletedTask;
            });
        }

        public void Dispose()
        {
            _steps.Dispose();
        }
    }
}

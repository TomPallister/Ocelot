namespace Ocelot.AcceptanceTests
{
    using Microsoft.AspNetCore.Http;
    using Ocelot.Configuration.File;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using TestStack.BDDfy;
    using Xunit;

    public class StickySessionsTests : IDisposable
    {
        private readonly Steps _steps;
        private int _counterOne;
        private int _counterTwo;
        private static readonly object SyncLock = new object();
        private readonly ServiceHandler _serviceHandler;

        public StickySessionsTests()
        {
            _serviceHandler = new ServiceHandler();
            _steps = new Steps();
        }

        [Fact]
        public void should_use_same_downstream_host()
        {
            var downstreamPortOne = RandomPortFinder.GetRandomPort();
            var downstreamPortTwo = RandomPortFinder.GetRandomPort();
            var downstreamServiceOneUrl = $"http://localhost:{downstreamPortOne}";
            var downstreamServiceTwoUrl = $"http://localhost:{downstreamPortTwo}";

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                    {
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/",
                            DownstreamScheme = "http",
                            UpstreamPathTemplate = "/",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            LoadBalancerOptions = new FileLoadBalancerOptions
                            {
                                Type = "CookieStickySessions",
                                Key = "sessionid",
                                Expiry = 300000
                            },
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = downstreamPortOne
                                },
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = downstreamPortTwo
                                }
                            }
                        }
                    }
            };

            this.Given(x => x.GivenProductServiceOneIsRunning(downstreamServiceOneUrl, 200))
                .And(x => x.GivenProductServiceTwoIsRunning(downstreamServiceTwoUrl, 200))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .When(x => _steps.WhenIGetUrlOnTheApiGatewayMultipleTimes("/", 10, "sessionid", "123"))
                .Then(x => x.ThenTheFirstServiceIsCalled(10))
                .Then(x => x.ThenTheSecondServiceIsCalled(0))
                .BDDfy();
        }

        [Fact]
        public void should_use_different_downstream_host_for_different_re_route()
        {
            var downstreamPortOne = RandomPortFinder.GetRandomPort();
            var downstreamPortTwo = RandomPortFinder.GetRandomPort();
            var downstreamServiceOneUrl = $"http://localhost:{downstreamPortOne}";
            var downstreamServiceTwoUrl = $"http://localhost:{downstreamPortTwo}";

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                    {
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/",
                            DownstreamScheme = "http",
                            UpstreamPathTemplate = "/",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            LoadBalancerOptions = new FileLoadBalancerOptions
                            {
                                Type = "CookieStickySessions",
                                Key = "sessionid",
                                Expiry = 300000
                            },
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = downstreamPortOne
                                },
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = downstreamPortTwo
                                }
                            }
                        },
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/",
                            DownstreamScheme = "http",
                            UpstreamPathTemplate = "/test",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            LoadBalancerOptions = new FileLoadBalancerOptions
                            {
                                Type = "CookieStickySessions",
                                Key = "bestid",
                                Expiry = 300000
                            },
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = downstreamPortTwo
                                },
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = downstreamPortOne
                                }
                            }
                        }
                    }
            };

            this.Given(x => x.GivenProductServiceOneIsRunning(downstreamServiceOneUrl, 200))
                .And(x => x.GivenProductServiceTwoIsRunning(downstreamServiceTwoUrl, 200))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/", "sessionid", "123"))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/test", "bestid", "123"))
                .Then(x => x.ThenTheFirstServiceIsCalled(1))
                .Then(x => x.ThenTheSecondServiceIsCalled(1))
                .BDDfy();
        }

        [Fact]
        public void should_use_same_downstream_host_for_different_re_route()
        {
            var downstreamPortOne = RandomPortFinder.GetRandomPort();
            var downstreamPortTwo = RandomPortFinder.GetRandomPort();
            var downstreamServiceOneUrl = $"http://localhost:{downstreamPortOne}";
            var downstreamServiceTwoUrl = $"http://localhost:{downstreamPortTwo}";

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                    {
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/",
                            DownstreamScheme = "http",
                            UpstreamPathTemplate = "/",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            LoadBalancerOptions = new FileLoadBalancerOptions
                            {
                                Type = "CookieStickySessions",
                                Key = "sessionid",
                                Expiry = 300000
                            },
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = downstreamPortOne
                                },
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = downstreamPortTwo
                                }
                            }
                        },
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/",
                            DownstreamScheme = "http",
                            UpstreamPathTemplate = "/test",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            LoadBalancerOptions = new FileLoadBalancerOptions
                            {
                                Type = "CookieStickySessions",
                                Key = "sessionid",
                                Expiry = 300000
                            },
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = downstreamPortTwo
                                },
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = downstreamPortOne
                                }
                            }
                        }
                    }
            };

            this.Given(x => x.GivenProductServiceOneIsRunning(downstreamServiceOneUrl, 200))
                .And(x => x.GivenProductServiceTwoIsRunning(downstreamServiceTwoUrl, 200))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/", "sessionid", "123"))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/test", "sessionid", "123"))
                .Then(x => x.ThenTheFirstServiceIsCalled(2))
                .Then(x => x.ThenTheSecondServiceIsCalled(0))
                .BDDfy();
        }

        private void ThenTheFirstServiceIsCalled(int expected)
        {
            _counterOne.ShouldBe(expected);
        }

        private void ThenTheSecondServiceIsCalled(int expected)
        {
            _counterTwo.ShouldBe(expected);
        }

        private void GivenProductServiceOneIsRunning(string url, int statusCode)
        {
            _serviceHandler.GivenThereIsAServiceRunningOn(url, async context =>
            {
                try
                {
                    var response = string.Empty;
                    lock (SyncLock)
                    {
                        _counterOne++;
                        response = _counterOne.ToString();
                    }
                    context.Response.StatusCode = statusCode;
                    await context.Response.WriteAsync(response);
                }
                catch (Exception exception)
                {
                    await context.Response.WriteAsync(exception.StackTrace);
                }
            });
        }

        private void GivenProductServiceTwoIsRunning(string url, int statusCode)
        {
            _serviceHandler.GivenThereIsAServiceRunningOn(url, async context =>
            {
                try
                {
                    var response = string.Empty;
                    lock (SyncLock)
                    {
                        _counterTwo++;
                        response = _counterTwo.ToString();
                    }

                    context.Response.StatusCode = statusCode;
                    await context.Response.WriteAsync(response);
                }
                catch (Exception exception)
                {
                    await context.Response.WriteAsync(exception.StackTrace);
                }
            });
        }

        public void Dispose()
        {
            _serviceHandler?.Dispose();
            _steps.Dispose();
        }
    }
}

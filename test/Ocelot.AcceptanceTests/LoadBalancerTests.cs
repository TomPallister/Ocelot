namespace Ocelot.AcceptanceTests
{
    using Microsoft.AspNetCore.Http;
    using Ocelot.Configuration.File;
    using Ocelot.LoadBalancer.LoadBalancers;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Configuration;
    using Middleware;
    using Responses;
    using ServiceDiscovery.Providers;
    using TestStack.BDDfy;
    using Values;
    using Xunit;

    public class LoadBalancerTests : IDisposable
    {
        private readonly Steps _steps;
        private int _counterOne;
        private int _counterTwo;
        private static readonly object _syncLock = new object();
        private readonly ServiceHandler _serviceHandler;

        public LoadBalancerTests()
        {
            _serviceHandler = new ServiceHandler();
            _steps = new Steps();
        }

        [Fact]
        public void should_load_balance_request_with_least_connection()
        {
            int portOne = RandomPortFinder.GetRandomPort();
            int portTwo = RandomPortFinder.GetRandomPort();

            var downstreamServiceOneUrl = $"http://localhost:{portOne}";
            var downstreamServiceTwoUrl = $"http://localhost:{portTwo}";

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
                            LoadBalancerOptions = new FileLoadBalancerOptions { Type = nameof(LeastConnection) },
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = portOne
                                },
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = portTwo
                                }
                            }
                        }
                    },
                GlobalConfiguration = new FileGlobalConfiguration()
                {
                }
            };

            this.Given(x => x.GivenProductServiceOneIsRunning(downstreamServiceOneUrl, 200))
                .And(x => x.GivenProductServiceTwoIsRunning(downstreamServiceTwoUrl, 200))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .When(x => _steps.WhenIGetUrlOnTheApiGatewayMultipleTimes("/", 50))
                .Then(x => x.ThenTheTwoServicesShouldHaveBeenCalledTimes(50))
                .And(x => x.ThenBothServicesCalledRealisticAmountOfTimes(24, 26))
                .BDDfy();
        }

        [Fact]
        public void should_load_balance_request_with_round_robin()
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
                            LoadBalancerOptions = new FileLoadBalancerOptions { Type = nameof(RoundRobin) },
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
                    },
                GlobalConfiguration = new FileGlobalConfiguration()
                {
                }
            };

            this.Given(x => x.GivenProductServiceOneIsRunning(downstreamServiceOneUrl, 200))
                .And(x => x.GivenProductServiceTwoIsRunning(downstreamServiceTwoUrl, 200))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .When(x => _steps.WhenIGetUrlOnTheApiGatewayMultipleTimes("/", 50))
                .Then(x => x.ThenTheTwoServicesShouldHaveBeenCalledTimes(50))
                .And(x => x.ThenBothServicesCalledRealisticAmountOfTimes(24, 26))
                .BDDfy();
        }

        [Fact]
        public void should_load_balance_request_with_custom_load_balancer()
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
                            LoadBalancerOptions = new FileLoadBalancerOptions { Type = nameof(CustomLoadBalancer) },
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = downstreamPortOne,
                                },
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = downstreamPortTwo,
                                },
                            },
                        },
                    },
                GlobalConfiguration = new FileGlobalConfiguration(),
            };

            Func<IServiceProvider, DownstreamRoute, IServiceDiscoveryProvider, CustomLoadBalancer> loadBalancerFactoryFunc = (serviceProvider, route, serviceDiscoveryProvider) => new CustomLoadBalancer(serviceDiscoveryProvider.Get);

            this.Given(x => x.GivenProductServiceOneIsRunning(downstreamServiceOneUrl, 200))
                .And(x => x.GivenProductServiceTwoIsRunning(downstreamServiceTwoUrl, 200))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunningWithCustomLoadBalancer(loadBalancerFactoryFunc))
                .When(x => _steps.WhenIGetUrlOnTheApiGatewayMultipleTimes("/", 50))
                .Then(x => x.ThenTheTwoServicesShouldHaveBeenCalledTimes(50))
                .And(x => x.ThenBothServicesCalledRealisticAmountOfTimes(24, 26))
                .BDDfy();
        }

        private class CustomLoadBalancer : ILoadBalancer
        {
            private readonly Func<Task<List<Service>>> _services;
            private readonly object _lock = new object();

            private int _last;

            public CustomLoadBalancer(Func<Task<List<Service>>> services)
            {
                _services = services;
            }

            public async Task<Response<ServiceHostAndPort>> Lease(HttpContext httpContext)
            {
                var services = await _services();
                lock (_lock)
                {
                    if (_last >= services.Count)
                    {
                        _last = 0;
                    }

                    var next = services[_last];
                    _last++;
                    return new OkResponse<ServiceHostAndPort>(next.HostAndPort);
                }
            }

        public void Release(ServiceHostAndPort hostAndPort)
            {
            }
        }

        private void ThenBothServicesCalledRealisticAmountOfTimes(int bottom, int top)
        {
            _counterOne.ShouldBeInRange(bottom, top);
            _counterOne.ShouldBeInRange(bottom, top);
        }

        private void ThenTheTwoServicesShouldHaveBeenCalledTimes(int expected)
        {
            var total = _counterOne + _counterTwo;
            total.ShouldBe(expected);
        }

        private void GivenProductServiceOneIsRunning(string url, int statusCode)
        {
            _serviceHandler.GivenThereIsAServiceRunningOn(url, async context =>
            {
                try
                {
                    var response = string.Empty;
                    lock (_syncLock)
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
                    lock (_syncLock)
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

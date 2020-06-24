namespace Ocelot.AcceptanceTests
{
    using Ocelot.Configuration.File;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using Xunit;

    public class CannotStartOcelotTests : IDisposable
    {
        private readonly Steps _steps;

        public CannotStartOcelotTests()
        {
            _steps = new Steps();
        }

        [Fact]
        public void should_throw_exception_if_cannot_start_because_service_discovery_provider_specified_in_config_but_no_service_discovery_provider_registered_with_dynamic_re_routes()
        {
            var invalidConfig = new FileConfiguration
            {
                GlobalConfiguration = new FileGlobalConfiguration
                {
                    ServiceDiscoveryProvider = new FileServiceDiscoveryProvider
                    {
                        Scheme = "https",
                        Host = "localhost",
                        Type = "consul",
                        Port = 8500
                    }
                }
            };

            Exception exception = null;
            _steps.GivenThereIsAConfiguration(invalidConfig);
            try
            {
                _steps.GivenOcelotIsRunning();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            exception.ShouldNotBeNull();
            exception.Message.ShouldBe("One or more errors occurred. (Unable to start Ocelot, errors are: Unable to start Ocelot, errors are: Unable to start Ocelot because either a Route or GlobalConfiguration are using ServiceDiscoveryOptions but no ServiceDiscoveryFinderDelegate has been registered in dependency injection container. Are you missing a package like Ocelot.Provider.Consul and services.AddConsul() or Ocelot.Provider.Eureka and services.AddEureka()?)");
        }

        [Fact]
        public void should_throw_exception_if_cannot_start_because_service_discovery_provider_specified_in_config_but_no_service_discovery_provider_registered()
        {
            var invalidConfig = new FileConfiguration
            {
                Routes = new List<FileRoute>
                {
                    new FileRoute
                    {
                        DownstreamPathTemplate = "/",
                        DownstreamScheme = "http",
                        UpstreamPathTemplate = "/laura",
                        UpstreamHttpMethod = new List<string> { "Get" },
                        ServiceName = "test"
                    }
                },
                GlobalConfiguration = new FileGlobalConfiguration
                {
                    ServiceDiscoveryProvider = new FileServiceDiscoveryProvider
                    {
                        Scheme = "https",
                        Host = "localhost",
                        Type = "consul",
                        Port = 8500
                    }
                }
            };

            Exception exception = null;
            _steps.GivenThereIsAConfiguration(invalidConfig);
            try
            {
                _steps.GivenOcelotIsRunning();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            exception.ShouldNotBeNull();
            exception.Message.ShouldBe("One or more errors occurred. (Unable to start Ocelot, errors are: Unable to start Ocelot, errors are: Unable to start Ocelot because either a Route or GlobalConfiguration are using ServiceDiscoveryOptions but no ServiceDiscoveryFinderDelegate has been registered in dependency injection container. Are you missing a package like Ocelot.Provider.Consul and services.AddConsul() or Ocelot.Provider.Eureka and services.AddEureka()?,Unable to start Ocelot, errors are: Unable to start Ocelot because either a Route or GlobalConfiguration are using ServiceDiscoveryOptions but no ServiceDiscoveryFinderDelegate has been registered in dependency injection container. Are you missing a package like Ocelot.Provider.Consul and services.AddConsul() or Ocelot.Provider.Eureka and services.AddEureka()?)");
        }

        [Fact]
        public void should_throw_exception_if_cannot_start_because_no_qos_delegate_registered_globally()
        {
            var invalidConfig = new FileConfiguration
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
                                Port = 51878,
                            }
                        },
                        UpstreamPathTemplate = "/laura",
                        UpstreamHttpMethod = new List<string> { "Get" },
                        Key = "Laura",
                    }
                },
                GlobalConfiguration = new FileGlobalConfiguration
                {
                    QoSOptions = new FileQoSOptions
                    {
                        TimeoutValue = 1,
                        ExceptionsAllowedBeforeBreaking = 1
                    }
                }
            };

            Exception exception = null;
            _steps.GivenThereIsAConfiguration(invalidConfig);
            try
            {
                _steps.GivenOcelotIsRunning();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            exception.ShouldNotBeNull();
            exception.Message.ShouldBe("One or more errors occurred. (Unable to start Ocelot, errors are: Unable to start Ocelot because either a Route or GlobalConfiguration are using QoSOptions but no QosDelegatingHandlerDelegate has been registered in dependency injection container. Are you missing a package like Ocelot.Provider.Polly and services.AddPolly()?)");
        }

        [Fact]
        public void should_throw_exception_if_cannot_start_because_no_qos_delegate_registered_for_re_route()
        {
            var invalidConfig = new FileConfiguration
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
                                Port = 51878,
                            }
                        },
                        UpstreamPathTemplate = "/laura",
                        UpstreamHttpMethod = new List<string> { "Get" },
                        Key = "Laura",
                        QoSOptions = new FileQoSOptions
                        {
                            TimeoutValue = 1,
                            ExceptionsAllowedBeforeBreaking = 1
                        }
                    }
                }
            };

            Exception exception = null;
            _steps.GivenThereIsAConfiguration(invalidConfig);
            try
            {
                _steps.GivenOcelotIsRunning();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            exception.ShouldNotBeNull();
            exception.Message.ShouldBe("One or more errors occurred. (Unable to start Ocelot, errors are: Unable to start Ocelot because either a Route or GlobalConfiguration are using QoSOptions but no QosDelegatingHandlerDelegate has been registered in dependency injection container. Are you missing a package like Ocelot.Provider.Polly and services.AddPolly()?)");
        }

        [Fact]
        public void should_throw_exception_if_cannot_start()
        {
            var invalidConfig = new FileConfiguration()
            {
                Routes = new List<FileRoute>
                {
                    new FileRoute
                    {
                        UpstreamPathTemplate = "api",
                        DownstreamPathTemplate = "test"
                    }
                }
            };

            Exception exception = null;
            _steps.GivenThereIsAConfiguration(invalidConfig);
            try
            {
                _steps.GivenOcelotIsRunning();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            exception.ShouldNotBeNull();
            exception.Message.ShouldBe("One or more errors occurred. (Unable to start Ocelot, errors are: Downstream Path Template test doesnt start with forward slash,Upstream Path Template api doesnt start with forward slash,When not using service discovery DownstreamHostAndPorts must be set and not empty or Ocelot cannot find your service!)");
        }

        public void Dispose()
        {
            _steps.Dispose();
        }
    }
}

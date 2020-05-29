namespace Ocelot.AcceptanceTests
{
    using Microsoft.AspNetCore.Http;
    using Ocelot.Configuration.File;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using TestStack.BDDfy;
    using Xunit;

    public class HeaderTests : IDisposable
    {
        private int _count;
        private readonly Steps _steps;
        private readonly ServiceHandler _serviceHandler;

        public HeaderTests()
        {
            _serviceHandler = new ServiceHandler();
            _steps = new Steps();
        }

        [Fact]
        public void should_transform_upstream_header()
        {
            var port = RandomPortFinder.GetRandomPort();

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
                                    Port = port,
                                }
                            },
                            UpstreamPathTemplate = "/",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            UpstreamHeaderTransform = new Dictionary<string,string>
                            {
                                {"Laz", "D, GP"}
                            }
                        }
                    }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/", 200, "Laz"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .And(x => _steps.GivenIAddAHeader("Laz", "D"))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("GP"))
                .BDDfy();
        }

        [Fact]
        public void should_transform_downstream_header()
        {
            var port = RandomPortFinder.GetRandomPort();

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
                                    Port = port,
                                }
                            },
                            UpstreamPathTemplate = "/",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            DownstreamHeaderTransform = new Dictionary<string,string>
                            {
                                {"Location", "http://www.bbc.co.uk/, http://ocelot.com/"}
                            }
                        }
                    }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/", 200, "Location", "http://www.bbc.co.uk/"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseHeaderIs("Location", "http://ocelot.com/"))
                .BDDfy();
        }

        [Fact]
        public void should_fix_issue_190()
        {
            var port = RandomPortFinder.GetRandomPort();

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
                                    Port = port,
                                }
                            },
                            UpstreamPathTemplate = "/",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            DownstreamHeaderTransform = new Dictionary<string,string>
                            {
                                {"Location", $"http://localhost:{port}, {{BaseUrl}}"}
                            },
                            HttpHandlerOptions = new FileHttpHandlerOptions
                            {
                                AllowAutoRedirect = false
                            }
                        }
                    }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/", 302, "Location", $"http://localhost:{port}/pay/Receive"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.Redirect))
                .And(x => _steps.ThenTheResponseHeaderIs("Location", "http://localhost:5000/pay/Receive"))
                .BDDfy();
        }

        [Fact]
        public void should_fix_issue_205()
        {
            var port = RandomPortFinder.GetRandomPort();

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
                                    Port = port,
                                }
                            },
                            UpstreamPathTemplate = "/",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            DownstreamHeaderTransform = new Dictionary<string,string>
                            {
                                {"Location", "{DownstreamBaseUrl}, {BaseUrl}"}
                            },
                            HttpHandlerOptions = new FileHttpHandlerOptions
                            {
                                AllowAutoRedirect = false
                            }
                        }
                    }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/", 302, "Location", $"http://localhost:{port}/pay/Receive"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.Redirect))
                .And(x => _steps.ThenTheResponseHeaderIs("Location", "http://localhost:5000/pay/Receive"))
                .BDDfy();
        }

        [Fact]
        public void should_fix_issue_417()
        {
            var port = RandomPortFinder.GetRandomPort();

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
                                    Port = port,
                                }
                            },
                            UpstreamPathTemplate = "/",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            DownstreamHeaderTransform = new Dictionary<string,string>
                            {
                                {"Location", "{DownstreamBaseUrl}, {BaseUrl}"}
                            },
                            HttpHandlerOptions = new FileHttpHandlerOptions
                            {
                                AllowAutoRedirect = false
                            }
                        }
                    },
                GlobalConfiguration = new FileGlobalConfiguration
                {
                    BaseUrl = "http://anotherapp.azurewebsites.net"
                }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/", 302, "Location", $"http://localhost:{port}/pay/Receive"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.Redirect))
                .And(x => _steps.ThenTheResponseHeaderIs("Location", "http://anotherapp.azurewebsites.net/pay/Receive"))
                .BDDfy();
        }

        [Fact]
        public void request_should_reuse_cookies_with_cookie_container()
        {
            var port = RandomPortFinder.GetRandomPort();

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                {
                    new FileRoute
                    {
                        DownstreamPathTemplate = "/sso/{everything}",
                        DownstreamScheme = "http",
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "localhost",
                                Port = port,
                            }
                        },
                        UpstreamPathTemplate = "/sso/{everything}",
                        UpstreamHttpMethod = new List<string> { "Get", "Post", "Options" },
                        HttpHandlerOptions = new FileHttpHandlerOptions
                        {
                            UseCookieContainer = true
                        }
                    }
                }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/sso/test", 200))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .And(x => _steps.WhenIGetUrlOnTheApiGateway("/sso/test"))
                .And(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseHeaderIs("Set-Cookie", "test=0; path=/"))
                .And(x => _steps.GivenIAddCookieToMyRequest("test=1; path=/"))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/sso/test"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .BDDfy();
        }

        [Fact]
        public void request_should_have_own_cookies_no_cookie_container()
        {
            var port = RandomPortFinder.GetRandomPort();

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                {
                    new FileRoute
                    {
                        DownstreamPathTemplate = "/sso/{everything}",
                        DownstreamScheme = "http",
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "localhost",
                                Port = port,
                            }
                        },
                        UpstreamPathTemplate = "/sso/{everything}",
                        UpstreamHttpMethod = new List<string> { "Get", "Post", "Options" },
                        HttpHandlerOptions = new FileHttpHandlerOptions
                        {
                            UseCookieContainer = false
                        }
                    }
                }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/sso/test", 200))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .And(x => _steps.WhenIGetUrlOnTheApiGateway("/sso/test"))
                .And(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseHeaderIs("Set-Cookie", "test=0; path=/"))
                .And(x => _steps.GivenIAddCookieToMyRequest("test=1; path=/"))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/sso/test"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .BDDfy();
        }

        [Fact]
        public void issue_474_should_not_put_spaces_in_header()
        {
            var port = RandomPortFinder.GetRandomPort();

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
                                    Port = port,
                                }
                            },
                            UpstreamPathTemplate = "/",
                            UpstreamHttpMethod = new List<string> { "Get" },
                        }
                    }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/", 200, "Accept"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .And(x => _steps.GivenIAddAHeader("Accept", "text/html,application/xhtml+xml,application/xml;"))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("text/html,application/xhtml+xml,application/xml;"))
                .BDDfy();
        }

        [Fact]
        public void issue_474_should_put_spaces_in_header()
        {
            var port = RandomPortFinder.GetRandomPort();

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
                                    Port = port,
                                }
                            },
                            UpstreamPathTemplate = "/",
                            UpstreamHttpMethod = new List<string> { "Get" },
                        }
                    }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/", 200, "Accept"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .And(x => _steps.GivenIAddAHeader("Accept", "text/html"))
                .And(x => _steps.GivenIAddAHeader("Accept", "application/xhtml+xml"))
                .And(x => _steps.GivenIAddAHeader("Accept", "application/xml"))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("text/html, application/xhtml+xml, application/xml"))
                .BDDfy();
        }

        private void GivenThereIsAServiceRunningOn(string baseUrl, string basePath, int statusCode)
        {
            _serviceHandler.GivenThereIsAServiceRunningOn(baseUrl, basePath, context =>
            {
                if (_count == 0)
                {
                    context.Response.Cookies.Append("test", "0");
                    _count++;
                    context.Response.StatusCode = statusCode;
                    return Task.CompletedTask;
                }

                if (context.Request.Cookies.TryGetValue("test", out var cookieValue) || context.Request.Headers.TryGetValue("Set-Cookie", out var headerValue))
                {
                    if (cookieValue == "0" || headerValue == "test=1; path=/")
                    {
                        context.Response.StatusCode = statusCode;
                        return Task.CompletedTask;
                    }
                }

                context.Response.StatusCode = 500;
                return Task.CompletedTask;
            });
        }

        private void GivenThereIsAServiceRunningOn(string baseUrl, string basePath, int statusCode, string headerKey)
        {
            _serviceHandler.GivenThereIsAServiceRunningOn(baseUrl, basePath, async context =>
            {
                if (context.Request.Headers.TryGetValue(headerKey, out var values))
                {
                    var result = values.First();
                    context.Response.StatusCode = statusCode;
                    await context.Response.WriteAsync(result);
                }
            });
        }

        private void GivenThereIsAServiceRunningOn(string baseUrl, string basePath, int statusCode, string headerKey, string headerValue)
        {
            _serviceHandler.GivenThereIsAServiceRunningOn(baseUrl, basePath, context =>
            {
                context.Response.OnStarting(() =>
                {
                    context.Response.Headers.Add(headerKey, headerValue);
                    context.Response.StatusCode = statusCode;
                    return Task.CompletedTask;
                });

                return Task.CompletedTask;
            });
        }

        public void Dispose()
        {
            _serviceHandler?.Dispose();
            _steps.Dispose();
        }
    }
}

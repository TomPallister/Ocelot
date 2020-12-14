using Microsoft.AspNetCore.Http;
using Ocelot.Configuration.File;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.AcceptanceTests
{
    public class RoutingBasedOnHeadersTests : IDisposable
    {
        private readonly Steps _steps;
        private string _downstreamPath;
        private readonly ServiceHandler _serviceHandler;

        public RoutingBasedOnHeadersTests()
        {
            _serviceHandler = new ServiceHandler();
            _steps = new Steps();
        }

        [Fact]
        public void should_match_one_header_value()
        {
            var port = RandomPortFinder.GetRandomPort();
            var headerName = "country_code";
            var headerValue = "PL";

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
                                },
                            },
                            UpstreamPathTemplate = "/",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            UpstreamHeaderTemplates = new Dictionary<string, string>()
                            {
                                [headerName] = headerValue,
                            },
                        },
                    },
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/", 200, "Hello from Laura"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .And(x => _steps.GivenIAddAHeader(headerName, headerValue))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from Laura"))
                .BDDfy();
        }

        [Fact]
        public void should_match_one_header_value_when_more_headers()
        {
            var port = RandomPortFinder.GetRandomPort();
            var headerName = "country_code";
            var headerValue = "PL";

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
                                },
                            },
                            UpstreamPathTemplate = "/",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            UpstreamHeaderTemplates = new Dictionary<string, string>()
                            {
                                [headerName] = headerValue,
                            },
                        },
                    },
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/", 200, "Hello from Laura"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .And(x => _steps.GivenIAddAHeader("other", "otherValue"))
                .And(x => _steps.GivenIAddAHeader(headerName, headerValue))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from Laura"))
                .BDDfy();
        }

        [Fact]
        public void should_match_two_header_values_when_more_headers()
        {
            var port = RandomPortFinder.GetRandomPort();
            var headerName1 = "country_code";
            var headerValue1 = "PL";
            var headerName2 = "region";
            var headerValue2 = "MAZ";

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
                                },
                            },
                            UpstreamPathTemplate = "/",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            UpstreamHeaderTemplates = new Dictionary<string, string>()
                            {
                                [headerName1] = headerValue1,
                                [headerName2] = headerValue2,
                            },
                        },
                    },
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/", 200, "Hello from Laura"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .And(x => _steps.GivenIAddAHeader(headerName1, headerValue1))
                .And(x => _steps.GivenIAddAHeader("other", "otherValue"))
                .And(x => _steps.GivenIAddAHeader(headerName2, headerValue2))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from Laura"))
                .BDDfy();
        }

        [Fact]
        public void should_not_match_one_header_value()
        {
            var port = RandomPortFinder.GetRandomPort();
            var headerName = "country_code";
            var headerValue = "PL";
            var anotherHeaderValue = "UK";

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
                                },
                            },
                            UpstreamPathTemplate = "/",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            UpstreamHeaderTemplates = new Dictionary<string, string>()
                            {
                                [headerName] = headerValue,
                            },
                        },
                    },
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/", 200, "Hello from Laura"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .And(x => _steps.GivenIAddAHeader(headerName, anotherHeaderValue))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.NotFound))
                .BDDfy();
        }

        [Fact]
        public void should_not_match_one_header_value_when_no_headers()
        {
            var port = RandomPortFinder.GetRandomPort();
            var headerName = "country_code";
            var headerValue = "PL";

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
                                },
                            },
                            UpstreamPathTemplate = "/",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            UpstreamHeaderTemplates = new Dictionary<string, string>()
                            {
                                [headerName] = headerValue,
                            },
                        },
                    },
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/", 200, "Hello from Laura"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.NotFound))
                .BDDfy();
        }

        [Fact]
        public void should_not_match_two_header_values_when_one_different()
        {
            var port = RandomPortFinder.GetRandomPort();
            var headerName1 = "country_code";
            var headerValue1 = "PL";
            var headerName2 = "region";
            var headerValue2 = "MAZ";

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
                                },
                            },
                            UpstreamPathTemplate = "/",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            UpstreamHeaderTemplates = new Dictionary<string, string>()
                            {
                                [headerName1] = headerValue1,
                                [headerName2] = headerValue2,
                            },
                        },
                    },
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/", 200, "Hello from Laura"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .And(x => _steps.GivenIAddAHeader(headerName1, headerValue1))
                .And(x => _steps.GivenIAddAHeader("other", "otherValue"))
                .And(x => _steps.GivenIAddAHeader(headerName2, "anothervalue"))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.NotFound))
                .BDDfy();
        }

        [Fact]
        public void should_not_match_two_header_values_when_one_not_existing()
        {
            var port = RandomPortFinder.GetRandomPort();
            var headerName1 = "country_code";
            var headerValue1 = "PL";
            var headerName2 = "region";
            var headerValue2 = "MAZ";

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
                                },
                            },
                            UpstreamPathTemplate = "/",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            UpstreamHeaderTemplates = new Dictionary<string, string>()
                            {
                                [headerName1] = headerValue1,
                                [headerName2] = headerValue2,
                            },
                        },
                    },
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/", 200, "Hello from Laura"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .And(x => _steps.GivenIAddAHeader(headerName1, headerValue1))
                .And(x => _steps.GivenIAddAHeader("other", "otherValue"))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.NotFound))
                .BDDfy();
        }

        [Fact]
        public void should_not_match_one_header_value_when_header_duplicated()
        {
            var port = RandomPortFinder.GetRandomPort();
            var headerName = "country_code";
            var headerValue = "PL";

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
                                },
                            },
                            UpstreamPathTemplate = "/",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            UpstreamHeaderTemplates = new Dictionary<string, string>()
                            {
                                [headerName] = headerValue,
                            },
                        },
                    },
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/", 200, "Hello from Laura"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .And(x => _steps.GivenIAddAHeader(headerName, headerValue))
                .And(x => _steps.GivenIAddAHeader(headerName, "othervalue"))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.NotFound))
                .BDDfy();
        }

        [Fact]
        public void should_aggregated_route_match_header_value()
        {
            var port1 = RandomPortFinder.GetRandomPort();
            var port2 = RandomPortFinder.GetRandomPort();
            var headerName = "country_code";
            var headerValue = "PL";

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                    {
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/a",
                            DownstreamScheme = "http",
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = port1,
                                },
                            },
                            UpstreamPathTemplate = "/a",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            Key = "Laura",
                        },
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/b",
                            DownstreamScheme = "http",
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = port2,
                                },
                            },
                            UpstreamPathTemplate = "/b",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            Key = "Tom",
                        },
                    },
                Aggregates = new List<FileAggregateRoute>()
                {
                    new FileAggregateRoute
                        {
                            UpstreamPathTemplate = "/",
                            UpstreamHost = "localhost",
                            RouteKeys = new List<string>
                            {
                                "Laura",
                                "Tom",
                            },
                            UpstreamHeaderTemplates = new Dictionary<string, string>()
                            {
                                [headerName] = headerValue,
                            },
                        },
                },
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port1}", "/a", 200, "Hello from Laura"))
                .And(x => GivenThereIsAServiceRunningOn($"http://localhost:{port2}", "/b", 200, "Hello from Tom"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .And(x => _steps.GivenIAddAHeader(headerName, headerValue))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .BDDfy();
        }

        [Fact]
        public void should_aggregated_route_not_match_header_value()
        {
            var port1 = RandomPortFinder.GetRandomPort();
            var port2 = RandomPortFinder.GetRandomPort();
            var headerName = "country_code";
            var headerValue = "PL";

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                    {
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/a",
                            DownstreamScheme = "http",
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = port1,
                                },
                            },
                            UpstreamPathTemplate = "/a",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            Key = "Laura",
                        },
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/b",
                            DownstreamScheme = "http",
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = port2,
                                },
                            },
                            UpstreamPathTemplate = "/b",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            Key = "Tom",
                        },
                    },
                Aggregates = new List<FileAggregateRoute>()
                {
                    new FileAggregateRoute
                        {
                            UpstreamPathTemplate = "/",
                            UpstreamHost = "localhost",
                            RouteKeys = new List<string>
                            {
                                "Laura",
                                "Tom",
                            },
                            UpstreamHeaderTemplates = new Dictionary<string, string>()
                            {
                                [headerName] = headerValue,
                            },
                        },
                },
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port1}", "/a", 200, "Hello from Laura"))
                .And(x => GivenThereIsAServiceRunningOn($"http://localhost:{port2}", "/b", 200, "Hello from Tom"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.NotFound))
                .BDDfy();
        }

        [Fact]
        public void should_match_header_placeholder()
        {
            var port = RandomPortFinder.GetRandomPort();
            var headerName = "Region";

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                    {
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/api.internal-{code}/products",
                            DownstreamScheme = "http",
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = port,
                                },
                            },
                            UpstreamPathTemplate = "/products",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            UpstreamHeaderTemplates = new Dictionary<string, string>()
                            {
                                [headerName] = "{header:code}",
                            },
                        },
                    },
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/api.internal-uk/products", 200, "Hello from UK"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .And(x => _steps.GivenIAddAHeader(headerName, "uk"))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/products"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from UK"))
                .BDDfy();
        }

        [Fact]
        public void should_match_header_placeholder_not_in_downstream_path()
        {
            var port = RandomPortFinder.GetRandomPort();
            var headerName = "ProductName";

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                    {
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/products-info",
                            DownstreamScheme = "http",
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = port,
                                },
                            },
                            UpstreamPathTemplate = "/products",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            UpstreamHeaderTemplates = new Dictionary<string, string>()
                            {
                                [headerName] = "product-{header:everything}",
                            },
                        },
                    },
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/products-info", 200, "Hello from products"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .And(x => _steps.GivenIAddAHeader(headerName, "product-Camera"))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/products"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from products"))
                .BDDfy();
        }

        [Fact]
        public void should_distinguish_route_for_different_roles()
        {
            var port = RandomPortFinder.GetRandomPort();
            var headerName = "Origin";

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                    {
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/products-admin",
                            DownstreamScheme = "http",
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = port,
                                },
                            },
                            UpstreamPathTemplate = "/products",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            UpstreamHeaderTemplates = new Dictionary<string, string>()
                            {
                                [headerName] = "admin.xxx.com",
                            },
                        },
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/products",
                            DownstreamScheme = "http",
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = port,
                                },
                            },
                            UpstreamPathTemplate = "/products",
                            UpstreamHttpMethod = new List<string> { "Get" },
                        },
                    },
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/products-admin", 200, "Hello from products admin"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .And(x => _steps.GivenIAddAHeader(headerName, "admin.xxx.com"))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/products"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from products admin"))
                .BDDfy();
        }

        [Fact]
        public void should_match_header_and_url_placeholders()
        {
            var port = RandomPortFinder.GetRandomPort();
            var headerName = "country_code";

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                    {
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/{country_code}/{version}/{aa}",
                            DownstreamScheme = "http",
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = port,
                                },
                            },
                            UpstreamPathTemplate = "/{aa}",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            UpstreamHeaderTemplates = new Dictionary<string, string>()
                            {
                                [headerName] = "start_{header:country_code}_version_{header:version}_end",
                            },
                        },
                    },
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/pl/v1/bb", 200, "Hello from Laura"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .And(x => _steps.GivenIAddAHeader(headerName, "start_pl_version_v1_end"))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/bb"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from Laura"))
                .BDDfy();
        }

        [Fact]
        public void should_match_header_with_braces()
        {
            var port = RandomPortFinder.GetRandomPort();
            var headerName = "country_code";

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                    {
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/aa",
                            DownstreamScheme = "http",
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = port,
                                },
                            },
                            UpstreamPathTemplate = "/",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            UpstreamHeaderTemplates = new Dictionary<string, string>()
                            {
                                [headerName] = "my_{header}",
                            },
                        },
                    },
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/aa", 200, "Hello from Laura"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .And(x => _steps.GivenIAddAHeader(headerName, "my_{header}"))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from Laura"))
                .BDDfy();
        }

        [Fact]
        public void should_match_two_headers_with_the_same_name()
        {
            var port = RandomPortFinder.GetRandomPort();
            var headerName = "country_code";
            var headerValue1 = "PL";
            var headerValue2 = "UK";

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
                                },
                            },
                            UpstreamPathTemplate = "/",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            UpstreamHeaderTemplates = new Dictionary<string, string>()
                            {
                                [headerName] = headerValue1 + ";{header:whatever}",
                            },
                        },
                    },
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/", 200, "Hello from Laura"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .And(x => _steps.GivenIAddAHeader(headerName, headerValue1))
                .And(x => _steps.GivenIAddAHeader(headerName, headerValue2))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from Laura"))
                .BDDfy();
        }

        private void GivenThereIsAServiceRunningOn(string baseUrl, string basePath, int statusCode, string responseBody)
        {
            _serviceHandler.GivenThereIsAServiceRunningOn(baseUrl, basePath, async context =>
            {
                _downstreamPath = !string.IsNullOrEmpty(context.Request.PathBase.Value) ? context.Request.PathBase.Value : context.Request.Path.Value;

                if (_downstreamPath != basePath)
                {
                    context.Response.StatusCode = statusCode;
                    await context.Response.WriteAsync("downstream path didnt match base path");
                }
                else
                {
                    context.Response.StatusCode = statusCode;
                    await context.Response.WriteAsync(responseBody);
                }
            });
        }

        internal void ThenTheDownstreamUrlPathShouldBe(string expectedDownstreamPath)
        {
            _downstreamPath.ShouldBe(expectedDownstreamPath);
        }

        public void Dispose()
        {
            _serviceHandler.Dispose();
            _steps.Dispose();
        }
    }
}

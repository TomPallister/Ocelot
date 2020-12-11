namespace Ocelot.AcceptanceTests
{
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using Ocelot.Configuration.File;
    using Ocelot.Middleware;
    using Ocelot.Multiplexer;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using TestStack.BDDfy;
    using Xunit;

    public class AggregateTests : IDisposable
    {
        private readonly Steps _steps;
        private string _downstreamPathOne;
        private string _downstreamPathTwo;
        private readonly ServiceHandler _serviceHandler;

        public AggregateTests()
        {
            _serviceHandler = new ServiceHandler();
            _steps = new Steps();
        }

        [Fact]
        public void should_fix_issue_597()
        {
            var port = RandomPortFinder.GetRandomPort();
            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                {
                    new FileRoute
                    {
                        DownstreamPathTemplate = "/api/values?MailId={userid}",
                        UpstreamPathTemplate = "/key1data/{userid}",
                        UpstreamHttpMethod = new List<string> {"Get"},
                        DownstreamScheme = "http",
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "localhost",
                                Port = port,
                            },
                        },
                        Key = "key1",
                    },
                    new FileRoute
                    {
                        DownstreamPathTemplate = "/api/values?MailId={userid}",
                        UpstreamPathTemplate = "/key2data/{userid}",
                        UpstreamHttpMethod = new List<string> {"Get"},
                        DownstreamScheme = "http",
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "localhost",
                                Port = port,
                            },
                        },
                        Key = "key2",
                    },
                    new FileRoute
                    {
                        DownstreamPathTemplate = "/api/values?MailId={userid}",
                        UpstreamPathTemplate = "/key3data/{userid}",
                        UpstreamHttpMethod = new List<string> {"Get"},
                        DownstreamScheme = "http",
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "localhost",
                                Port = port,
                            },
                        },
                        Key = "key3",
                    },
                    new FileRoute
                    {
                        DownstreamPathTemplate = "/api/values?MailId={userid}",
                        UpstreamPathTemplate = "/key4data/{userid}",
                        UpstreamHttpMethod = new List<string> {"Get"},
                        DownstreamScheme = "http",
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "localhost",
                                Port = port,
                            },
                        },
                        Key = "key4",
                    },
                },
                Aggregates = new List<FileAggregateRoute>
                {
                    new FileAggregateRoute
                    {
                        RouteKeys = new List<string>{
                            "key1",
                            "key2",
                            "key3",
                            "key4",
                        },
                        UpstreamPathTemplate = "/EmpDetail/IN/{userid}",
                    },
                    new FileAggregateRoute
                    {
                        RouteKeys = new List<string>{
                            "key1",
                            "key2",
                        },
                        UpstreamPathTemplate = "/EmpDetail/US/{userid}",
                    },
                },
                GlobalConfiguration = new FileGlobalConfiguration
                {
                    RequestIdKey = "CorrelationID",
                },
            };

            var expected = "{\"key1\":some_data,\"key2\":some_data}";

            this.Given(x => x.GivenServiceIsRunning($"http://localhost:{port}", 200, "some_data"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/EmpDetail/US/1"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe(expected))
                .BDDfy();
        }

        [Fact]
        public void should_allow_get_method()
        {
            FileAggregateRoute route = JsonConvert.DeserializeObject<FileAggregateRoute>("{\"UpstreamHttpMethod\":[\"Get\"]}");
            Assert.Contains("Get", route.UpstreamHttpMethod);
        }

        [Fact]
        public void should_allow_post_method()
        {
            FileAggregateRoute route = JsonConvert.DeserializeObject<FileAggregateRoute>("{\"UpstreamHttpMethod\":[\"Post\"]}");
            Assert.Contains("Post", route.UpstreamHttpMethod);
        }

        [Fact]
        public void should_return_response_200_with_advanced_aggregate_configs()
        {
            var port1 = RandomPortFinder.GetRandomPort();
            var port2 = RandomPortFinder.GetRandomPort();
            var port3 = RandomPortFinder.GetRandomPort();
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
                                    Port = port1,
                                },
                            },
                            UpstreamPathTemplate = "/Comments",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            Key = "Comments",
                        },
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/users/{userId}",
                            DownstreamScheme = "http",
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = port2,
                                },
                            },
                            UpstreamPathTemplate = "/UserDetails",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            Key = "UserDetails",
                        },
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/posts/{postId}",
                            DownstreamScheme = "http",
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = port3,
                                },
                            },
                            UpstreamPathTemplate = "/PostDetails",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            Key = "PostDetails",
                        },
                    },
                Aggregates = new List<FileAggregateRoute>
                    {
                        new FileAggregateRoute
                        {
                            UpstreamPathTemplate = "/",
                            UpstreamHost = "localhost",
                            RouteKeys = new List<string>
                            {
                                "Comments",
                                "UserDetails",
                                "PostDetails",
                            },
                            RouteKeysConfig = new List<AggregateRouteConfig>()
                            {
                                new AggregateRouteConfig(){RouteKey = "UserDetails",JsonPath = "$[*].writerId",Parameter = "userId"},
                                new AggregateRouteConfig(){RouteKey = "PostDetails",JsonPath = "$[*].postId",Parameter = "postId"},
                            },
                        },
                    },
            };

            var userDetailsResponseContent = @"{""id"":1,""firstName"":""abolfazl"",""lastName"":""rajabpour""}";
            var postDetailsResponseContent = @"{""id"":1,""title"":""post1""}";
            var commentsResponseContent = @"[{""id"":1,""writerId"":1,""postId"":2,""text"":""text1""},{""id"":2,""writerId"":1,""postId"":2,""text"":""text2""}]";

            var expected = "{\"Comments\":" + commentsResponseContent + ",\"UserDetails\":" + userDetailsResponseContent + ",\"PostDetails\":" + postDetailsResponseContent + "}";

            this.Given(x => x.GivenServiceOneIsRunning($"http://localhost:{port1}", "/", 200, commentsResponseContent))
                .Given(x => x.GivenServiceTwoIsRunning($"http://localhost:{port2}", "/users/1", 200, userDetailsResponseContent))
                .Given(x => x.GivenServiceTwoIsRunning($"http://localhost:{port3}", "/posts/2", 200, postDetailsResponseContent))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe(expected))
                .BDDfy();
        }

        [Fact]
        public void should_return_response_200_with_simple_url_user_defined_aggregate()
        {
            var port1 = RandomPortFinder.GetRandomPort();
            var port2 = RandomPortFinder.GetRandomPort();
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
                                    Port = port1,
                                },
                            },
                            UpstreamPathTemplate = "/laura",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            Key = "Laura",
                        },
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/",
                            DownstreamScheme = "http",
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = port2,
                                },
                            },
                            UpstreamPathTemplate = "/tom",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            Key = "Tom",
                        },
                    },
                Aggregates = new List<FileAggregateRoute>
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
                            Aggregator = "FakeDefinedAggregator",
                        },
                    },
            };

            var expected = "Bye from Laura, Bye from Tom";

            this.Given(x => x.GivenServiceOneIsRunning($"http://localhost:{port1}", "/", 200, "{Hello from Laura}"))
                .Given(x => x.GivenServiceTwoIsRunning($"http://localhost:{port2}", "/", 200, "{Hello from Tom}"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunningWithSpecficAggregatorsRegisteredInDi<FakeDefinedAggregator, FakeDepdendency>())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe(expected))
                .And(x => ThenTheDownstreamUrlPathShouldBe("/", "/"))
                .BDDfy();
        }

        [Fact]
        public void should_return_response_200_with_simple_url()
        {
            var port1 = RandomPortFinder.GetRandomPort();
            var port2 = RandomPortFinder.GetRandomPort();
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
                                    Port = port1,
                                },
                            },
                            UpstreamPathTemplate = "/laura",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            Key = "Laura",
                        },
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/",
                            DownstreamScheme = "http",
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = port2,
                                },
                            },
                            UpstreamPathTemplate = "/tom",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            Key = "Tom",
                        },
                    },
                Aggregates = new List<FileAggregateRoute>
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
                        },
                    },
            };

            var expected = "{\"Laura\":{Hello from Laura},\"Tom\":{Hello from Tom}}";

            this.Given(x => x.GivenServiceOneIsRunning($"http://localhost:{port1}", "/", 200, "{Hello from Laura}"))
                .Given(x => x.GivenServiceTwoIsRunning($"http://localhost:{port2}", "/", 200, "{Hello from Tom}"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe(expected))
                .And(x => ThenTheDownstreamUrlPathShouldBe("/", "/"))
                .BDDfy();
        }

        [Fact]
        public void should_return_response_200_with_simple_url_one_service_404()
        {
            var port1 = RandomPortFinder.GetRandomPort();
            var port2 = RandomPortFinder.GetRandomPort();
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
                                    Port = port1,
                                },
                            },
                            UpstreamPathTemplate = "/laura",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            Key = "Laura",
                        },
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/",
                            DownstreamScheme = "http",
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = port2,
                                },
                            },
                            UpstreamPathTemplate = "/tom",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            Key = "Tom",
                        },
                    },
                Aggregates = new List<FileAggregateRoute>
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
                        },
                    },
            };

            var expected = "{\"Laura\":,\"Tom\":{Hello from Tom}}";

            this.Given(x => x.GivenServiceOneIsRunning($"http://localhost:{port1}", "/", 404, ""))
                .Given(x => x.GivenServiceTwoIsRunning($"http://localhost:{port2}", "/", 200, "{Hello from Tom}"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe(expected))
                .And(x => ThenTheDownstreamUrlPathShouldBe("/", "/"))
                .BDDfy();
        }

        [Fact]
        public void should_return_response_200_with_simple_url_both_service_404()
        {
            var port1 = RandomPortFinder.GetRandomPort();
            var port2 = RandomPortFinder.GetRandomPort();
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
                                    Port = port1,
                                },
                            },
                            UpstreamPathTemplate = "/laura",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            Key = "Laura",
                        },
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/",
                            DownstreamScheme = "http",
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = port2,
                                },
                            },
                            UpstreamPathTemplate = "/tom",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            Key = "Tom",
                        },
                    },
                Aggregates = new List<FileAggregateRoute>
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
                        },
                    },
            };

            var expected = "{\"Laura\":,\"Tom\":}";

            this.Given(x => x.GivenServiceOneIsRunning($"http://localhost:{port1}", "/", 404, ""))
                .Given(x => x.GivenServiceTwoIsRunning($"http://localhost:{port2}", "/", 404, ""))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe(expected))
                .And(x => ThenTheDownstreamUrlPathShouldBe("/", "/"))
                .BDDfy();
        }

        [Fact]
        public void should_be_thread_safe()
        {
            var port1 = RandomPortFinder.GetRandomPort();
            var port2 = RandomPortFinder.GetRandomPort();
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
                                    Port = port1,
                                },
                            },
                            UpstreamPathTemplate = "/laura",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            Key = "Laura",
                        },
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/",
                            DownstreamScheme = "http",
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = port2,
                                },
                            },
                            UpstreamPathTemplate = "/tom",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            Key = "Tom",
                        },
                    },
                Aggregates = new List<FileAggregateRoute>
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
                        },
                    },
            };

            this.Given(x => x.GivenServiceOneIsRunning($"http://localhost:{port1}", "/", 200, "{Hello from Laura}"))
                .Given(x => x.GivenServiceTwoIsRunning($"http://localhost:{port2}", "/", 200, "{Hello from Tom}"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .When(x => _steps.WhenIMakeLotsOfDifferentRequestsToTheApiGateway())
                .And(x => ThenTheDownstreamUrlPathShouldBe("/", "/"))
                .BDDfy();
        }

        private void GivenServiceIsRunning(string baseUrl, int statusCode, string responseBody)
        {
            _serviceHandler.GivenThereIsAServiceRunningOn(baseUrl, async context =>
            {
                context.Response.StatusCode = statusCode;
                await context.Response.WriteAsync(responseBody);
            });
        }

        private void GivenServiceOneIsRunning(string baseUrl, string basePath, int statusCode, string responseBody)
        {
            _serviceHandler.GivenThereIsAServiceRunningOn(baseUrl, basePath, async context =>
            {
                _downstreamPathOne = !string.IsNullOrEmpty(context.Request.PathBase.Value) ? context.Request.PathBase.Value : context.Request.Path.Value;

                if (_downstreamPathOne != basePath)
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

        private void GivenServiceTwoIsRunning(string baseUrl, string basePath, int statusCode, string responseBody)
        {
            _serviceHandler.GivenThereIsAServiceRunningOn(baseUrl, basePath, async context =>
            {
                _downstreamPathTwo = !string.IsNullOrEmpty(context.Request.PathBase.Value) ? context.Request.PathBase.Value : context.Request.Path.Value;

                if (_downstreamPathTwo != basePath)
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

        internal void ThenTheDownstreamUrlPathShouldBe(string expectedDownstreamPathOne, string expectedDownstreamPath)
        {
            _downstreamPathOne.ShouldBe(expectedDownstreamPathOne);
            _downstreamPathTwo.ShouldBe(expectedDownstreamPath);
        }

        public void Dispose()
        {
            _serviceHandler.Dispose();
            _steps.Dispose();
        }
    }

    public class FakeDepdendency
    {
    }

    public class FakeDefinedAggregator : IDefinedAggregator
    {
        private readonly FakeDepdendency _dep;

        public FakeDefinedAggregator(FakeDepdendency dep)
        {
            _dep = dep;
        }

        public async Task<DownstreamResponse> Aggregate(List<HttpContext> responses)
        {
            var one = await responses[0].Items.DownstreamResponse().Content.ReadAsStringAsync();
            var two = await responses[1].Items.DownstreamResponse().Content.ReadAsStringAsync();

            var merge = $"{one}, {two}";
            merge = merge.Replace("Hello", "Bye").Replace("{", "").Replace("}", "");
            var headers = responses.SelectMany(x => x.Items.DownstreamResponse().Headers).ToList();
            return new DownstreamResponse(new StringContent(merge), HttpStatusCode.OK, headers, "some reason");
        }
    }
}

using Moq;
using Ocelot.Configuration;
using Ocelot.Configuration.Creator;
using Ocelot.Configuration.File;
using Ocelot.Infrastructure;
using Ocelot.Logging;
using Ocelot.Responses;
using Ocelot.UnitTests.Responder;
using Shouldly;
using System.Collections.Generic;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.Configuration
{
    public class HeaderFindAndReplaceCreatorTests
    {
        private HeaderFindAndReplaceCreator _creator;
        private FileRoute _route;
        private HeaderTransformations _result;
        private Mock<IPlaceholders> _placeholders;
        private Mock<IOcelotLoggerFactory> _factory;
        private Mock<IOcelotLogger> _logger;

        public HeaderFindAndReplaceCreatorTests()
        {
            _logger = new Mock<IOcelotLogger>();
            _factory = new Mock<IOcelotLoggerFactory>();
            _factory.Setup(x => x.CreateLogger<HeaderFindAndReplaceCreator>()).Returns(_logger.Object);
            _placeholders = new Mock<IPlaceholders>();
            _creator = new HeaderFindAndReplaceCreator(_placeholders.Object, _factory.Object);
        }

        [Fact]
        public void should_create()
        {
            var route = new FileRoute
            {
                UpstreamHeaderTransform = new Dictionary<string, string>
                {
                    {"Test", "Test, Chicken"},
                    {"Moop", "o, a"}
                },
                DownstreamHeaderTransform = new Dictionary<string, string>
                {
                    {"Pop", "West, East"},
                    {"Bop", "e, r"}
                }
            };

            var upstream = new List<HeaderFindAndReplace>
            {
                new HeaderFindAndReplace("Test", "Test", "Chicken", 0),
                new HeaderFindAndReplace("Moop", "o", "a", 0)
            };

            var downstream = new List<HeaderFindAndReplace>
            {
                new HeaderFindAndReplace("Pop", "West", "East", 0),
                new HeaderFindAndReplace("Bop", "e", "r", 0)
            };

            this.Given(x => GivenTheRoute(route))
                .When(x => WhenICreate())
                .Then(x => ThenTheFollowingUpstreamIsReturned(upstream))
                .Then(x => ThenTheFollowingDownstreamIsReturned(downstream))
                .BDDfy();
        }

        [Fact]
        public void should_create_with_add_headers_to_request()
        {
            const string key = "X-Forwarded-For";
            const string value = "{RemoteIpAddress}";

            var route = new FileRoute
            {
                UpstreamHeaderTransform = new Dictionary<string, string>
                {
                    {key, value},
                }
            };

            var expected = new AddHeader(key, value);

            this.Given(x => GivenTheRoute(route))
                .When(x => WhenICreate())
                .Then(x => ThenTheFollowingAddHeaderToUpstreamIsReturned(expected))
                .BDDfy();
        }

        [Fact]
        public void should_use_base_url_placeholder()
        {
            var route = new FileRoute
            {
                DownstreamHeaderTransform = new Dictionary<string, string>
                {
                    {"Location", "http://www.bbc.co.uk/, {BaseUrl}"},
                }
            };

            var downstream = new List<HeaderFindAndReplace>
            {
                new HeaderFindAndReplace("Location", "http://www.bbc.co.uk/", "http://ocelot.com/", 0),
            };

            this.Given(x => GivenTheRoute(route))
                .And(x => GivenTheBaseUrlIs("http://ocelot.com/"))
                .When(x => WhenICreate())
                .Then(x => ThenTheFollowingDownstreamIsReturned(downstream))
                .BDDfy();
        }

        [Fact]
        public void should_log_errors_and_not_add_headers()
        {
            var route = new FileRoute
            {
                DownstreamHeaderTransform = new Dictionary<string, string>
                {
                    {"Location", "http://www.bbc.co.uk/, {BaseUrl}"},
                },
                UpstreamHeaderTransform = new Dictionary<string, string>
                {
                    {"Location", "http://www.bbc.co.uk/, {BaseUrl}"},
                }
            };

            var expected = new List<HeaderFindAndReplace>
            {
            };

            this.Given(x => GivenTheRoute(route))
                .And(x => GivenTheBaseUrlErrors())
                .When(x => WhenICreate())
                .Then(x => ThenTheFollowingDownstreamIsReturned(expected))
                .And(x => ThenTheFollowingUpstreamIsReturned(expected))
                .And(x => ThenTheLoggerIsCalledCorrectly("Unable to add DownstreamHeaderTransform Location: http://www.bbc.co.uk/, {BaseUrl}"))
                .And(x => ThenTheLoggerIsCalledCorrectly("Unable to add UpstreamHeaderTransform Location: http://www.bbc.co.uk/, {BaseUrl}"))
                .BDDfy();
        }

        private void ThenTheLoggerIsCalledCorrectly(string message)
        {
            _logger.Verify(x => x.LogWarning(message), Times.Once);
        }

        [Fact]
        public void should_use_base_url_partial_placeholder()
        {
            var route = new FileRoute
            {
                DownstreamHeaderTransform = new Dictionary<string, string>
                {
                    {"Location", "http://www.bbc.co.uk/pay, {BaseUrl}pay"},
                }
            };

            var downstream = new List<HeaderFindAndReplace>
            {
                new HeaderFindAndReplace("Location", "http://www.bbc.co.uk/pay", "http://ocelot.com/pay", 0),
            };

            this.Given(x => GivenTheRoute(route))
                .And(x => GivenTheBaseUrlIs("http://ocelot.com/"))
                .When(x => WhenICreate())
                .Then(x => ThenTheFollowingDownstreamIsReturned(downstream))
                .BDDfy();
        }

        [Fact]
        public void should_add_trace_id_header()
        {
            var route = new FileRoute
            {
                DownstreamHeaderTransform = new Dictionary<string, string>
                {
                    {"Trace-Id", "{TraceId}"},
                }
            };

            var expected = new AddHeader("Trace-Id", "{TraceId}");

            this.Given(x => GivenTheRoute(route))
                .And(x => GivenTheBaseUrlIs("http://ocelot.com/"))
                .When(x => WhenICreate())
                .Then(x => ThenTheFollowingAddHeaderToDownstreamIsReturned(expected))
                .BDDfy();
        }

        [Fact]
        public void should_add_downstream_header_as_is_when_no_replacement_is_given()
        {
            var route = new FileRoute
            {
                DownstreamHeaderTransform = new Dictionary<string, string>
                {
                    {"X-Custom-Header", "Value"},
                }
            };

            var expected = new AddHeader("X-Custom-Header", "Value");

            this.Given(x => GivenTheRoute(route))
                .And(x => WhenICreate())
                .Then(x => x.ThenTheFollowingAddHeaderToDownstreamIsReturned(expected))
                .BDDfy();
        }

        [Fact]
        public void should_add_upstream_header_as_is_when_no_replacement_is_given()
        {
            var route = new FileRoute
            {
                UpstreamHeaderTransform = new Dictionary<string, string>
                {
                    {"X-Custom-Header", "Value"},
                }
            };

            var expected = new AddHeader("X-Custom-Header", "Value");

            this.Given(x => GivenTheRoute(route))
                .And(x => WhenICreate())
                .Then(x => x.ThenTheFollowingAddHeaderToUpstreamIsReturned(expected))
                .BDDfy();
        }

        private void GivenTheBaseUrlIs(string baseUrl)
        {
            _placeholders.Setup(x => x.Get(It.IsAny<string>())).Returns(new OkResponse<string>(baseUrl));
        }

        private void GivenTheBaseUrlErrors()
        {
            _placeholders.Setup(x => x.Get(It.IsAny<string>())).Returns(new ErrorResponse<string>(new AnyError()));
        }

        private void ThenTheFollowingAddHeaderToDownstreamIsReturned(AddHeader addHeader)
        {
            _result.AddHeadersToDownstream[0].Key.ShouldBe(addHeader.Key);
            _result.AddHeadersToDownstream[0].Value.ShouldBe(addHeader.Value);
        }

        private void ThenTheFollowingAddHeaderToUpstreamIsReturned(AddHeader addHeader)
        {
            _result.AddHeadersToUpstream[0].Key.ShouldBe(addHeader.Key);
            _result.AddHeadersToUpstream[0].Value.ShouldBe(addHeader.Value);
        }

        private void ThenTheFollowingDownstreamIsReturned(List<HeaderFindAndReplace> downstream)
        {
            _result.Downstream.Count.ShouldBe(downstream.Count);

            for (int i = 0; i < _result.Downstream.Count; i++)
            {
                var result = _result.Downstream[i];
                var expected = downstream[i];
                result.Find.ShouldBe(expected.Find);
                result.Index.ShouldBe(expected.Index);
                result.Key.ShouldBe(expected.Key);
                result.Replace.ShouldBe(expected.Replace);
            }
        }

        private void GivenTheRoute(FileRoute route)
        {
            _route = route;
        }

        private void WhenICreate()
        {
            _result = _creator.Create(_route);
        }

        private void ThenTheFollowingUpstreamIsReturned(List<HeaderFindAndReplace> expecteds)
        {
            _result.Upstream.Count.ShouldBe(expecteds.Count);

            for (int i = 0; i < _result.Upstream.Count; i++)
            {
                var result = _result.Upstream[i];
                var expected = expecteds[i];
                result.Find.ShouldBe(expected.Find);
                result.Index.ShouldBe(expected.Index);
                result.Key.ShouldBe(expected.Key);
                result.Replace.ShouldBe(expected.Replace);
            }
        }
    }
}

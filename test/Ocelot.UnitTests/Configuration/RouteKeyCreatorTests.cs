namespace Ocelot.UnitTests.Configuration
{
    using Ocelot.Configuration.Creator;
    using Ocelot.Configuration.File;
    using Ocelot.LoadBalancer.LoadBalancers;
    using Shouldly;
    using System.Collections.Generic;
    using System.Linq;
    using TestStack.BDDfy;
    using Xunit;

    public class RouteKeyCreatorTests
    {
        private RouteKeyCreator _creator;
        private FileRoute _route;
        private string _result;

        public RouteKeyCreatorTests()
        {
            _creator = new RouteKeyCreator();
        }

        [Fact]
        public void should_return_sticky_session_key()
        {
            var route = new FileRoute
            {
                LoadBalancerOptions = new FileLoadBalancerOptions
                {
                    Key = "testy",
                    Type = nameof(CookieStickySessions)
                }
            };

            this.Given(_ => GivenThe(route))
                .When(_ => WhenICreate())
                .Then(_ => ThenTheResultIs($"{nameof(CookieStickySessions)}:{route.LoadBalancerOptions.Key}"))
                .BDDfy();
        }

        [Fact]
        public void should_return_re_route_key()
        {
            var route = new FileRoute
            {
                UpstreamPathTemplate = "/api/product",
                UpstreamHttpMethod = new List<string> { "GET", "POST", "PUT" },
                DownstreamHostAndPorts = new List<FileHostAndPort>
                {
                    new FileHostAndPort
                    {
                        Host = "localhost",
                        Port = 123
                    },
                    new FileHostAndPort
                    {
                        Host = "localhost",
                        Port = 123
                    }
                }
            };

            this.Given(_ => GivenThe(route))
                .When(_ => WhenICreate())
                .Then(_ => ThenTheResultIs($"{route.UpstreamPathTemplate}|{string.Join(",", route.UpstreamHttpMethod)}|{string.Join(",", route.DownstreamHostAndPorts.Select(x => $"{x.Host}:{x.Port}"))}"))
                .BDDfy();
        }

        private void GivenThe(FileRoute route)
        {
            _route = route;
        }

        private void WhenICreate()
        {
            _result = _creator.Create(_route);
        }

        private void ThenTheResultIs(string expected)
        {
            _result.ShouldBe(expected);
        }
    }
}

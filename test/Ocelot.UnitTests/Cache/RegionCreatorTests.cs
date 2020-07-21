using Ocelot.Cache;
using Ocelot.Configuration.File;
using Shouldly;
using System.Collections.Generic;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.Cache
{
    public class RegionCreatorTests
    {
        private string _result;
        private FileRoute _route;

        [Fact]
        public void should_create_region()
        {
            var route = new FileRoute
            {
                UpstreamHttpMethod = new List<string> { "Get" },
                UpstreamPathTemplate = "/testdummy"
            };

            this.Given(_ => GivenTheRoute(route))
                .When(_ => WhenICreateTheRegion())
                .Then(_ => ThenTheRegionIs("Gettestdummy"))
                .BDDfy();
        }

        [Fact]
        public void should_use_region()
        {
            var route = new FileRoute
            {
                FileCacheOptions = new FileCacheOptions
                {
                    Region = "region"
                }
            };

            this.Given(_ => GivenTheRoute(route))
                .When(_ => WhenICreateTheRegion())
                .Then(_ => ThenTheRegionIs("region"))
                .BDDfy();
        }

        private void GivenTheRoute(FileRoute route)
        {
            _route = route;
        }

        private void WhenICreateTheRegion()
        {
            RegionCreator regionCreator = new RegionCreator();
            _result = regionCreator.Create(_route);
        }

        private void ThenTheRegionIs(string expected)
        {
            _result.ShouldBe(expected);
        }
    }
}

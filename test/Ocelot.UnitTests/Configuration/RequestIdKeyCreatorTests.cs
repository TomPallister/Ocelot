using Ocelot.Configuration.Creator;
using Ocelot.Configuration.File;
using Shouldly;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.Configuration
{
    public class RequestIdKeyCreatorTests
    {
        private FileRoute _fileRoute;
        private FileGlobalConfiguration _fileGlobalConfig;
        private string _result;
        private RequestIdKeyCreator _creator;

        public RequestIdKeyCreatorTests()
        {
            _creator = new RequestIdKeyCreator();
        }

        [Fact]
        public void should_use_global_configuration()
        {
            var route = new FileRoute();
            var globalConfig = new FileGlobalConfiguration
            {
                RequestIdKey = "cheese"
            };

            this.Given(x => x.GivenTheFollowingRoute(route))
                .And(x => x.GivenTheFollowingGlobalConfig(globalConfig))
                .When(x => x.WhenICreate())
                .Then(x => x.ThenTheFollowingIsReturned("cheese"))
                .BDDfy();
        }

        [Fact]
        public void should_use_re_route_specific()
        {
            var route = new FileRoute
            {
                RequestIdKey = "cheese"
            };
            var globalConfig = new FileGlobalConfiguration();

            this.Given(x => x.GivenTheFollowingRoute(route))
                .And(x => x.GivenTheFollowingGlobalConfig(globalConfig))
                .When(x => x.WhenICreate())
                .Then(x => x.ThenTheFollowingIsReturned("cheese"))
                .BDDfy();
        }

        [Fact]
        public void should_use_re_route_over_global_specific()
        {
            var route = new FileRoute
            {
                RequestIdKey = "cheese"
            };
            var globalConfig = new FileGlobalConfiguration
            {
                RequestIdKey = "test"
            };

            this.Given(x => x.GivenTheFollowingRoute(route))
                .And(x => x.GivenTheFollowingGlobalConfig(globalConfig))
                .When(x => x.WhenICreate())
                .Then(x => x.ThenTheFollowingIsReturned("cheese"))
                .BDDfy();
        }

        private void GivenTheFollowingRoute(FileRoute fileRoute)
        {
            _fileRoute = fileRoute;
        }

        private void GivenTheFollowingGlobalConfig(FileGlobalConfiguration globalConfig)
        {
            _fileGlobalConfig = globalConfig;
        }

        private void WhenICreate()
        {
            _result = _creator.Create(_fileRoute, _fileGlobalConfig);
        }

        private void ThenTheFollowingIsReturned(string expected)
        {
            _result.ShouldBe(expected);
        }
    }
}

using Ocelot.Configuration.Creator;
using Ocelot.Configuration.File;
using Ocelot.Values;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Text;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.Configuration
{
    public class UpstreamHeaderTemplatePatternCreatorTests
    {
        private FileRoute _fileRoute;
        private readonly UpstreamHeaderTemplatePatternCreator _creator;
        private Dictionary<string, UpstreamHeaderTemplate> _result;

        public UpstreamHeaderTemplatePatternCreatorTests()
        {
            _creator = new UpstreamHeaderTemplatePatternCreator();
        }

        [Fact]
        public void should_match_two_placeholders()
        {
            var fileRoute = new FileRoute
            {
                UpstreamHeaderTemplates = new Dictionary<string, string>
                {
                    ["country"] = "any text {cc} and other {version} and {bob} the end",                    
                },
            };

            this.Given(x => x.GivenTheFollowingFileRoute(fileRoute))
                .When(x => x.WhenICreateTheTemplatePattern())
                .Then(x => x.ThenTheFollowingIsReturned("country", "^(?i)any text {.+} and other {.+} and {.+} the end$"))
                .BDDfy();
        }

        private void GivenTheFollowingFileRoute(FileRoute fileRoute)
        {
            _fileRoute = fileRoute;
        }

        private void WhenICreateTheTemplatePattern()
        {
            _result = _creator.Create(_fileRoute);
        }

        private void ThenTheFollowingIsReturned(string headerKey, string expected)
        {
            _result[headerKey].Template.ShouldBe(expected);
        }
    }
}

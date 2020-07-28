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
        public void should_create_pattern_without_placeholders()
        {
            var fileRoute = new FileRoute
            {
                UpstreamHeaderTemplates = new Dictionary<string, string>
                {
                    ["country"] = "a text without placeholders",
                },
            };

            this.Given(x => x.GivenTheFollowingFileRoute(fileRoute))
                .When(x => x.WhenICreateTheTemplatePattern())
                .Then(x => x.ThenTheFollowingIsReturned("country", "^(?i)a text without placeholders$"))
                .BDDfy();
        }

        [Fact]
        public void should_create_pattern_case_sensitive()
        {
            var fileRoute = new FileRoute
            {
                RouteIsCaseSensitive = true,
                UpstreamHeaderTemplates = new Dictionary<string, string>
                {
                    ["country"] = "a text without placeholders",
                },
            };

            this.Given(x => x.GivenTheFollowingFileRoute(fileRoute))
                .When(x => x.WhenICreateTheTemplatePattern())
                .Then(x => x.ThenTheFollowingIsReturned("country", "^a text without placeholders$"))
                .BDDfy();
        }

        [Fact]
        public void should_create_pattern_with_placeholder_in_the_beginning()
        {
            var fileRoute = new FileRoute
            {
                UpstreamHeaderTemplates = new Dictionary<string, string>
                {
                    ["country"] = "{header:start}rest of the text",
                },
            };

            this.Given(x => x.GivenTheFollowingFileRoute(fileRoute))
                .When(x => x.WhenICreateTheTemplatePattern())
                .Then(x => x.ThenTheFollowingIsReturned("country", "^(?i)(?<start>.+)rest of the text$"))
                .BDDfy();
        }

        [Fact]
        public void should_create_pattern_with_placeholder_at_the_end()
        {
            var fileRoute = new FileRoute
            {
                UpstreamHeaderTemplates = new Dictionary<string, string>
                {
                    ["country"] = "rest of the text{header:end}",
                },
            };

            this.Given(x => x.GivenTheFollowingFileRoute(fileRoute))
                .When(x => x.WhenICreateTheTemplatePattern())
                .Then(x => x.ThenTheFollowingIsReturned("country", "^(?i)rest of the text(?<end>.+)$"))
                .BDDfy();
        }

        [Fact]
        public void should_create_pattern_with_placeholder_only()
        {
            var fileRoute = new FileRoute
            {
                UpstreamHeaderTemplates = new Dictionary<string, string>
                {
                    ["country"] = "{header:countrycode}",
                },
            };

            this.Given(x => x.GivenTheFollowingFileRoute(fileRoute))
                .When(x => x.WhenICreateTheTemplatePattern())
                .Then(x => x.ThenTheFollowingIsReturned("country", "^(?i)(?<countrycode>.+)$"))
                .BDDfy();
        }

        [Fact]
        public void should_crate_pattern_with_more_placeholders()
        {
            var fileRoute = new FileRoute
            {
                UpstreamHeaderTemplates = new Dictionary<string, string>
                {
                    ["country"] = "any text {header:cc} and other {header:version} and {header:bob} the end",                    
                },
            };

            this.Given(x => x.GivenTheFollowingFileRoute(fileRoute))
                .When(x => x.WhenICreateTheTemplatePattern())
                .Then(x => x.ThenTheFollowingIsReturned("country", "^(?i)any text (?<cc>.+) and other (?<version>.+) and (?<bob>.+) the end$"))
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

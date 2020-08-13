using Ocelot.DownstreamRouteFinder.HeaderMatcher;
using Ocelot.DownstreamRouteFinder.UrlMatcher;
using Ocelot.Values;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.DownstreamRouteFinder.HeaderMatcher
{
    public class HeaderPlaceholderNameAndValueFinderTests
    {
        private readonly IHeaderPlaceholderNameAndValueFinder _finder;
        private Dictionary<string, string> _upstreamHeaders;
        private Dictionary<string, UpstreamHeaderTemplate> _upstreamHeaderTemplates;
        private List<PlaceholderNameAndValue> _result;

        public HeaderPlaceholderNameAndValueFinderTests()
        {
            _finder = new HeaderPlaceholderNameAndValueFinder();
        }

        [Fact]
        public void should_return_no_placeholders()
        {
            var upstreamHeaderTemplates = new Dictionary<string, UpstreamHeaderTemplate>();
            var upstreamHeaders = new Dictionary<string, string>();
            var expected = new List<PlaceholderNameAndValue>();

            this.Given(x => x.GivenUpstreamHeaderTemplatesAre(upstreamHeaderTemplates))
                .And(x => x.GivenUpstreamHeadersAre(upstreamHeaders))
                .When(x => x.WhenICallFindPlaceholders())
                .Then(x => x.TheResultIs(expected))
                .BDDfy();
        }

        [Fact]
        public void should_return_one_placeholder_with_value_when_no_other_text()
        {
            var upstreamHeaderTemplates = new Dictionary<string, UpstreamHeaderTemplate>
            {
                ["country"] = new UpstreamHeaderTemplate("^(?i)(?<countrycode>.+)$", "{header:countrycode}"),
            };
            var upstreamHeaders = new Dictionary<string, string>
            {
                ["country"] = "PL",
            };
            var expected = new List<PlaceholderNameAndValue>
            {
                new PlaceholderNameAndValue("{countrycode}", "PL"),
            };

            this.Given(x => x.GivenUpstreamHeaderTemplatesAre(upstreamHeaderTemplates))
                .And(x => x.GivenUpstreamHeadersAre(upstreamHeaders))
                .When(x => x.WhenICallFindPlaceholders())
                .Then(x => x.TheResultIs(expected))
                .BDDfy();
        }

        [Fact]
        public void should_return_one_placeholder_with_value_when_other_text_on_the_right()
        {
            var upstreamHeaderTemplates = new Dictionary<string, UpstreamHeaderTemplate>
            {
                ["country"] = new UpstreamHeaderTemplate("^(?<countrycode>.+)-V1$", "{header:countrycode}-V1"),
            };
            var upstreamHeaders = new Dictionary<string, string>
            {
                ["country"] = "PL-V1",
            };
            var expected = new List<PlaceholderNameAndValue>
            {
                new PlaceholderNameAndValue("{countrycode}", "PL"),
            };

            this.Given(x => x.GivenUpstreamHeaderTemplatesAre(upstreamHeaderTemplates))
                .And(x => x.GivenUpstreamHeadersAre(upstreamHeaders))
                .When(x => x.WhenICallFindPlaceholders())
                .Then(x => x.TheResultIs(expected))
                .BDDfy();
        }

        [Fact]
        public void should_return_one_placeholder_with_value_when_other_text_on_the_left()
        {
            var upstreamHeaderTemplates = new Dictionary<string, UpstreamHeaderTemplate>
            {
                ["country"] = new UpstreamHeaderTemplate("^V1-(?<countrycode>.+)$", "V1-{header:countrycode}"),
            };
            var upstreamHeaders = new Dictionary<string, string>
            {
                ["country"] = "V1-PL",
            };
            var expected = new List<PlaceholderNameAndValue>
            {
                new PlaceholderNameAndValue("{countrycode}", "PL"),
            };

            this.Given(x => x.GivenUpstreamHeaderTemplatesAre(upstreamHeaderTemplates))
                .And(x => x.GivenUpstreamHeadersAre(upstreamHeaders))
                .When(x => x.WhenICallFindPlaceholders())
                .Then(x => x.TheResultIs(expected))
                .BDDfy();
        }

        [Fact]
        public void should_return_one_placeholder_with_value_when_other_texts_surrounding()
        {
            var upstreamHeaderTemplates = new Dictionary<string, UpstreamHeaderTemplate>
            {
                ["country"] = new UpstreamHeaderTemplate("^cc:(?<countrycode>.+)-V1$", "cc:{header:countrycode}-V1"),
            };
            var upstreamHeaders = new Dictionary<string, string>
            {
                ["country"] = "cc:PL-V1",
            };
            var expected = new List<PlaceholderNameAndValue>
            {
                new PlaceholderNameAndValue("{countrycode}", "PL"),
            };

            this.Given(x => x.GivenUpstreamHeaderTemplatesAre(upstreamHeaderTemplates))
                .And(x => x.GivenUpstreamHeadersAre(upstreamHeaders))
                .When(x => x.WhenICallFindPlaceholders())
                .Then(x => x.TheResultIs(expected))
                .BDDfy();
        }

        [Fact]
        public void should_return_two_placeholders_with_text_between()
        {
            var upstreamHeaderTemplates = new Dictionary<string, UpstreamHeaderTemplate>
            {
                ["countryAndVersion"] = new UpstreamHeaderTemplate("^(?i)(?<countrycode>.+)-(?<version>.+)$", "{header:countrycode}-{header:version}"),
            };
            var upstreamHeaders = new Dictionary<string, string>
            {
                ["countryAndVersion"] = "PL-v1",
            };
            var expected = new List<PlaceholderNameAndValue>
            {
                new PlaceholderNameAndValue("{countrycode}", "PL"),
                new PlaceholderNameAndValue("{version}", "v1"),
            };

            this.Given(x => x.GivenUpstreamHeaderTemplatesAre(upstreamHeaderTemplates))
                .And(x => x.GivenUpstreamHeadersAre(upstreamHeaders))
                .When(x => x.WhenICallFindPlaceholders())
                .Then(x => x.TheResultIs(expected))
                .BDDfy();
        }

        [Fact]
        public void should_return_placeholders_from_different_headers()
        {
            var upstreamHeaderTemplates = new Dictionary<string, UpstreamHeaderTemplate>
            {
                ["country"] = new UpstreamHeaderTemplate("^(?i)(?<countrycode>.+)$", "{header:countrycode}"),
                ["version"] = new UpstreamHeaderTemplate("^(?i)(?<version>.+)$", "{header:version}"),
            };
            var upstreamHeaders = new Dictionary<string, string>
            {
                ["country"] = "PL",
                ["version"] = "v1",
            };
            var expected = new List<PlaceholderNameAndValue>
            {
                new PlaceholderNameAndValue("{countrycode}", "PL"),
                new PlaceholderNameAndValue("{version}", "v1"),
            };

            this.Given(x => x.GivenUpstreamHeaderTemplatesAre(upstreamHeaderTemplates))
                .And(x => x.GivenUpstreamHeadersAre(upstreamHeaders))
                .When(x => x.WhenICallFindPlaceholders())
                .Then(x => x.TheResultIs(expected))
                .BDDfy();
        }

        private void GivenUpstreamHeaderTemplatesAre(Dictionary<string, UpstreamHeaderTemplate> upstreaHeaderTemplates)
        {
            _upstreamHeaderTemplates = upstreaHeaderTemplates;
        }

        private void GivenUpstreamHeadersAre(Dictionary<string, string> upstreamHeaders)
        {
            _upstreamHeaders = upstreamHeaders;
        }

        private void WhenICallFindPlaceholders()
        {
            _result = _finder.Find(_upstreamHeaders, _upstreamHeaderTemplates);
        }

        private void TheResultIs(List<PlaceholderNameAndValue> expected)
        {
            _result.ShouldNotBeNull();
            _result.Count.ShouldBe(expected.Count);
            _result.ForEach(x => expected.Any(e => e.Name == x.Name && e.Value == x.Value).ShouldBeTrue());
        }
    }
}

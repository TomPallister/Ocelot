using Ocelot.DownstreamRouteFinder.HeaderMatcher;
using Ocelot.Values;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.DownstreamRouteFinder.HeaderMatcher
{
    public class HeadersToHeaderTemplatesMatcherTests
    {
        private readonly IHeadersToHeaderTemplatesMatcher _headerMatcher;
        private Dictionary<string, string> _upstreamHeaders;
        private Dictionary<string, UpstreamHeaderTemplate> _templateHeaders;
        private bool _result;

        public HeadersToHeaderTemplatesMatcherTests()
        {
            _headerMatcher = new HeadersToHeaderTemplatesMatcher();
        }

        [Fact]
        public void should_match_when_no_template_headers()
        {
            var upstreamHeaders = new Dictionary<string, string>()
            {
                ["anyHeader"] = "anyHeaderValue",
            };

            var templateHeaders = new Dictionary<string, UpstreamHeaderTemplate>();

            this.Given(x => x.GivenIHaveUpstreamHeaders(upstreamHeaders))
                .And(x => x.GivenIHaveTemplateHeadersInRoute(templateHeaders))
                .When(x => x.WhenIMatchTheHeaders())
                .Then(x => x.ThenTheResultIsTrue())
                .BDDfy();
        }

        [Fact]
        public void should_match_the_same_headers()
        {
            var upstreamHeaders = new Dictionary<string, string>()
            {
                ["anyHeader"] = "anyHeaderValue",
            };

            var templateHeaders = new Dictionary<string, UpstreamHeaderTemplate>()
            {
                ["anyHeader"] = new UpstreamHeaderTemplate("^(?i)anyHeaderValue$", "anyHeaderValue"),
            };

            this.Given(x => x.GivenIHaveUpstreamHeaders(upstreamHeaders))
                .And(x => x.GivenIHaveTemplateHeadersInRoute(templateHeaders))
                .When(x => x.WhenIMatchTheHeaders())
                .Then(x => x.ThenTheResultIsTrue())
                .BDDfy();
        }

        [Fact]
        public void should_not_match_the_same_headers_when_differ_case_and_case_sensitive()
        {
            var upstreamHeaders = new Dictionary<string, string>()
            {
                ["anyHeader"] = "ANYHEADERVALUE",
            };

            var templateHeaders = new Dictionary<string, UpstreamHeaderTemplate>()
            {
                ["anyHeader"] = new UpstreamHeaderTemplate("^anyHeaderValue$", "anyHeaderValue"),
            };

            this.Given(x => x.GivenIHaveUpstreamHeaders(upstreamHeaders))
                .And(x => x.GivenIHaveTemplateHeadersInRoute(templateHeaders))
                .When(x => x.WhenIMatchTheHeaders())
                .Then(x => x.ThenTheResultIsFalse())
                .BDDfy();
        }

        [Fact]
        public void should_match_the_same_headers_when_differ_case_and_case_insensitive()
        {
            var upstreamHeaders = new Dictionary<string, string>()
            {
                ["anyHeader"] = "ANYHEADERVALUE",
            };

            var templateHeaders = new Dictionary<string, UpstreamHeaderTemplate>()
            {
                ["anyHeader"] = new UpstreamHeaderTemplate("^(?i)anyHeaderValue$", "anyHeaderValue"),
            };

            this.Given(x => x.GivenIHaveUpstreamHeaders(upstreamHeaders))
                .And(x => x.GivenIHaveTemplateHeadersInRoute(templateHeaders))
                .When(x => x.WhenIMatchTheHeaders())
                .Then(x => x.ThenTheResultIsTrue())
                .BDDfy();
        }

        [Fact]
        public void should_not_match_different_headers_values()
        {
            var upstreamHeaders = new Dictionary<string, string>()
            {
                ["anyHeader"] = "anyHeaderValueDifferent",
            };

            var templateHeaders = new Dictionary<string, UpstreamHeaderTemplate>()
            {
                ["anyHeader"] = new UpstreamHeaderTemplate("^(?i)anyHeaderValue$", "anyHeaderValue"),
            };

            this.Given(x => x.GivenIHaveUpstreamHeaders(upstreamHeaders))
                .And(x => x.GivenIHaveTemplateHeadersInRoute(templateHeaders))
                .When(x => x.WhenIMatchTheHeaders())
                .Then(x => x.ThenTheResultIsFalse())
                .BDDfy();
        }

        [Fact]
        public void should_not_match_the_same_headers_names()
        {
            var upstreamHeaders = new Dictionary<string, string>()
            {
                ["anyHeaderDifferent"] = "anyHeaderValue",
            };

            var templateHeaders = new Dictionary<string, UpstreamHeaderTemplate>()
            {
                ["anyHeader"] = new UpstreamHeaderTemplate("^(?i)anyHeaderValue$", "anyHeaderValue"),
            };

            this.Given(x => x.GivenIHaveUpstreamHeaders(upstreamHeaders))
                .And(x => x.GivenIHaveTemplateHeadersInRoute(templateHeaders))
                .When(x => x.WhenIMatchTheHeaders())
                .Then(x => x.ThenTheResultIsFalse())
                .BDDfy();
        }

        [Fact]
        public void should_match_all_the_same_headers()
        {
            var upstreamHeaders = new Dictionary<string, string>()
            {
                ["anyHeader"] = "anyHeaderValue",
                ["notNeededHeader"] = "notNeededHeaderValue",
                ["secondHeader"] = "secondHeaderValue",
                ["thirdHeader"] = "thirdHeaderValue",
            };

            var templateHeaders = new Dictionary<string, UpstreamHeaderTemplate>()
            {
                ["secondHeader"] = new UpstreamHeaderTemplate("^(?i)secondHeaderValue$", "secondHeaderValue"),
                ["thirdHeader"] = new UpstreamHeaderTemplate("^(?i)thirdHeaderValue$", "thirdHeaderValue"),
                ["anyHeader"] = new UpstreamHeaderTemplate("^(?i)anyHeaderValue$", "anyHeaderValue"),
            };

            this.Given(x => x.GivenIHaveUpstreamHeaders(upstreamHeaders))
                .And(x => x.GivenIHaveTemplateHeadersInRoute(templateHeaders))
                .When(x => x.WhenIMatchTheHeaders())
                .Then(x => x.ThenTheResultIsTrue())
                .BDDfy();
        }

        [Fact]
        public void should_not_match_the_headers_when_one_of_them_different()
        {
            var upstreamHeaders = new Dictionary<string, string>()
            {
                ["anyHeader"] = "anyHeaderValue",
                ["notNeededHeader"] = "notNeededHeaderValue",
                ["secondHeader"] = "secondHeaderValueDIFFERENT",
                ["thirdHeader"] = "thirdHeaderValue",
            };

            var templateHeaders = new Dictionary<string, UpstreamHeaderTemplate>()
            {
                ["secondHeader"] = new UpstreamHeaderTemplate("^(?i)secondHeaderValue$", "secondHeaderValue"),
                ["thirdHeader"] = new UpstreamHeaderTemplate("^(?i)thirdHeaderValue$", "thirdHeaderValue"),
                ["anyHeader"] = new UpstreamHeaderTemplate("^(?i)anyHeaderValue$", "anyHeaderValue"),
            };

            this.Given(x => x.GivenIHaveUpstreamHeaders(upstreamHeaders))
                .And(x => x.GivenIHaveTemplateHeadersInRoute(templateHeaders))
                .When(x => x.WhenIMatchTheHeaders())
                .Then(x => x.ThenTheResultIsFalse())
                .BDDfy();
        }

        [Fact]
        public void should_match_the_header_with_placeholder()
        {
            var upstreamHeaders = new Dictionary<string, string>()
            {
                ["anyHeader"] = "PL",
            };

            var templateHeaders = new Dictionary<string, UpstreamHeaderTemplate>()
            {
                ["anyHeader"] = new UpstreamHeaderTemplate("^(?i)(?<countrycode>.+)$", "{header:countrycode}"),
            };

            this.Given(x => x.GivenIHaveUpstreamHeaders(upstreamHeaders))
                .And(x => x.GivenIHaveTemplateHeadersInRoute(templateHeaders))
                .When(x => x.WhenIMatchTheHeaders())
                .Then(x => x.ThenTheResultIsTrue())
                .BDDfy();
        }

        [Fact]
        public void should_match_the_header_with_placeholders()
        {
            var upstreamHeaders = new Dictionary<string, string>()
            {
                ["anyHeader"] = "PL-V1",
            };

            var templateHeaders = new Dictionary<string, UpstreamHeaderTemplate>()
            {
                ["anyHeader"] = new UpstreamHeaderTemplate("^(?i)(?<countrycode>.+)-(?<version>.+)$", "{header:countrycode}-{header:version}"),
            };

            this.Given(x => x.GivenIHaveUpstreamHeaders(upstreamHeaders))
                .And(x => x.GivenIHaveTemplateHeadersInRoute(templateHeaders))
                .When(x => x.WhenIMatchTheHeaders())
                .Then(x => x.ThenTheResultIsTrue())
                .BDDfy();
        }

        [Fact]
        public void should_not_match_the_header_with_placeholders()
        {
            var upstreamHeaders = new Dictionary<string, string>()
            {
                ["anyHeader"] = "PL",
            };

            var templateHeaders = new Dictionary<string, UpstreamHeaderTemplate>()
            {
                ["anyHeader"] = new UpstreamHeaderTemplate("^(?i)(?<countrycode>.+)-(?<version>.+)$", "{header:countrycode}-{header:version}"),
            };

            this.Given(x => x.GivenIHaveUpstreamHeaders(upstreamHeaders))
                .And(x => x.GivenIHaveTemplateHeadersInRoute(templateHeaders))
                .When(x => x.WhenIMatchTheHeaders())
                .Then(x => x.ThenTheResultIsFalse())
                .BDDfy();
        }

        private void GivenIHaveUpstreamHeaders(Dictionary<string, string> upstreamHeaders)
        {
            _upstreamHeaders = upstreamHeaders;
        }

        private void GivenIHaveTemplateHeadersInRoute(Dictionary<string, UpstreamHeaderTemplate> templateHeaders)
        {
            _templateHeaders = templateHeaders;
        }

        private void WhenIMatchTheHeaders()
        {
            _result = _headerMatcher.Match(_upstreamHeaders, _templateHeaders);
        }

        private void ThenTheResultIsTrue()
        {
            _result.ShouldBeTrue();
        }

        private void ThenTheResultIsFalse()
        {
            _result.ShouldBeFalse();
        }
    }
}

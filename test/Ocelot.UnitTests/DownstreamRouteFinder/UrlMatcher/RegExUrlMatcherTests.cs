using Ocelot.DownstreamRouteFinder.UrlMatcher;
using Ocelot.Responses;
using Ocelot.Values;
using Shouldly;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.DownstreamRouteFinder.UrlMatcher
{
    public class RegExUrlMatcherTests
    {
        private readonly IUrlPathToUrlTemplateMatcher _urlMatcher;
        private string _path;
        private string _downstreamPathTemplate;
        private Response<UrlMatch> _result;
        private string _queryString;
        private bool _containsQueryString;

        public RegExUrlMatcherTests()
        {
            _urlMatcher = new RegExUrlMatcher();
        }

        [Fact]
        public void should_not_match()
        {
            this.Given(x => x.GivenIHaveAUpstreamPath("/api/v1/aaaaaaaaa/cards"))
              .And(x => x.GivenIHaveAnUpstreamUrlTemplatePattern("^(?i)/api/v[^/]+/cards$"))
              .When(x => x.WhenIMatchThePaths())
              .And(x => x.ThenTheResultIsFalse())
              .BDDfy();
        }

        [Fact]
        public void should_match()
        {
            this.Given(x => x.GivenIHaveAUpstreamPath("/api/v1/cards"))
              .And(x => x.GivenIHaveAnUpstreamUrlTemplatePattern("^(?i)/api/v[^/]+/cards$"))
              .When(x => x.WhenIMatchThePaths())
              .And(x => x.ThenTheResultIsTrue())
              .BDDfy();
        }

        [Fact]
        public void should_match_path_with_no_query_string()
        {
            const string regExForwardSlashAndOnePlaceHolder = "^(?i)/newThing$";

            this.Given(x => x.GivenIHaveAUpstreamPath("/newThing"))
                .And(_ => GivenIHaveAQueryString("?DeviceType=IphoneApp&Browser=moonpigIphone&BrowserString=-&CountryCode=123&DeviceName=iPhone 5 (GSM+CDMA)&OperatingSystem=iPhone OS 7.1.2&BrowserVersion=3708AdHoc&ipAddress=-"))
                .And(x => x.GivenIHaveAnUpstreamUrlTemplatePattern(regExForwardSlashAndOnePlaceHolder))
                .When(x => x.WhenIMatchThePaths())
                .And(x => x.ThenTheResultIsTrue())
                .BDDfy();
        }

        [Fact]
        public void should_match_query_string()
        {
            const string regExForwardSlashAndOnePlaceHolder = "^(?i)/api/subscriptions/[^/]+/updates\\?unitId=.+$";

            this.Given(x => x.GivenIHaveAUpstreamPath("/api/subscriptions/1/updates"))
                .And(_ => GivenIHaveAQueryString("?unitId=2"))
                .And(x => x.GivenIHaveAnUpstreamUrlTemplatePattern(regExForwardSlashAndOnePlaceHolder))
                .And(_ => GivenThereIsAQueryInTemplate())
                .When(x => x.WhenIMatchThePaths())
                .And(x => x.ThenTheResultIsTrue())
                .BDDfy();
        }

        [Fact]
        public void should_match_query_string_with_multiple_params()
        {
            const string regExForwardSlashAndOnePlaceHolder = "^(?i)/api/subscriptions/[^/]+/updates\\?unitId=.+&productId=.+$";

            this.Given(x => x.GivenIHaveAUpstreamPath("/api/subscriptions/1/updates?unitId=2"))
                .And(_ => GivenIHaveAQueryString("?unitId=2&productId=2"))
                .And(x => x.GivenIHaveAnUpstreamUrlTemplatePattern(regExForwardSlashAndOnePlaceHolder))
                .And(_ => GivenThereIsAQueryInTemplate())
                .When(x => x.WhenIMatchThePaths())
                .And(x => x.ThenTheResultIsTrue())
                .BDDfy();
        }

        [Fact]
        public void should_not_match_slash_becaue_we_need_to_match_something_after_it()
        {
            const string regExForwardSlashAndOnePlaceHolder = "^/[0-9a-zA-Z].+";

            this.Given(x => x.GivenIHaveAUpstreamPath("/"))
              .And(x => x.GivenIHaveAnUpstreamUrlTemplatePattern(regExForwardSlashAndOnePlaceHolder))
              .When(x => x.WhenIMatchThePaths())
              .And(x => x.ThenTheResultIsFalse())
              .BDDfy();
        }

        [Fact]
        public void should_not_match_forward_slash_only_regex()
        {
            this.Given(x => x.GivenIHaveAUpstreamPath("/working/"))
              .And(x => x.GivenIHaveAnUpstreamUrlTemplatePattern("^/$"))
              .When(x => x.WhenIMatchThePaths())
              .And(x => x.ThenTheResultIsFalse())
              .BDDfy();
        }

        [Fact]
        public void should_not_match_issue_134()
        {
            this.Given(x => x.GivenIHaveAUpstreamPath("/api/vacancy/1/"))
              .And(x => x.GivenIHaveAnUpstreamUrlTemplatePattern("^(?i)/vacancy/[^/]+/$"))
              .When(x => x.WhenIMatchThePaths())
              .And(x => x.ThenTheResultIsFalse())
              .BDDfy();
        }

        [Fact]
        public void should_match_forward_slash_only_regex()
        {
            this.Given(x => x.GivenIHaveAUpstreamPath("/"))
              .And(x => x.GivenIHaveAnUpstreamUrlTemplatePattern("^/$"))
              .When(x => x.WhenIMatchThePaths())
              .And(x => x.ThenTheResultIsTrue())
              .BDDfy();
        }

        [Fact]
        public void should_find_match_when_template_smaller_than_valid_path()
        {
            this.Given(x => x.GivenIHaveAUpstreamPath("/api/products/2354325435624623464235"))
                .And(x => x.GivenIHaveAnUpstreamUrlTemplatePattern("^/api/products/.+$"))
                .When(x => x.WhenIMatchThePaths())
                .And(x => x.ThenTheResultIsTrue())
                .BDDfy();
        }

        [Fact]
        public void should_not_find_match()
        {
            this.Given(x => x.GivenIHaveAUpstreamPath("/api/values"))
                .And(x => x.GivenIHaveAnUpstreamUrlTemplatePattern("^/$"))
                .When(x => x.WhenIMatchThePaths())
                .And(x => x.ThenTheResultIsFalse())
                .BDDfy();
        }

        [Fact]
        public void can_match_down_stream_url()
        {
            this.Given(x => x.GivenIHaveAUpstreamPath(""))
                .And(x => x.GivenIHaveAnUpstreamUrlTemplatePattern("^$"))
                .When(x => x.WhenIMatchThePaths())
                .And(x => x.ThenTheResultIsTrue())
                .BDDfy();
        }

        [Fact]
        public void can_match_down_stream_url_with_no_slash()
        {
            this.Given(x => x.GivenIHaveAUpstreamPath("api"))
                 .Given(x => x.GivenIHaveAnUpstreamUrlTemplatePattern("^api$"))
                 .When(x => x.WhenIMatchThePaths())
                 .Then(x => x.ThenTheResultIsTrue())
                 .BDDfy();
        }

        [Fact]
        public void can_match_down_stream_url_with_one_slash()
        {
            this.Given(x => x.GivenIHaveAUpstreamPath("api/"))
                 .Given(x => x.GivenIHaveAnUpstreamUrlTemplatePattern("^api/$"))
                 .When(x => x.WhenIMatchThePaths())
                 .Then(x => x.ThenTheResultIsTrue())
                 .BDDfy();
        }

        [Fact]
        public void can_match_down_stream_url_with_downstream_template()
        {
            this.Given(x => x.GivenIHaveAUpstreamPath("api/product/products/"))
              .Given(x => x.GivenIHaveAnUpstreamUrlTemplatePattern("^api/product/products/$"))
              .When(x => x.WhenIMatchThePaths())
              .Then(x => x.ThenTheResultIsTrue())
              .BDDfy();
        }

        [Fact]
        public void can_match_down_stream_url_with_downstream_template_with_one_place_holder()
        {
            this.Given(x => x.GivenIHaveAUpstreamPath("api/product/products/1"))
               .Given(x => x.GivenIHaveAnUpstreamUrlTemplatePattern("^api/product/products/.+$"))
               .When(x => x.WhenIMatchThePaths())
               .Then(x => x.ThenTheResultIsTrue())
               .BDDfy();
        }

        [Fact]
        public void can_match_down_stream_url_with_downstream_template_with_two_place_holders()
        {
            this.Given(x => x.GivenIHaveAUpstreamPath("api/product/products/1/2"))
                 .Given(x => x.GivenIHaveAnUpstreamUrlTemplatePattern("^api/product/products/[^/]+/.+$"))
                 .When(x => x.WhenIMatchThePaths())
                 .Then(x => x.ThenTheResultIsTrue())
                 .BDDfy();
        }

        [Fact]
        public void can_match_down_stream_url_with_downstream_template_with_two_place_holders_seperated_by_something()
        {
            this.Given(x => x.GivenIHaveAUpstreamPath("api/product/products/1/categories/2"))
                .And(x => x.GivenIHaveAnUpstreamUrlTemplatePattern("^api/product/products/[^/]+/categories/.+$"))
                .When(x => x.WhenIMatchThePaths())
                .Then(x => x.ThenTheResultIsTrue())
                .BDDfy();
        }

        [Fact]
        public void can_match_down_stream_url_with_downstream_template_with_three_place_holders_seperated_by_something()
        {
            this.Given(x => x.GivenIHaveAUpstreamPath("api/product/products/1/categories/2/variant/123"))
                .And(x => x.GivenIHaveAnUpstreamUrlTemplatePattern("^api/product/products/[^/]+/categories/[^/]+/variant/.+$"))
                .When(x => x.WhenIMatchThePaths())
                .Then(x => x.ThenTheResultIsTrue())
                .BDDfy();
        }

        [Fact]
        public void can_match_down_stream_url_with_downstream_template_with_three_place_holders()
        {
            this.Given(x => x.GivenIHaveAUpstreamPath("api/product/products/1/categories/2/variant/"))
                 .And(x => x.GivenIHaveAnUpstreamUrlTemplatePattern("^api/product/products/[^/]+/categories/[^/]+/variant/$"))
                 .When(x => x.WhenIMatchThePaths())
                 .Then(x => x.ThenTheResultIsTrue())
                 .BDDfy();
        }

        [Fact]
        public void should_ignore_case_sensitivity()
        {
            this.Given(x => x.GivenIHaveAUpstreamPath("API/product/products/1/categories/2/variant/"))
               .And(x => x.GivenIHaveAnUpstreamUrlTemplatePattern("^(?i)api/product/products/[^/]+/categories/[^/]+/variant/$"))
               .When(x => x.WhenIMatchThePaths())
               .Then(x => x.ThenTheResultIsTrue())
               .BDDfy();
        }

        [Fact]
        public void should_respect_case_sensitivity()
        {
            this.Given(x => x.GivenIHaveAUpstreamPath("API/product/products/1/categories/2/variant/"))
              .And(x => x.GivenIHaveAnUpstreamUrlTemplatePattern("^api/product/products/[^/]+/categories/[^/]+/variant/$"))
              .When(x => x.WhenIMatchThePaths())
              .Then(x => x.ThenTheResultIsFalse())
              .BDDfy();
        }

        private void GivenIHaveAUpstreamPath(string path)
        {
            _path = path;
        }

        private void GivenIHaveAQueryString(string queryString)
        {
            _queryString = queryString;
        }

        private void GivenIHaveAnUpstreamUrlTemplatePattern(string downstreamUrlTemplate)
        {
            _downstreamPathTemplate = downstreamUrlTemplate;
        }

        private void WhenIMatchThePaths()
        {
            _result = _urlMatcher.Match(_path, _queryString, new UpstreamPathTemplate(_downstreamPathTemplate, 0, _containsQueryString, _downstreamPathTemplate));
        }

        private void ThenTheResultIsTrue()
        {
            _result.Data.Match.ShouldBeTrue();
        }

        private void ThenTheResultIsFalse()
        {
            _result.Data.Match.ShouldBeFalse();
        }

        private void GivenThereIsAQueryInTemplate()
        {
            _containsQueryString = true;
        }
    }
}

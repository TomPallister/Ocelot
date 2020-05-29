using Ocelot.Configuration.Creator;
using Ocelot.Configuration.File;
using Ocelot.Values;
using Shouldly;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.Configuration
{
    public class UpstreamTemplatePatternCreatorTests
    {
        private FileRoute _fileRoute;
        private readonly UpstreamTemplatePatternCreator _creator;
        private UpstreamPathTemplate _result;

        public UpstreamTemplatePatternCreatorTests()
        {
            _creator = new UpstreamTemplatePatternCreator();
        }

        [Fact]
        public void should_match_up_to_next_slash()
        {
            var fileRoute = new FileRoute
            {
                UpstreamPathTemplate = "/api/v{apiVersion}/cards",
                Priority = 0
            };

            this.Given(x => x.GivenTheFollowingFileRoute(fileRoute))
                .When(x => x.WhenICreateTheTemplatePattern())
                .Then(x => x.ThenTheFollowingIsReturned("^(?i)/api/v[^/]+/cards$"))
                .And(x => ThenThePriorityIs(0))
                .BDDfy();
        }

        [Fact]
        public void should_use_re_route_priority()
        {
            var fileRoute = new FileRoute
            {
                UpstreamPathTemplate = "/orders/{catchAll}",
                Priority = 0
            };

            this.Given(x => x.GivenTheFollowingFileRoute(fileRoute))
                .When(x => x.WhenICreateTheTemplatePattern())
                .Then(x => x.ThenTheFollowingIsReturned("^(?i)/orders/.+$"))
                .And(x => ThenThePriorityIs(0))
                .BDDfy();
        }

        [Fact]
        public void should_use_zero_priority()
        {
            var fileRoute = new FileRoute
            {
                UpstreamPathTemplate = "/{catchAll}",
                Priority = 1
            };

            this.Given(x => x.GivenTheFollowingFileRoute(fileRoute))
                .When(x => x.WhenICreateTheTemplatePattern())
                .Then(x => x.ThenTheFollowingIsReturned("^/.*"))
                .And(x => ThenThePriorityIs(0))
                .BDDfy();
        }

        [Fact]
        public void should_set_upstream_template_pattern_to_ignore_case_sensitivity()
        {
            var fileRoute = new FileRoute
            {
                UpstreamPathTemplate = "/PRODUCTS/{productId}",
                RouteIsCaseSensitive = false
            };

            this.Given(x => x.GivenTheFollowingFileRoute(fileRoute))
                .When(x => x.WhenICreateTheTemplatePattern())
                .Then(x => x.ThenTheFollowingIsReturned("^(?i)/PRODUCTS/.+$"))
                .And(x => ThenThePriorityIs(1))
                .BDDfy();
        }

        [Fact]
        public void should_match_forward_slash_or_no_forward_slash_if_template_end_with_forward_slash()
        {
            var fileRoute = new FileRoute
            {
                UpstreamPathTemplate = "/PRODUCTS/",
                RouteIsCaseSensitive = false
            };

            this.Given(x => x.GivenTheFollowingFileRoute(fileRoute))
                .When(x => x.WhenICreateTheTemplatePattern())
                .Then(x => x.ThenTheFollowingIsReturned("^(?i)/PRODUCTS(/|)$"))
                .And(x => ThenThePriorityIs(1))
                .BDDfy();
        }

        [Fact]
        public void should_set_upstream_template_pattern_to_respect_case_sensitivity()
        {
            var fileRoute = new FileRoute
            {
                UpstreamPathTemplate = "/PRODUCTS/{productId}",
                RouteIsCaseSensitive = true
            };
            this.Given(x => x.GivenTheFollowingFileRoute(fileRoute))
                .When(x => x.WhenICreateTheTemplatePattern())
                .Then(x => x.ThenTheFollowingIsReturned("^/PRODUCTS/.+$"))
                .And(x => ThenThePriorityIs(1))
                .BDDfy();
        }

        [Fact]
        public void should_create_template_pattern_that_matches_anything_to_end_of_string()
        {
            var fileRoute = new FileRoute
            {
                UpstreamPathTemplate = "/api/products/{productId}",
                RouteIsCaseSensitive = true
            };

            this.Given(x => x.GivenTheFollowingFileRoute(fileRoute))
                .When(x => x.WhenICreateTheTemplatePattern())
                .Then(x => x.ThenTheFollowingIsReturned("^/api/products/.+$"))
                .And(x => ThenThePriorityIs(1))
                .BDDfy();
        }

        [Fact]
        public void should_create_template_pattern_that_matches_more_than_one_placeholder()
        {
            var fileRoute = new FileRoute
            {
                UpstreamPathTemplate = "/api/products/{productId}/variants/{variantId}",
                RouteIsCaseSensitive = true
            };

            this.Given(x => x.GivenTheFollowingFileRoute(fileRoute))
                .When(x => x.WhenICreateTheTemplatePattern())
                .Then(x => x.ThenTheFollowingIsReturned("^/api/products/[^/]+/variants/.+$"))
                .And(x => ThenThePriorityIs(1))
                .BDDfy();
        }

        [Fact]
        public void should_create_template_pattern_that_matches_more_than_one_placeholder_with_trailing_slash()
        {
            var fileRoute = new FileRoute
            {
                UpstreamPathTemplate = "/api/products/{productId}/variants/{variantId}/",
                RouteIsCaseSensitive = true
            };

            this.Given(x => x.GivenTheFollowingFileRoute(fileRoute))
                .When(x => x.WhenICreateTheTemplatePattern())
                .Then(x => x.ThenTheFollowingIsReturned("^/api/products/[^/]+/variants/[^/]+(/|)$"))
                .And(x => ThenThePriorityIs(1))
                .BDDfy();
        }

        [Fact]
        public void should_create_template_pattern_that_matches_to_end_of_string()
        {
            var fileRoute = new FileRoute
            {
                UpstreamPathTemplate = "/"
            };

            this.Given(x => x.GivenTheFollowingFileRoute(fileRoute))
                .When(x => x.WhenICreateTheTemplatePattern())
                .Then(x => x.ThenTheFollowingIsReturned("^/$"))
                .And(x => ThenThePriorityIs(1))
                .BDDfy();
        }

        [Fact]
        public void should_create_template_pattern_that_matches_to_end_of_string_when_slash_and_placeholder()
        {
            var fileRoute = new FileRoute
            {
                UpstreamPathTemplate = "/{url}"
            };

            this.Given(x => x.GivenTheFollowingFileRoute(fileRoute))
                .When(x => x.WhenICreateTheTemplatePattern())
                .Then(x => x.ThenTheFollowingIsReturned("^/.*"))
                .And(x => ThenThePriorityIs(0))
                .BDDfy();
        }

        [Fact]
        public void should_create_template_pattern_that_starts_with_placeholder_then_has_another_later()
        {
            var fileRoute = new FileRoute
            {
                UpstreamPathTemplate = "/{productId}/products/variants/{variantId}/",
                RouteIsCaseSensitive = true
            };

            this.Given(x => x.GivenTheFollowingFileRoute(fileRoute))
                .When(x => x.WhenICreateTheTemplatePattern())
                .Then(x => x.ThenTheFollowingIsReturned("^/[^/]+/products/variants/[^/]+(/|)$"))
                .And(x => ThenThePriorityIs(1))
                .BDDfy();
        }

        [Fact]
        public void should_create_template_pattern_that_matches_query_string()
        {
            var fileRoute = new FileRoute
            {
                UpstreamPathTemplate = "/api/subscriptions/{subscriptionId}/updates?unitId={unitId}"
            };

            this.Given(x => x.GivenTheFollowingFileRoute(fileRoute))
                .When(x => x.WhenICreateTheTemplatePattern())
                .Then(x => x.ThenTheFollowingIsReturned("^(?i)/api/subscriptions/[^/]+/updates\\?unitId=.+$"))
                .And(x => ThenThePriorityIs(1))
                .BDDfy();
        }

        [Fact]
        public void should_create_template_pattern_that_matches_query_string_with_multiple_params()
        {
            var fileRoute = new FileRoute
            {
                UpstreamPathTemplate = "/api/subscriptions/{subscriptionId}/updates?unitId={unitId}&productId={productId}"
            };

            this.Given(x => x.GivenTheFollowingFileRoute(fileRoute))
                .When(x => x.WhenICreateTheTemplatePattern())
                .Then(x => x.ThenTheFollowingIsReturned("^(?i)/api/subscriptions/[^/]+/updates\\?unitId=.+&productId=.+$"))
                .And(x => ThenThePriorityIs(1))
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

        private void ThenTheFollowingIsReturned(string expected)
        {
            _result.Template.ShouldBe(expected);
        }

        private void ThenThePriorityIs(int v)
        {
            _result.Priority.ShouldBe(v);
        }
    }
}

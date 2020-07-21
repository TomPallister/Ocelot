using Ocelot.Configuration.Builder;
using Ocelot.DownstreamRouteFinder;
using Ocelot.DownstreamRouteFinder.UrlMatcher;
using Ocelot.DownstreamUrlCreator.UrlTemplateReplacer;
using Ocelot.Responses;
using Ocelot.Values;
using Shouldly;
using System.Collections.Generic;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.DownstreamUrlCreator.UrlTemplateReplacer
{
    public class UpstreamUrlPathTemplateVariableReplacerTests
    {
        private DownstreamRouteHolder _downstreamRoute;
        private Response<DownstreamPath> _result;
        private readonly IDownstreamPathPlaceholderReplacer _downstreamPathReplacer;

        public UpstreamUrlPathTemplateVariableReplacerTests()
        {
            _downstreamPathReplacer = new DownstreamTemplatePathPlaceholderReplacer();
        }

        [Fact]
        public void can_replace_no_template_variables()
        {
            this.Given(x => x.GivenThereIsAUrlMatch(
                new DownstreamRouteHolder(
                    new List<PlaceholderNameAndValue>(),
                    new RouteBuilder()
                        .WithDownstreamRoute(new DownstreamRouteBuilder()
                            .WithUpstreamHttpMethod(new List<string> { "Get" })
                            .Build())
                        .WithUpstreamHttpMethod(new List<string> { "Get" })
                        .Build())))
                .When(x => x.WhenIReplaceTheTemplateVariables())
                .Then(x => x.ThenTheDownstreamUrlPathIsReturned(""))
                .BDDfy();
        }

        [Fact]
        public void can_replace_no_template_variables_with_slash()
        {
            this.Given(x => x.GivenThereIsAUrlMatch(
                new DownstreamRouteHolder(
                new List<PlaceholderNameAndValue>(),
                new RouteBuilder()
                    .WithDownstreamRoute(new DownstreamRouteBuilder()
                        .WithDownstreamPathTemplate("/")
                        .WithUpstreamHttpMethod(new List<string> { "Get" })
                        .Build())
                    .WithUpstreamHttpMethod(new List<string> { "Get" })
                    .Build())))
                .When(x => x.WhenIReplaceTheTemplateVariables())
                .Then(x => x.ThenTheDownstreamUrlPathIsReturned("/"))
                .BDDfy();
        }

        [Fact]
        public void can_replace_url_no_slash()
        {
            this.Given(x => x.GivenThereIsAUrlMatch(new DownstreamRouteHolder(new List<PlaceholderNameAndValue>(),
                new RouteBuilder()
                    .WithDownstreamRoute(new DownstreamRouteBuilder()
                        .WithDownstreamPathTemplate("api")
                        .WithUpstreamHttpMethod(new List<string> { "Get" })
                        .Build())
                    .WithUpstreamHttpMethod(new List<string> { "Get" })
                    .Build())))
                .When(x => x.WhenIReplaceTheTemplateVariables())
                .Then(x => x.ThenTheDownstreamUrlPathIsReturned("api"))
                .BDDfy();
        }

        [Fact]
        public void can_replace_url_one_slash()
        {
            this.Given(x => x.GivenThereIsAUrlMatch(new DownstreamRouteHolder(new List<PlaceholderNameAndValue>(),
                new RouteBuilder()
                    .WithDownstreamRoute(new DownstreamRouteBuilder()
                        .WithDownstreamPathTemplate("api/")
                        .WithUpstreamHttpMethod(new List<string> { "Get" })
                        .Build())
                    .WithUpstreamHttpMethod(new List<string> { "Get" })
                    .Build())))
                .When(x => x.WhenIReplaceTheTemplateVariables())
                .Then(x => x.ThenTheDownstreamUrlPathIsReturned("api/"))
                .BDDfy();
        }

        [Fact]
        public void can_replace_url_multiple_slash()
        {
            this.Given(x => x.GivenThereIsAUrlMatch(new DownstreamRouteHolder(new List<PlaceholderNameAndValue>(),
                new RouteBuilder()
                    .WithDownstreamRoute(new DownstreamRouteBuilder()
                        .WithDownstreamPathTemplate("api/product/products/")
                        .WithUpstreamHttpMethod(new List<string> { "Get" })
                        .Build())
                    .WithUpstreamHttpMethod(new List<string> { "Get" })
                    .Build())))
                .When(x => x.WhenIReplaceTheTemplateVariables())
                .Then(x => x.ThenTheDownstreamUrlPathIsReturned("api/product/products/"))
                .BDDfy();
        }

        [Fact]
        public void can_replace_url_one_template_variable()
        {
            var templateVariables = new List<PlaceholderNameAndValue>()
            {
                new PlaceholderNameAndValue("{productId}", "1")
            };

            this.Given(x => x.GivenThereIsAUrlMatch(new DownstreamRouteHolder(templateVariables,
                new RouteBuilder()
                    .WithDownstreamRoute(new DownstreamRouteBuilder()
                        .WithDownstreamPathTemplate("productservice/products/{productId}/")
                        .WithUpstreamHttpMethod(new List<string> { "Get" })
                        .Build())
                    .WithUpstreamHttpMethod(new List<string> { "Get" })
                    .Build())))
             .When(x => x.WhenIReplaceTheTemplateVariables())
             .Then(x => x.ThenTheDownstreamUrlPathIsReturned("productservice/products/1/"))
             .BDDfy();
        }

        [Fact]
        public void can_replace_url_one_template_variable_with_path_after()
        {
            var templateVariables = new List<PlaceholderNameAndValue>()
            {
                new PlaceholderNameAndValue("{productId}", "1")
            };

            this.Given(x => x.GivenThereIsAUrlMatch(new DownstreamRouteHolder(templateVariables,
                new RouteBuilder()
                    .WithDownstreamRoute(new DownstreamRouteBuilder()
                        .WithDownstreamPathTemplate("productservice/products/{productId}/variants")
                        .WithUpstreamHttpMethod(new List<string> { "Get" })
                        .Build())
                    .WithUpstreamHttpMethod(new List<string> { "Get" })
                    .Build())))
             .When(x => x.WhenIReplaceTheTemplateVariables())
             .Then(x => x.ThenTheDownstreamUrlPathIsReturned("productservice/products/1/variants"))
             .BDDfy();
        }

        [Fact]
        public void can_replace_url_two_template_variable()
        {
            var templateVariables = new List<PlaceholderNameAndValue>()
            {
                new PlaceholderNameAndValue("{productId}", "1"),
                new PlaceholderNameAndValue("{variantId}", "12")
            };

            this.Given(x => x.GivenThereIsAUrlMatch(new DownstreamRouteHolder(templateVariables,
                new RouteBuilder()
                    .WithDownstreamRoute(new DownstreamRouteBuilder()
                        .WithDownstreamPathTemplate("productservice/products/{productId}/variants/{variantId}")
                        .WithUpstreamHttpMethod(new List<string> { "Get" })
                        .Build())
                    .WithUpstreamHttpMethod(new List<string> { "Get" })
                    .Build())))
             .When(x => x.WhenIReplaceTheTemplateVariables())
             .Then(x => x.ThenTheDownstreamUrlPathIsReturned("productservice/products/1/variants/12"))
             .BDDfy();
        }

        [Fact]
        public void can_replace_url_three_template_variable()
        {
            var templateVariables = new List<PlaceholderNameAndValue>()
            {
                new PlaceholderNameAndValue("{productId}", "1"),
                new PlaceholderNameAndValue("{variantId}", "12"),
                new PlaceholderNameAndValue("{categoryId}", "34")
            };

            this.Given(x => x.GivenThereIsAUrlMatch(new DownstreamRouteHolder(templateVariables,
                new RouteBuilder()
                    .WithDownstreamRoute(new DownstreamRouteBuilder()
                        .WithDownstreamPathTemplate("productservice/category/{categoryId}/products/{productId}/variants/{variantId}")
                        .WithUpstreamHttpMethod(new List<string> { "Get" })
                        .Build())
                    .WithUpstreamHttpMethod(new List<string> { "Get" })
                .Build())))
             .When(x => x.WhenIReplaceTheTemplateVariables())
             .Then(x => x.ThenTheDownstreamUrlPathIsReturned("productservice/category/34/products/1/variants/12"))
             .BDDfy();
        }

        private void GivenThereIsAUrlMatch(DownstreamRouteHolder downstreamRoute)
        {
            _downstreamRoute = downstreamRoute;
        }

        private void WhenIReplaceTheTemplateVariables()
        {
            _result = _downstreamPathReplacer.Replace(_downstreamRoute.Route.DownstreamRoute[0].DownstreamPathTemplate.Value, _downstreamRoute.TemplatePlaceholderNameAndValues);
        }

        private void ThenTheDownstreamUrlPathIsReturned(string expected)
        {
            _result.Data.Value.ShouldBe(expected);
        }
    }
}

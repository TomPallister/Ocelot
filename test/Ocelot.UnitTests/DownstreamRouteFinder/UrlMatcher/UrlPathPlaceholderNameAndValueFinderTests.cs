using Ocelot.DownstreamRouteFinder.UrlMatcher;
using Ocelot.Responses;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.DownstreamRouteFinder.UrlMatcher
{
    public class UrlPathPlaceholderNameAndValueFinderTests
    {
        private readonly IPlaceholderNameAndValueFinder _finder;
        private string _downstreamUrlPath;
        private string _downstreamPathTemplate;
        private Response<List<PlaceholderNameAndValue>> _result;
        private string _query;

        public UrlPathPlaceholderNameAndValueFinderTests()
        {
            _finder = new UrlPathPlaceholderNameAndValueFinder();
        }

        [Fact]
        public void can_match_down_stream_url()
        {
            this.Given(x => x.GivenIHaveAUpstreamPath(""))
                .And(x => x.GivenIHaveAnUpstreamUrlTemplate(""))
                .When(x => x.WhenIFindTheUrlVariableNamesAndValues())
                .And(x => x.ThenTheTemplatesVariablesAre(new List<PlaceholderNameAndValue>()))
                .BDDfy();
        }

        [Fact]
        public void can_match_down_stream_url_with_nothing_then_placeholder_no_value_is_blank()
        {
            var expectedTemplates = new List<PlaceholderNameAndValue>
            {
                new PlaceholderNameAndValue("{url}", "")
            };

            this.Given(x => x.GivenIHaveAUpstreamPath(""))
                .And(x => x.GivenIHaveAnUpstreamUrlTemplate("/{url}"))
                .When(x => x.WhenIFindTheUrlVariableNamesAndValues())
                .And(x => x.ThenTheTemplatesVariablesAre(expectedTemplates))
                .BDDfy();
        }

        [Fact]
        public void can_match_down_stream_url_with_nothing_then_placeholder_value_is_test()
        {
            var expectedTemplates = new List<PlaceholderNameAndValue>
            {
                new PlaceholderNameAndValue("{url}", "test")
            };

            this.Given(x => x.GivenIHaveAUpstreamPath("/test"))
                .And(x => x.GivenIHaveAnUpstreamUrlTemplate("/{url}"))
                .When(x => x.WhenIFindTheUrlVariableNamesAndValues())
                .And(x => x.ThenTheTemplatesVariablesAre(expectedTemplates))
                .BDDfy();
        }

        [Fact]
        public void should_match_everything_in_path_with_query()
        {
            var expectedTemplates = new List<PlaceholderNameAndValue>
            {
                new PlaceholderNameAndValue("{everything}", "test/toot")
            };

            this.Given(x => x.GivenIHaveAUpstreamPath("/test/toot"))
                .And(x => GivenIHaveAQuery("?$filter=Name%20eq%20'Sam'"))
                .And(x => x.GivenIHaveAnUpstreamUrlTemplate("/{everything}"))
                .When(x => x.WhenIFindTheUrlVariableNamesAndValues())
                .And(x => x.ThenTheTemplatesVariablesAre(expectedTemplates))
                .BDDfy();
        }

        [Fact]
        public void should_match_everything_in_path()
        {
            var expectedTemplates = new List<PlaceholderNameAndValue>
            {
                new PlaceholderNameAndValue("{everything}", "test/toot")
            };

            this.Given(x => x.GivenIHaveAUpstreamPath("/test/toot"))
                .And(x => x.GivenIHaveAnUpstreamUrlTemplate("/{everything}"))
                .When(x => x.WhenIFindTheUrlVariableNamesAndValues())
                .And(x => x.ThenTheTemplatesVariablesAre(expectedTemplates))
                .BDDfy();
        }

        [Fact]
        public void can_match_down_stream_url_with_forward_slash_then_placeholder_no_value_is_blank()
        {
            var expectedTemplates = new List<PlaceholderNameAndValue>
            {
                new PlaceholderNameAndValue("{url}", "")
            };

            this.Given(x => x.GivenIHaveAUpstreamPath("/"))
                .And(x => x.GivenIHaveAnUpstreamUrlTemplate("/{url}"))
                .When(x => x.WhenIFindTheUrlVariableNamesAndValues())
                .And(x => x.ThenTheTemplatesVariablesAre(expectedTemplates))
                .BDDfy();
        }

        [Fact]
        public void can_match_down_stream_url_with_forward_slash()
        {
            var expectedTemplates = new List<PlaceholderNameAndValue>
            {
            };

            this.Given(x => x.GivenIHaveAUpstreamPath("/"))
                .And(x => x.GivenIHaveAnUpstreamUrlTemplate("/"))
                .When(x => x.WhenIFindTheUrlVariableNamesAndValues())
                .And(x => x.ThenTheTemplatesVariablesAre(expectedTemplates))
                .BDDfy();
        }

        [Fact]
        public void can_match_down_stream_url_with_forward_slash_then_placeholder_then_another_value()
        {
            var expectedTemplates = new List<PlaceholderNameAndValue>
            {
                new PlaceholderNameAndValue("{url}", "1")
            };

            this.Given(x => x.GivenIHaveAUpstreamPath("/1/products"))
                .And(x => x.GivenIHaveAnUpstreamUrlTemplate("/{url}/products"))
                .When(x => x.WhenIFindTheUrlVariableNamesAndValues())
                .And(x => x.ThenTheTemplatesVariablesAre(expectedTemplates))
                .BDDfy();
        }

        [Fact]
        public void should_not_find_anything()
        {
            this.Given(x => x.GivenIHaveAUpstreamPath("/products"))
                .And(x => x.GivenIHaveAnUpstreamUrlTemplate("/products/"))
                .When(x => x.WhenIFindTheUrlVariableNamesAndValues())
                .And(x => x.ThenTheTemplatesVariablesAre(new List<PlaceholderNameAndValue>()))
                .BDDfy();
        }

        [Fact]
        public void should_find_query_string()
        {
            var expectedTemplates = new List<PlaceholderNameAndValue>
            {
                new PlaceholderNameAndValue("{productId}", "1")
            };

            this.Given(x => x.GivenIHaveAUpstreamPath("/products"))
                .And(x => x.GivenIHaveAQuery("?productId=1"))
                .And(x => x.GivenIHaveAnUpstreamUrlTemplate("/products?productId={productId}"))
                .When(x => x.WhenIFindTheUrlVariableNamesAndValues())
                .And(x => x.ThenTheTemplatesVariablesAre(expectedTemplates))
                .BDDfy();
        }

        [Fact]
        public void should_find_query_string_dont_include_hardcoded()
        {
            var expectedTemplates = new List<PlaceholderNameAndValue>
            {
                new PlaceholderNameAndValue("{productId}", "1")
            };

            this.Given(x => x.GivenIHaveAUpstreamPath("/products"))
                .And(x => x.GivenIHaveAQuery("?productId=1&categoryId=2"))
                .And(x => x.GivenIHaveAnUpstreamUrlTemplate("/products?productId={productId}"))
                .When(x => x.WhenIFindTheUrlVariableNamesAndValues())
                .And(x => x.ThenTheTemplatesVariablesAre(expectedTemplates))
                .BDDfy();
        }

        [Fact]
        public void should_find_multiple_query_string()
        {
            var expectedTemplates = new List<PlaceholderNameAndValue>
            {
                new PlaceholderNameAndValue("{productId}", "1"),
                new PlaceholderNameAndValue("{categoryId}", "2")
            };

            this.Given(x => x.GivenIHaveAUpstreamPath("/products"))
                .And(x => x.GivenIHaveAQuery("?productId=1&categoryId=2"))
                .And(x => x.GivenIHaveAnUpstreamUrlTemplate("/products?productId={productId}&categoryId={categoryId}"))
                .When(x => x.WhenIFindTheUrlVariableNamesAndValues())
                .And(x => x.ThenTheTemplatesVariablesAre(expectedTemplates))
                .BDDfy();
        }

        [Fact]
        public void should_find_multiple_query_string_and_path()
        {
            var expectedTemplates = new List<PlaceholderNameAndValue>
            {
                new PlaceholderNameAndValue("{productId}", "1"),
                new PlaceholderNameAndValue("{categoryId}", "2"),
                new PlaceholderNameAndValue("{account}", "3")
            };

            this.Given(x => x.GivenIHaveAUpstreamPath("/products/3"))
                .And(x => x.GivenIHaveAQuery("?productId=1&categoryId=2"))
                .And(x => x.GivenIHaveAnUpstreamUrlTemplate("/products/{account}?productId={productId}&categoryId={categoryId}"))
                .When(x => x.WhenIFindTheUrlVariableNamesAndValues())
                .And(x => x.ThenTheTemplatesVariablesAre(expectedTemplates))
                .BDDfy();
        }

        [Fact]
        public void should_find_multiple_query_string_and_path_that_ends_with_slash()
        {
            var expectedTemplates = new List<PlaceholderNameAndValue>
            {
                new PlaceholderNameAndValue("{productId}", "1"),
                new PlaceholderNameAndValue("{categoryId}", "2"),
                new PlaceholderNameAndValue("{account}", "3")
            };

            this.Given(x => x.GivenIHaveAUpstreamPath("/products/3/"))
                .And(x => x.GivenIHaveAQuery("?productId=1&categoryId=2"))
                .And(x => x.GivenIHaveAnUpstreamUrlTemplate("/products/{account}/?productId={productId}&categoryId={categoryId}"))
                .When(x => x.WhenIFindTheUrlVariableNamesAndValues())
                .And(x => x.ThenTheTemplatesVariablesAre(expectedTemplates))
                .BDDfy();
        }

        [Fact]
        public void can_match_down_stream_url_with_no_slash()
        {
            this.Given(x => x.GivenIHaveAUpstreamPath("api"))
                 .Given(x => x.GivenIHaveAnUpstreamUrlTemplate("api"))
                 .When(x => x.WhenIFindTheUrlVariableNamesAndValues())
                 .And(x => x.ThenTheTemplatesVariablesAre(new List<PlaceholderNameAndValue>()))
                 .BDDfy();
        }

        [Fact]
        public void can_match_down_stream_url_with_one_slash()
        {
            this.Given(x => x.GivenIHaveAUpstreamPath("api/"))
                 .Given(x => x.GivenIHaveAnUpstreamUrlTemplate("api/"))
                 .When(x => x.WhenIFindTheUrlVariableNamesAndValues())
                 .And(x => x.ThenTheTemplatesVariablesAre(new List<PlaceholderNameAndValue>()))
                 .BDDfy();
        }

        [Fact]
        public void can_match_down_stream_url_with_downstream_template()
        {
            this.Given(x => x.GivenIHaveAUpstreamPath("api/product/products/"))
              .Given(x => x.GivenIHaveAnUpstreamUrlTemplate("api/product/products/"))
              .When(x => x.WhenIFindTheUrlVariableNamesAndValues())
              .And(x => x.ThenTheTemplatesVariablesAre(new List<PlaceholderNameAndValue>()))
              .BDDfy();
        }

        [Fact]
        public void can_match_down_stream_url_with_downstream_template_with_one_place_holder()
        {
            var expectedTemplates = new List<PlaceholderNameAndValue>
            {
                new PlaceholderNameAndValue("{productId}", "1")
            };

            this.Given(x => x.GivenIHaveAUpstreamPath("api/product/products/1"))
               .Given(x => x.GivenIHaveAnUpstreamUrlTemplate("api/product/products/{productId}"))
               .When(x => x.WhenIFindTheUrlVariableNamesAndValues())
               .And(x => x.ThenTheTemplatesVariablesAre(expectedTemplates))
               .BDDfy();
        }

        [Fact]
        public void can_match_down_stream_url_with_downstream_template_with_two_place_holders()
        {
            var expectedTemplates = new List<PlaceholderNameAndValue>
            {
                new PlaceholderNameAndValue("{productId}", "1"),
                new PlaceholderNameAndValue("{categoryId}", "2")
            };

            this.Given(x => x.GivenIHaveAUpstreamPath("api/product/products/1/2"))
                 .Given(x => x.GivenIHaveAnUpstreamUrlTemplate("api/product/products/{productId}/{categoryId}"))
                 .When(x => x.WhenIFindTheUrlVariableNamesAndValues())
                 .And(x => x.ThenTheTemplatesVariablesAre(expectedTemplates))
                 .BDDfy();
        }

        [Fact]
        public void can_match_down_stream_url_with_downstream_template_with_two_place_holders_seperated_by_something()
        {
            var expectedTemplates = new List<PlaceholderNameAndValue>
            {
                new PlaceholderNameAndValue("{productId}", "1"),
                new PlaceholderNameAndValue("{categoryId}", "2")
            };

            this.Given(x => x.GivenIHaveAUpstreamPath("api/product/products/1/categories/2"))
                .And(x => x.GivenIHaveAnUpstreamUrlTemplate("api/product/products/{productId}/categories/{categoryId}"))
                .When(x => x.WhenIFindTheUrlVariableNamesAndValues())
                .And(x => x.ThenTheTemplatesVariablesAre(expectedTemplates))
                .BDDfy();
        }

        [Fact]
        public void can_match_down_stream_url_with_downstream_template_with_three_place_holders_seperated_by_something()
        {
            var expectedTemplates = new List<PlaceholderNameAndValue>
            {
                new PlaceholderNameAndValue("{productId}", "1"),
                new PlaceholderNameAndValue("{categoryId}", "2"),
                new PlaceholderNameAndValue("{variantId}", "123")
            };

            this.Given(x => x.GivenIHaveAUpstreamPath("api/product/products/1/categories/2/variant/123"))
                .And(x => x.GivenIHaveAnUpstreamUrlTemplate("api/product/products/{productId}/categories/{categoryId}/variant/{variantId}"))
                .When(x => x.WhenIFindTheUrlVariableNamesAndValues())
                .And(x => x.ThenTheTemplatesVariablesAre(expectedTemplates))
                .BDDfy();
        }

        [Fact]
        public void can_match_down_stream_url_with_downstream_template_with_three_place_holders()
        {
            var expectedTemplates = new List<PlaceholderNameAndValue>
            {
                new PlaceholderNameAndValue("{productId}", "1"),
                new PlaceholderNameAndValue("{categoryId}", "2")
            };

            this.Given(x => x.GivenIHaveAUpstreamPath("api/product/products/1/categories/2/variant/"))
                 .And(x => x.GivenIHaveAnUpstreamUrlTemplate("api/product/products/{productId}/categories/{categoryId}/variant/"))
                 .When(x => x.WhenIFindTheUrlVariableNamesAndValues())
                 .And(x => x.ThenTheTemplatesVariablesAre(expectedTemplates))
                 .BDDfy();
        }

        [Fact]
        public void can_match_down_stream_url_with_downstream_template_with_place_holder_to_final_url_path()
        {
            var expectedTemplates = new List<PlaceholderNameAndValue>
            {
                new PlaceholderNameAndValue("{finalUrlPath}", "product/products/categories/"),
            };

            this.Given(x => x.GivenIHaveAUpstreamPath("api/product/products/categories/"))
                 .And(x => x.GivenIHaveAnUpstreamUrlTemplate("api/{finalUrlPath}/"))
                 .When(x => x.WhenIFindTheUrlVariableNamesAndValues())
                 .And(x => x.ThenTheTemplatesVariablesAre(expectedTemplates))
                 .BDDfy();
        }

        private void ThenTheTemplatesVariablesAre(List<PlaceholderNameAndValue> expectedResults)
        {
            foreach (var expectedResult in expectedResults)
            {
                var result = _result.Data.First(t => t.Name == expectedResult.Name);
                result.Value.ShouldBe(expectedResult.Value);
            }
        }

        private void GivenIHaveAUpstreamPath(string downstreamPath)
        {
            _downstreamUrlPath = downstreamPath;
        }

        private void GivenIHaveAnUpstreamUrlTemplate(string downstreamUrlTemplate)
        {
            _downstreamPathTemplate = downstreamUrlTemplate;
        }

        private void WhenIFindTheUrlVariableNamesAndValues()
        {
            _result = _finder.Find(_downstreamUrlPath, _query, _downstreamPathTemplate);
        }

        private void GivenIHaveAQuery(string query)
        {
            _query = query;
        }
    }
}

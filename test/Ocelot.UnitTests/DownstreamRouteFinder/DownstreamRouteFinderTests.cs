using Moq;
using Ocelot.Configuration;
using Ocelot.Configuration.Builder;
using Ocelot.DownstreamRouteFinder;
using Ocelot.DownstreamRouteFinder.Finder;
using Ocelot.DownstreamRouteFinder.UrlMatcher;
using Ocelot.Responses;
using Ocelot.Values;
using Shouldly;
using System.Collections.Generic;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.DownstreamRouteFinder
{
    using System;

    public class DownstreamRouteFinderTests
    {
        private readonly IDownstreamRouteProvider _downstreamRouteFinder;
        private readonly Mock<IUrlPathToUrlTemplateMatcher> _mockMatcher;
        private readonly Mock<IPlaceholderNameAndValueFinder> _finder;
        private string _upstreamUrlPath;
        private Response<Ocelot.DownstreamRouteFinder.DownstreamRouteHolder> _result;
        private List<Route> _routesConfig;
        private InternalConfiguration _config;
        private Response<UrlMatch> _match;
        private string _upstreamHttpMethod;
        private string _upstreamHost;
        private string _upstreamQuery;

        public DownstreamRouteFinderTests()
        {
            _mockMatcher = new Mock<IUrlPathToUrlTemplateMatcher>();
            _finder = new Mock<IPlaceholderNameAndValueFinder>();
            _downstreamRouteFinder = new Ocelot.DownstreamRouteFinder.Finder.DownstreamRouteFinder(_mockMatcher.Object, _finder.Object);
        }

        [Fact]
        public void should_return_highest_priority_when_first()
        {
            var serviceProviderConfig = new ServiceProviderConfigurationBuilder().Build();

            this.Given(x => x.GivenThereIsAnUpstreamUrlPath("someUpstreamPath"))
                .And(x => x.GivenTheTemplateVariableAndNameFinderReturns(
                            new OkResponse<List<PlaceholderNameAndValue>>(new List<PlaceholderNameAndValue>())))
                .And(x => x.GivenTheConfigurationIs(new List<Route>
                {
                    new RouteBuilder()
                        .WithDownstreamRoute(new DownstreamRouteBuilder()
                            .WithDownstreamPathTemplate("someDownstreamPath")
                            .WithUpstreamHttpMethod(new List<string> { "Post" })
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("test", 1, false, "someUpstreamPath"))
                            .Build())
                        .WithUpstreamHttpMethod(new List<string> { "Post" })
                        .WithUpstreamPathTemplate(new UpstreamPathTemplate("test", 1, false, "someUpstreamPath"))
                        .Build(),
                    new RouteBuilder()
                        .WithDownstreamRoute(new DownstreamRouteBuilder()
                            .WithDownstreamPathTemplate("someDownstreamPath")
                            .WithUpstreamHttpMethod(new List<string> { "Post" })
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("test", 0, false, "someUpstreamPath"))
                            .Build())
                        .WithUpstreamHttpMethod(new List<string> { "Post" })
                        .WithUpstreamPathTemplate(new UpstreamPathTemplate("test", 0, false, "someUpstreamPath"))
                        .Build()
                }, string.Empty, serviceProviderConfig))
                .And(x => x.GivenTheUrlMatcherReturns(new OkResponse<UrlMatch>(new UrlMatch(true))))
                .And(x => x.GivenTheUpstreamHttpMethodIs("Post"))
                .When(x => x.WhenICallTheFinder())
                .Then(x => x.ThenTheFollowingIsReturned(new DownstreamRouteHolder(new List<PlaceholderNameAndValue>(),
                        new RouteBuilder()
                            .WithDownstreamRoute(new DownstreamRouteBuilder()
                                .WithDownstreamPathTemplate("someDownstreamPath")
                                .WithUpstreamPathTemplate(new UpstreamPathTemplate("test", 1, false, "someUpstreamPath"))
                                .WithUpstreamHttpMethod(new List<string> { "Post" })
                                .Build())
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("test", 1, false, "someUpstreamPath"))
                            .WithUpstreamHttpMethod(new List<string> { "Post" })
                            .Build()
                        )))
                .BDDfy();
        }

        [Fact]
        public void should_return_highest_priority_when_lowest()
        {
            var serviceProviderConfig = new ServiceProviderConfigurationBuilder().Build();

            this.Given(x => x.GivenThereIsAnUpstreamUrlPath("someUpstreamPath"))
                .And(x => x.GivenTheTemplateVariableAndNameFinderReturns(
                            new OkResponse<List<PlaceholderNameAndValue>>(new List<PlaceholderNameAndValue>())))
                .And(x => x.GivenTheConfigurationIs(new List<Route>
                {
                    new RouteBuilder()
                        .WithDownstreamRoute(new DownstreamRouteBuilder()
                            .WithDownstreamPathTemplate("someDownstreamPath")
                            .WithUpstreamHttpMethod(new List<string> { "Post" })
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("test", 0, false, "someUpstreamPath"))
                            .Build())
                        .WithUpstreamHttpMethod(new List<string> { "Post" })
                        .WithUpstreamPathTemplate(new UpstreamPathTemplate("test", 0, false, "someUpstreamPath"))
                        .Build(),
                    new RouteBuilder()
                        .WithDownstreamRoute(new DownstreamRouteBuilder()
                            .WithDownstreamPathTemplate("someDownstreamPath")
                            .WithUpstreamHttpMethod(new List<string> { "Post" })
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("test", 1, false, "someUpstreamPath"))
                            .Build())
                        .WithUpstreamHttpMethod(new List<string> { "Post" })
                        .WithUpstreamPathTemplate(new UpstreamPathTemplate("test", 1, false, "someUpstreamPath"))
                        .Build()
                }, string.Empty, serviceProviderConfig))
                .And(x => x.GivenTheUrlMatcherReturns(new OkResponse<UrlMatch>(new UrlMatch(true))))
                .And(x => x.GivenTheUpstreamHttpMethodIs("Post"))
                .When(x => x.WhenICallTheFinder())
                .Then(x => x.ThenTheFollowingIsReturned(new DownstreamRouteHolder(new List<PlaceholderNameAndValue>(),
                        new RouteBuilder()
                            .WithDownstreamRoute(new DownstreamRouteBuilder()
                                .WithDownstreamPathTemplate("someDownstreamPath")
                                .WithUpstreamPathTemplate(new UpstreamPathTemplate("test", 1, false, "someUpstreamPath"))
                                .WithUpstreamHttpMethod(new List<string> { "Post" })
                                .Build())
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("test", 1, false, "someUpstreamPath"))
                            .WithUpstreamHttpMethod(new List<string> { "Post" })
                            .Build()
                        )))
                .BDDfy();
        }

        [Fact]
        public void should_return_route()
        {
            var serviceProviderConfig = new ServiceProviderConfigurationBuilder().Build();

            this.Given(x => x.GivenThereIsAnUpstreamUrlPath("matchInUrlMatcher/"))
                .And(x => x.GivenTheTemplateVariableAndNameFinderReturns(
                        new OkResponse<List<PlaceholderNameAndValue>>(
                            new List<PlaceholderNameAndValue>())))
                .And(x => x.GivenTheConfigurationIs(new List<Route>
                {
                    new RouteBuilder()
                        .WithDownstreamRoute(new DownstreamRouteBuilder()
                            .WithDownstreamPathTemplate("someDownstreamPath")
                            .WithUpstreamHttpMethod(new List<string> { "Get" })
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath"))
                            .Build())
                        .WithUpstreamHttpMethod(new List<string> { "Get" })
                        .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath"))
                        .Build()
                }, string.Empty, serviceProviderConfig
                ))
                .And(x => x.GivenTheUrlMatcherReturns(new OkResponse<UrlMatch>(new UrlMatch(true))))
                .And(x => x.GivenTheUpstreamHttpMethodIs("Get"))
                .When(x => x.WhenICallTheFinder())
                .Then(
                    x => x.ThenTheFollowingIsReturned(new DownstreamRouteHolder(
                            new List<PlaceholderNameAndValue>(),
                            new RouteBuilder()
                                .WithDownstreamRoute(new DownstreamRouteBuilder()
                                    .WithDownstreamPathTemplate("someDownstreamPath")
                                    .WithUpstreamHttpMethod(new List<string> { "Get" })
                                    .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath"))
                                    .Build())
                                .WithUpstreamHttpMethod(new List<string> { "Get" })
                                .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath"))
                                .Build()
                )))
                .And(x => x.ThenTheUrlMatcherIsCalledCorrectly())
                .BDDfy();
        }

        [Fact]
        public void should_not_append_slash_to_upstream_url_path()
        {
            var serviceProviderConfig = new ServiceProviderConfigurationBuilder().Build();

            this.Given(x => x.GivenThereIsAnUpstreamUrlPath("matchInUrlMatcher"))
                .And(x => x.GivenTheTemplateVariableAndNameFinderReturns(
                        new OkResponse<List<PlaceholderNameAndValue>>(
                            new List<PlaceholderNameAndValue>())))
                .And(x => x.GivenTheConfigurationIs(new List<Route>
                {
                    new RouteBuilder()
                        .WithDownstreamRoute(new DownstreamRouteBuilder()
                            .WithDownstreamPathTemplate("someDownstreamPath")
                            .WithUpstreamHttpMethod(new List<string> { "Get" })
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath"))
                            .Build())
                        .WithUpstreamHttpMethod(new List<string> { "Get" })
                        .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath"))
                        .Build()
                }, string.Empty, serviceProviderConfig
                ))
                .And(x => x.GivenTheUrlMatcherReturns(new OkResponse<UrlMatch>(new UrlMatch(true))))
                .And(x => x.GivenTheUpstreamHttpMethodIs("Get"))
                .When(x => x.WhenICallTheFinder())
                .Then(
                    x => x.ThenTheFollowingIsReturned(new DownstreamRouteHolder(
                            new List<PlaceholderNameAndValue>(),
                            new RouteBuilder()
                                .WithDownstreamRoute(new DownstreamRouteBuilder()
                                    .WithDownstreamPathTemplate("someDownstreamPath")
                                    .WithUpstreamHttpMethod(new List<string> { "Get" })
                                    .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath"))
                                    .Build())
                                .WithUpstreamHttpMethod(new List<string> { "Get" })
                                .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath"))
                                .Build()
                )))
                .And(x => x.ThenTheUrlMatcherIsCalledCorrectly("matchInUrlMatcher"))
                .BDDfy();
        }

        [Fact]
        public void should_return_route_if_upstream_path_and_upstream_template_are_the_same()
        {
            var serviceProviderConfig = new ServiceProviderConfigurationBuilder().Build();

            this.Given(x => x.GivenThereIsAnUpstreamUrlPath("someUpstreamPath"))
                .And(
                    x =>
                        x.GivenTheTemplateVariableAndNameFinderReturns(
                            new OkResponse<List<PlaceholderNameAndValue>>(new List<PlaceholderNameAndValue>())))
                .And(x => x.GivenTheConfigurationIs(new List<Route>
                {
                    new RouteBuilder()
                        .WithDownstreamRoute(new DownstreamRouteBuilder()
                            .WithDownstreamPathTemplate("someDownstreamPath")
                            .WithUpstreamHttpMethod(new List<string> { "Get" })
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath"))
                            .Build())
                        .WithUpstreamHttpMethod(new List<string> { "Get" })
                        .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath"))
                        .Build()
                }, string.Empty, serviceProviderConfig
                    ))
                .And(x => x.GivenTheUrlMatcherReturns(new OkResponse<UrlMatch>(new UrlMatch(true))))
                .And(x => x.GivenTheUpstreamHttpMethodIs("Get"))
                .When(x => x.WhenICallTheFinder())
                .Then(
                    x => x.ThenTheFollowingIsReturned(new DownstreamRouteHolder(new List<PlaceholderNameAndValue>(),
                        new RouteBuilder()
                            .WithDownstreamRoute(new DownstreamRouteBuilder()
                                .WithDownstreamPathTemplate("someDownstreamPath")
                                .WithUpstreamHttpMethod(new List<string> { "Get" })
                                .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath"))
                                .Build())
                            .WithUpstreamHttpMethod(new List<string> { "Get" })
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath"))
                            .Build()
                        )))
                .BDDfy();
        }

        [Fact]
        public void should_return_correct_route_for_http_verb()
        {
            var serviceProviderConfig = new ServiceProviderConfigurationBuilder().Build();

            this.Given(x => x.GivenThereIsAnUpstreamUrlPath("someUpstreamPath"))
                .And(
                    x =>
                        x.GivenTheTemplateVariableAndNameFinderReturns(
                            new OkResponse<List<PlaceholderNameAndValue>>(new List<PlaceholderNameAndValue>())))
                .And(x => x.GivenTheConfigurationIs(new List<Route>
                {
                    new RouteBuilder()
                        .WithDownstreamRoute(new DownstreamRouteBuilder()
                            .WithDownstreamPathTemplate("someDownstreamPath")
                            .WithUpstreamHttpMethod(new List<string> { "Get" })
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("", 1, false, "someUpstreamPath"))
                            .Build())
                        .WithUpstreamHttpMethod(new List<string> { "Get" })
                        .WithUpstreamPathTemplate(new UpstreamPathTemplate("", 1, false, "someUpstreamPath"))
                        .Build(),
                    new RouteBuilder()
                        .WithDownstreamRoute(new DownstreamRouteBuilder()
                            .WithDownstreamPathTemplate("someDownstreamPathForAPost")
                            .WithUpstreamHttpMethod(new List<string> { "Post" })
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("", 1, false, "someUpstreamPath"))
                            .Build())
                        .WithUpstreamHttpMethod(new List<string> { "Post" })
                        .WithUpstreamPathTemplate(new UpstreamPathTemplate("", 1, false, "someUpstreamPath"))
                        .Build()
                }, string.Empty, serviceProviderConfig
                    ))
                .And(x => x.GivenTheUrlMatcherReturns(new OkResponse<UrlMatch>(new UrlMatch(true))))
                .And(x => x.GivenTheUpstreamHttpMethodIs("Post"))
                .When(x => x.WhenICallTheFinder())
                .Then(
                    x => x.ThenTheFollowingIsReturned(new DownstreamRouteHolder(new List<PlaceholderNameAndValue>(),
                        new RouteBuilder()
                            .WithDownstreamRoute(new DownstreamRouteBuilder()
                                .WithDownstreamPathTemplate("someDownstreamPathForAPost")
                                .WithUpstreamHttpMethod(new List<string> { "Post" })
                                .WithUpstreamPathTemplate(new UpstreamPathTemplate("", 1, false, "someUpstreamPath"))
                                .Build())
                            .WithUpstreamHttpMethod(new List<string> { "Post" })
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("", 1, false, "someUpstreamPath"))
                            .Build()
                        )))
                .BDDfy();
        }

        [Fact]
        public void should_not_return_route()
        {
            var serviceProviderConfig = new ServiceProviderConfigurationBuilder().Build();

            this.Given(x => x.GivenThereIsAnUpstreamUrlPath("dontMatchPath/"))
                 .And(x => x.GivenTheConfigurationIs(new List<Route>
                     {
                        new RouteBuilder()
                        .WithDownstreamRoute(new DownstreamRouteBuilder()
                                .WithDownstreamPathTemplate("somPath")
                                .WithUpstreamHttpMethod(new List<string> { "Get" })
                                .WithUpstreamPathTemplate(new UpstreamPathTemplate("somePath", 1, false, "someUpstreamPath"))
                                .Build())
                        .WithUpstreamHttpMethod(new List<string> { "Get" })
                        .WithUpstreamPathTemplate(new UpstreamPathTemplate("somePath", 1, false, "someUpstreamPath"))
                        .Build(),
                     }, string.Empty, serviceProviderConfig
                 ))
                 .And(x => x.GivenTheUrlMatcherReturns(new OkResponse<UrlMatch>(new UrlMatch(false))))
                 .And(x => x.GivenTheUpstreamHttpMethodIs("Get"))
                 .When(x => x.WhenICallTheFinder())
                 .Then(
                     x => x.ThenAnErrorResponseIsReturned())
                 .And(x => x.ThenTheUrlMatcherIsCalledCorrectly())
                 .BDDfy();
        }

        [Fact]
        public void should_return_correct_route_for_http_verb_setting_multiple_upstream_http_method()
        {
            var serviceProviderConfig = new ServiceProviderConfigurationBuilder().Build();

            this.Given(x => x.GivenThereIsAnUpstreamUrlPath("someUpstreamPath"))
                .And(
                    x =>
                        x.GivenTheTemplateVariableAndNameFinderReturns(
                            new OkResponse<List<PlaceholderNameAndValue>>(new List<PlaceholderNameAndValue>())))
                .And(x => x.GivenTheConfigurationIs(new List<Route>
                {
                    new RouteBuilder()
                        .WithDownstreamRoute(new DownstreamRouteBuilder()
                            .WithDownstreamPathTemplate("someDownstreamPath")
                            .WithUpstreamHttpMethod(new List<string> { "Get", "Post" })
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("", 1, false, "someUpstreamPath"))
                            .Build())
                        .WithUpstreamHttpMethod(new List<string> { "Get", "Post" })
                        .WithUpstreamPathTemplate(new UpstreamPathTemplate("", 1, false, "someUpstreamPath"))
                        .Build()
                }, string.Empty, serviceProviderConfig
                    ))
                .And(x => x.GivenTheUrlMatcherReturns(new OkResponse<UrlMatch>(new UrlMatch(true))))
                .And(x => x.GivenTheUpstreamHttpMethodIs("Post"))
                .When(x => x.WhenICallTheFinder())
                .Then(
                    x => x.ThenTheFollowingIsReturned(new DownstreamRouteHolder(new List<PlaceholderNameAndValue>(),
                        new RouteBuilder()
                            .WithDownstreamRoute(new DownstreamRouteBuilder()
                                .WithDownstreamPathTemplate("someDownstreamPath")
                                .WithUpstreamHttpMethod(new List<string> { "Post" })
                                .WithUpstreamPathTemplate(new UpstreamPathTemplate("", 1, false, "someUpstreamPath"))
                                .Build())
                            .WithUpstreamHttpMethod(new List<string> { "Post" })
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("", 1, false, "someUpstreamPath"))
                            .Build()
                        )))
                .BDDfy();
        }

        [Fact]
        public void should_return_correct_route_for_http_verb_setting_all_upstream_http_method()
        {
            var serviceProviderConfig = new ServiceProviderConfigurationBuilder().Build();

            this.Given(x => x.GivenThereIsAnUpstreamUrlPath("someUpstreamPath"))
                .And(
                    x =>
                        x.GivenTheTemplateVariableAndNameFinderReturns(
                            new OkResponse<List<PlaceholderNameAndValue>>(new List<PlaceholderNameAndValue>())))
                .And(x => x.GivenTheConfigurationIs(new List<Route>
                {
                    new RouteBuilder()
                        .WithDownstreamRoute(new DownstreamRouteBuilder()
                            .WithDownstreamPathTemplate("someDownstreamPath")
                            .WithUpstreamHttpMethod(new List<string>())
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("", 1, false, "someUpstreamPath"))
                            .Build())
                        .WithUpstreamHttpMethod(new List<string>())
                        .WithUpstreamPathTemplate(new UpstreamPathTemplate("", 1, false, "someUpstreamPath"))
                        .Build()
                }, string.Empty, serviceProviderConfig
                    ))
                .And(x => x.GivenTheUrlMatcherReturns(new OkResponse<UrlMatch>(new UrlMatch(true))))
                .And(x => x.GivenTheUpstreamHttpMethodIs("Post"))
                .When(x => x.WhenICallTheFinder())
                .Then(
                    x => x.ThenTheFollowingIsReturned(new DownstreamRouteHolder(new List<PlaceholderNameAndValue>(),
                        new RouteBuilder()
                            .WithDownstreamRoute(new DownstreamRouteBuilder()
                                .WithDownstreamPathTemplate("someDownstreamPath")
                                .WithUpstreamHttpMethod(new List<string> { "Post" })
                                .WithUpstreamPathTemplate(new UpstreamPathTemplate("", 1, false, "someUpstreamPath"))
                                .Build())
                            .WithUpstreamHttpMethod(new List<string> { "Post" })
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("", 1, false, "someUpstreamPath"))
                            .Build()
                        )))
                .BDDfy();
        }

        [Fact]
        public void should_not_return_route_for_http_verb_not_setting_in_upstream_http_method()
        {
            var serviceProviderConfig = new ServiceProviderConfigurationBuilder().Build();

            this.Given(x => x.GivenThereIsAnUpstreamUrlPath("someUpstreamPath"))
                .And(
                    x =>
                        x.GivenTheTemplateVariableAndNameFinderReturns(
                            new OkResponse<List<PlaceholderNameAndValue>>(new List<PlaceholderNameAndValue>())))
                .And(x => x.GivenTheConfigurationIs(new List<Route>
                {
                    new RouteBuilder()
                        .WithDownstreamRoute(new DownstreamRouteBuilder()
                            .WithDownstreamPathTemplate("someDownstreamPath")
                            .WithUpstreamHttpMethod(new List<string> { "Get", "Patch", "Delete" })
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("", 1, false, "someUpstreamPath"))
                            .Build())
                        .WithUpstreamHttpMethod(new List<string> { "Get", "Patch", "Delete" })
                        .WithUpstreamPathTemplate(new UpstreamPathTemplate("", 1, false, "someUpstreamPath"))
                        .Build()
                }, string.Empty, serviceProviderConfig
                    ))
                .And(x => x.GivenTheUrlMatcherReturns(new OkResponse<UrlMatch>(new UrlMatch(true))))
                .And(x => x.GivenTheUpstreamHttpMethodIs("Post"))
                .When(x => x.WhenICallTheFinder())
                 .Then(x => x.ThenAnErrorResponseIsReturned())
                 .And(x => x.ThenTheUrlMatcherIsNotCalled())
                 .BDDfy();
        }

        [Fact]
        public void should_return_route_when_host_matches()
        {
            var serviceProviderConfig = new ServiceProviderConfigurationBuilder().Build();

            this.Given(x => x.GivenThereIsAnUpstreamUrlPath("matchInUrlMatcher/"))
                .And(x => GivenTheUpstreamHostIs("MATCH"))
                .And(x => x.GivenTheTemplateVariableAndNameFinderReturns(
                    new OkResponse<List<PlaceholderNameAndValue>>(
                        new List<PlaceholderNameAndValue>())))
                .And(x => x.GivenTheConfigurationIs(new List<Route>
                    {
                        new RouteBuilder()
                            .WithDownstreamRoute(new DownstreamRouteBuilder()
                                .WithDownstreamPathTemplate("someDownstreamPath")
                                .WithUpstreamHttpMethod(new List<string> { "Get" })
                                .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath"))
                                .Build())
                            .WithUpstreamHttpMethod(new List<string> { "Get" })
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath"))
                            .WithUpstreamHost("MATCH")
                            .Build()
                    }, string.Empty, serviceProviderConfig
                ))
                .And(x => x.GivenTheUrlMatcherReturns(new OkResponse<UrlMatch>(new UrlMatch(true))))
                .And(x => x.GivenTheUpstreamHttpMethodIs("Get"))
                .When(x => x.WhenICallTheFinder())
                .Then(
                    x => x.ThenTheFollowingIsReturned(new DownstreamRouteHolder(
                        new List<PlaceholderNameAndValue>(),
                        new RouteBuilder()
                            .WithDownstreamRoute(new DownstreamRouteBuilder()
                                .WithDownstreamPathTemplate("someDownstreamPath")
                                .WithUpstreamHttpMethod(new List<string> { "Get" })
                                .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath"))
                                .Build())
                            .WithUpstreamHttpMethod(new List<string> { "Get" })
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath"))
                            .Build()
                    )))
                .And(x => x.ThenTheUrlMatcherIsCalledCorrectly())
                .BDDfy();
        }

        [Fact]
        public void should_return_route_when_upstreamhost_is_null()
        {
            var serviceProviderConfig = new ServiceProviderConfigurationBuilder().Build();

            this.Given(x => x.GivenThereIsAnUpstreamUrlPath("matchInUrlMatcher/"))
                .And(x => GivenTheUpstreamHostIs("MATCH"))
                .And(x => x.GivenTheTemplateVariableAndNameFinderReturns(
                    new OkResponse<List<PlaceholderNameAndValue>>(
                        new List<PlaceholderNameAndValue>())))
                .And(x => x.GivenTheConfigurationIs(new List<Route>
                    {
                        new RouteBuilder()
                            .WithDownstreamRoute(new DownstreamRouteBuilder()
                                .WithDownstreamPathTemplate("someDownstreamPath")
                                .WithUpstreamHttpMethod(new List<string> { "Get" })
                                .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath"))
                                .Build())
                            .WithUpstreamHttpMethod(new List<string> { "Get" })
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath"))
                            .Build()
                    }, string.Empty, serviceProviderConfig
                ))
                .And(x => x.GivenTheUrlMatcherReturns(new OkResponse<UrlMatch>(new UrlMatch(true))))
                .And(x => x.GivenTheUpstreamHttpMethodIs("Get"))
                .When(x => x.WhenICallTheFinder())
                .Then(
                    x => x.ThenTheFollowingIsReturned(new DownstreamRouteHolder(
                        new List<PlaceholderNameAndValue>(),
                        new RouteBuilder()
                            .WithDownstreamRoute(new DownstreamRouteBuilder()
                                .WithDownstreamPathTemplate("someDownstreamPath")
                                .WithUpstreamHttpMethod(new List<string> { "Get" })
                                .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath"))
                                .Build())
                            .WithUpstreamHttpMethod(new List<string> { "Get" })
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath"))
                            .Build()
                    )))
                .And(x => x.ThenTheUrlMatcherIsCalledCorrectly())
                .BDDfy();
        }

        [Fact]
        public void should_not_return_route_when_host_doesnt_match()
        {
            var serviceProviderConfig = new ServiceProviderConfigurationBuilder().Build();

            this.Given(x => x.GivenThereIsAnUpstreamUrlPath("matchInUrlMatcher/"))
                .And(x => GivenTheUpstreamHostIs("DONTMATCH"))
                .And(x => x.GivenTheTemplateVariableAndNameFinderReturns(new OkResponse<List<PlaceholderNameAndValue>>(new List<PlaceholderNameAndValue>())))
                .And(x => x.GivenTheConfigurationIs(new List<Route>
                    {
                        new RouteBuilder()
                            .WithDownstreamRoute(new DownstreamRouteBuilder()
                                .WithDownstreamPathTemplate("someDownstreamPath")
                                .WithUpstreamHttpMethod(new List<string> { "Get" })
                                .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath"))
                                .Build())
                            .WithUpstreamHttpMethod(new List<string> { "Get" })
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath"))
                            .WithUpstreamHost("MATCH")
                            .Build(),
                        new RouteBuilder()
                            .WithDownstreamRoute(new DownstreamRouteBuilder()
                                .WithDownstreamPathTemplate("someDownstreamPath")
                                .WithUpstreamHttpMethod(new List<string> { }) // empty list of methods
                                .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath"))
                                .Build())
                            .WithUpstreamHttpMethod(new List<string> { }) // empty list of methods
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath"))
                            .WithUpstreamHost("MATCH")
                            .Build()
                    }, string.Empty, serviceProviderConfig
                ))
                .And(x => x.GivenTheUrlMatcherReturns(new OkResponse<UrlMatch>(new UrlMatch(true))))
                .And(x => x.GivenTheUpstreamHttpMethodIs("Get"))
                .When(x => x.WhenICallTheFinder())
                .Then(x => x.ThenAnErrorResponseIsReturned())
                .And(x => x.ThenTheUrlMatcherIsNotCalled())
                .BDDfy();
        }

        [Fact]
        public void should_not_return_route_when_host_doesnt_match_with_empty_upstream_http_method()
        {
            var serviceProviderConfig = new ServiceProviderConfigurationBuilder().Build();

            this.Given(x => x.GivenThereIsAnUpstreamUrlPath("matchInUrlMatcher/"))
                .And(x => GivenTheUpstreamHostIs("DONTMATCH"))
                .And(x => x.GivenTheTemplateVariableAndNameFinderReturns(new OkResponse<List<PlaceholderNameAndValue>>(new List<PlaceholderNameAndValue>())))
                .And(x => x.GivenTheConfigurationIs(new List<Route>
                    {
                        new RouteBuilder()
                            .WithDownstreamRoute(new DownstreamRouteBuilder()
                                .WithDownstreamPathTemplate("someDownstreamPath")
                                .WithUpstreamHttpMethod(new List<string>())
                                .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath"))
                                .Build())
                            .WithUpstreamHttpMethod(new List<string>())
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath"))
                            .WithUpstreamHost("MATCH")
                            .Build()
                    }, string.Empty, serviceProviderConfig
                ))
                .And(x => x.GivenTheUrlMatcherReturns(new OkResponse<UrlMatch>(new UrlMatch(true))))
                .And(x => x.GivenTheUpstreamHttpMethodIs("Get"))
                .When(x => x.WhenICallTheFinder())
                .Then(x => x.ThenAnErrorResponseIsReturned())
                .And(x => x.ThenTheUrlMatcherIsNotCalled())
                .BDDfy();
        }

        [Fact]
        public void should_return_route_when_host_does_match_with_empty_upstream_http_method()
        {
            var serviceProviderConfig = new ServiceProviderConfigurationBuilder().Build();

            this.Given(x => x.GivenThereIsAnUpstreamUrlPath("matchInUrlMatcher/"))
                .And(x => GivenTheUpstreamHostIs("MATCH"))
                .And(x => x.GivenTheTemplateVariableAndNameFinderReturns(new OkResponse<List<PlaceholderNameAndValue>>(new List<PlaceholderNameAndValue>())))
                .And(x => x.GivenTheConfigurationIs(new List<Route>
                    {
                        new RouteBuilder()
                            .WithDownstreamRoute(new DownstreamRouteBuilder()
                                .WithDownstreamPathTemplate("someDownstreamPath")
                                .WithUpstreamHttpMethod(new List<string>())
                                .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath"))
                                .Build())
                            .WithUpstreamHttpMethod(new List<string>())
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath"))
                            .WithUpstreamHost("MATCH")
                            .Build()
                    }, string.Empty, serviceProviderConfig
                ))
                .And(x => x.GivenTheUrlMatcherReturns(new OkResponse<UrlMatch>(new UrlMatch(true))))
                .And(x => x.GivenTheUpstreamHttpMethodIs("Get"))
                .When(x => x.WhenICallTheFinder())
                .And(x => x.ThenTheUrlMatcherIsCalledCorrectly(1, 0))
                .BDDfy();
        }

        [Fact]
        public void should_return_route_when_host_matches_but_null_host_on_same_path_first()
        {
            var serviceProviderConfig = new ServiceProviderConfigurationBuilder().Build();

            this.Given(x => x.GivenThereIsAnUpstreamUrlPath("matchInUrlMatcher/"))
                .And(x => GivenTheUpstreamHostIs("MATCH"))
                .And(x => x.GivenTheTemplateVariableAndNameFinderReturns(
                    new OkResponse<List<PlaceholderNameAndValue>>(
                        new List<PlaceholderNameAndValue>())))
                .And(x => x.GivenTheConfigurationIs(new List<Route>
                    {
                        new RouteBuilder()
                            .WithDownstreamRoute(new DownstreamRouteBuilder()
                                .WithDownstreamPathTemplate("THENULLPATH")
                                .WithUpstreamHttpMethod(new List<string> { "Get" })
                                .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath"))
                                .Build())
                            .WithUpstreamHttpMethod(new List<string> { "Get" })
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath"))
                            .Build(),
                        new RouteBuilder()
                            .WithDownstreamRoute(new DownstreamRouteBuilder()
                                .WithDownstreamPathTemplate("someDownstreamPath")
                                .WithUpstreamHttpMethod(new List<string> { "Get" })
                                .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath"))
                                .Build())
                            .WithUpstreamHttpMethod(new List<string> { "Get" })
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath"))
                            .WithUpstreamHost("MATCH")
                            .Build()
                    }, string.Empty, serviceProviderConfig
                ))
                .And(x => x.GivenTheUrlMatcherReturns(new OkResponse<UrlMatch>(new UrlMatch(true))))
                .And(x => x.GivenTheUpstreamHttpMethodIs("Get"))
                .When(x => x.WhenICallTheFinder())
                .Then(
                    x => x.ThenTheFollowingIsReturned(new DownstreamRouteHolder(
                        new List<PlaceholderNameAndValue>(),
                        new RouteBuilder()
                            .WithDownstreamRoute(new DownstreamRouteBuilder()
                                .WithDownstreamPathTemplate("someDownstreamPath")
                                .WithUpstreamHttpMethod(new List<string> { "Get" })
                                .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "test"))
                                .Build())
                            .WithUpstreamHttpMethod(new List<string> { "Get" })
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "test"))
                            .Build()
                    )))
                .And(x => x.ThenTheUrlMatcherIsCalledCorrectly(1, 0))
                .And(x => x.ThenTheUrlMatcherIsCalledCorrectly(1, 1))
                .BDDfy();
        }

        private void GivenTheUpstreamHostIs(string upstreamHost)
        {
            _upstreamHost = upstreamHost;
        }

        private void GivenTheTemplateVariableAndNameFinderReturns(Response<List<PlaceholderNameAndValue>> response)
        {
            _finder
                .Setup(x => x.Find(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(response);
        }

        private void GivenTheUpstreamHttpMethodIs(string upstreamHttpMethod)
        {
            _upstreamHttpMethod = upstreamHttpMethod;
        }

        private void ThenAnErrorResponseIsReturned()
        {
            _result.IsError.ShouldBeTrue();
        }

        private void ThenTheUrlMatcherIsCalledCorrectly()
        {
            _mockMatcher
                .Verify(x => x.Match(_upstreamUrlPath, _upstreamQuery, _routesConfig[0].UpstreamTemplatePattern), Times.Once);
        }

        private void ThenTheUrlMatcherIsCalledCorrectly(int times, int index = 0)
        {
            _mockMatcher
                .Verify(x => x.Match(_upstreamUrlPath, _upstreamQuery, _routesConfig[index].UpstreamTemplatePattern), Times.Exactly(times));
        }

        private void ThenTheUrlMatcherIsCalledCorrectly(string expectedUpstreamUrlPath)
        {
            _mockMatcher
                .Verify(x => x.Match(expectedUpstreamUrlPath, _upstreamQuery, _routesConfig[0].UpstreamTemplatePattern), Times.Once);
        }

        private void ThenTheUrlMatcherIsNotCalled()
        {
            _mockMatcher
                .Verify(x => x.Match(_upstreamUrlPath, _upstreamQuery, _routesConfig[0].UpstreamTemplatePattern), Times.Never);
        }

        private void GivenTheUrlMatcherReturns(Response<UrlMatch> match)
        {
            _match = match;
            _mockMatcher
                .Setup(x => x.Match(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<UpstreamPathTemplate>()))
                .Returns(_match);
        }

        private void GivenTheConfigurationIs(List<Route> routesConfig, string adminPath, ServiceProviderConfiguration serviceProviderConfig)
        {
            _routesConfig = routesConfig;
            _config = new InternalConfiguration(_routesConfig, adminPath, serviceProviderConfig, "", new LoadBalancerOptionsBuilder().Build(), "", new QoSOptionsBuilder().Build(), new HttpHandlerOptionsBuilder().Build(), new Version("1.1"));
        }

        private void GivenThereIsAnUpstreamUrlPath(string upstreamUrlPath)
        {
            _upstreamUrlPath = upstreamUrlPath;
        }

        private void WhenICallTheFinder()
        {
            _result = _downstreamRouteFinder.Get(_upstreamUrlPath, _upstreamQuery, _upstreamHttpMethod, _config, _upstreamHost);
        }

        private void ThenTheFollowingIsReturned(Ocelot.DownstreamRouteFinder.DownstreamRouteHolder expected)
        {
            _result.Data.Route.DownstreamRoute[0].DownstreamPathTemplate.Value.ShouldBe(expected.Route.DownstreamRoute[0].DownstreamPathTemplate.Value);
            _result.Data.Route.UpstreamTemplatePattern.Priority.ShouldBe(expected.Route.UpstreamTemplatePattern.Priority);

            for (int i = 0; i < _result.Data.TemplatePlaceholderNameAndValues.Count; i++)
            {
                _result.Data.TemplatePlaceholderNameAndValues[i].Name.ShouldBe(expected.TemplatePlaceholderNameAndValues[i].Name);
                _result.Data.TemplatePlaceholderNameAndValues[i].Value.ShouldBe(expected.TemplatePlaceholderNameAndValues[i].Value);
            }

            _result.IsError.ShouldBeFalse();
        }
    }
}

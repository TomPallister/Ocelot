using Ocelot.Configuration;
using Ocelot.Configuration.Builder;
using Ocelot.Configuration.Repository;
using Ocelot.Responses;
using Shouldly;
using System;
using System.Collections.Generic;
using Moq;
using Ocelot.Configuration.ChangeTracking;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.Configuration
{
    public class InMemoryConfigurationRepositoryTests
    {
        private readonly InMemoryInternalConfigurationRepository _repo;
        private IInternalConfiguration _config;
        private Response _result;
        private Response<IInternalConfiguration> _getResult;
        private readonly Mock<IOcelotConfigurationChangeTokenSource> _changeTokenSource;

        public InMemoryConfigurationRepositoryTests()
        {
            _changeTokenSource = new Mock<IOcelotConfigurationChangeTokenSource>(MockBehavior.Strict);
            _changeTokenSource.Setup(m => m.Activate());
            _repo = new InMemoryInternalConfigurationRepository(_changeTokenSource.Object);
        }

        [Fact]
        public void can_add_config()
        {
            this.Given(x => x.GivenTheConfigurationIs(new FakeConfig("initial", "adminath")))
                .When(x => x.WhenIAddOrReplaceTheConfig())
                .Then(x => x.ThenNoErrorsAreReturned())
                .And(x => AndTheChangeTokenIsActivated())
                .BDDfy();
        }

        [Fact]
        public void can_get_config()
        {
            this.Given(x => x.GivenThereIsASavedConfiguration())
                .When(x => x.WhenIGetTheConfiguration())
                .Then(x => x.ThenTheConfigurationIsReturned())
                .BDDfy();
        }

        private void ThenTheConfigurationIsReturned()
        {
            _getResult.Data.Routes[0].DownstreamRoute[0].DownstreamPathTemplate.Value.ShouldBe("initial");
        }

        private void WhenIGetTheConfiguration()
        {
            _getResult = _repo.Get();
        }

        private void GivenThereIsASavedConfiguration()
        {
            GivenTheConfigurationIs(new FakeConfig("initial", "adminath"));
            WhenIAddOrReplaceTheConfig();
        }

        private void GivenTheConfigurationIs(IInternalConfiguration config)
        {
            _config = config;
        }

        private void WhenIAddOrReplaceTheConfig()
        {
            _result = _repo.AddOrReplace(_config);
        }

        private void ThenNoErrorsAreReturned()
        {
            _result.IsError.ShouldBeFalse();
        }

        private void AndTheChangeTokenIsActivated()
        {
            _changeTokenSource.Verify(m => m.Activate(), Times.Once);
        }

        private class FakeConfig : IInternalConfiguration
        {
            private readonly string _downstreamTemplatePath;

            public FakeConfig(string downstreamTemplatePath, string administrationPath)
            {
                _downstreamTemplatePath = downstreamTemplatePath;
                AdministrationPath = administrationPath;
            }

            public List<Route> Routes
            {
                get
                {
                    var downstreamRoute = new DownstreamRouteBuilder()
                        .WithDownstreamPathTemplate(_downstreamTemplatePath)
                        .WithUpstreamHttpMethod(new List<string> { "Get" })
                        .Build();

                    return new List<Route>
                    {
                        new RouteBuilder()
                            .WithDownstreamRoute(downstreamRoute)
                            .WithUpstreamHttpMethod(new List<string> {"Get"})
                            .Build()
                    };
                }
            }

            public string AdministrationPath { get; }

            public ServiceProviderConfiguration ServiceProviderConfiguration => throw new NotImplementedException();

            public string RequestId { get; }
            public LoadBalancerOptions LoadBalancerOptions { get; }
            public string DownstreamScheme { get; }
            public QoSOptions QoSOptions { get; }
            public HttpHandlerOptions HttpHandlerOptions { get; }
            public Version DownstreamHttpVersion { get; }
        }
    }
}

using Ocelot.Configuration;
using Ocelot.Configuration.Builder;
using Ocelot.Configuration.Creator;
using Ocelot.Configuration.File;
using Shouldly;
using System;
using System.Collections.Generic;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.Configuration
{
    public class RateLimitOptionsCreatorTests
    {
        private FileRoute _fileRoute;
        private FileGlobalConfiguration _fileGlobalConfig;
        private bool _enabled;
        private RateLimitOptionsCreator _creator;
        private RateLimitOptions _result;

        public RateLimitOptionsCreatorTests()
        {
            _creator = new RateLimitOptionsCreator();
        }

        [Fact]
        public void should_create_rate_limit_options()
        {
            var fileRoute = new FileRoute
            {
                RateLimitOptions = new FileRateLimitRule
                {
                    ClientWhitelist = new List<string>(),
                    Period = "Period",
                    Limit = 1,
                    PeriodTimespan = 1,
                    EnableRateLimiting = true
                }
            };
            var fileGlobalConfig = new FileGlobalConfiguration
            {
                RateLimitOptions = new FileRateLimitOptions
                {
                    ClientIdHeader = "ClientIdHeader",
                    DisableRateLimitHeaders = true,
                    QuotaExceededMessage = "QuotaExceededMessage",
                    RateLimitCounterPrefix = "RateLimitCounterPrefix",
                    HttpStatusCode = 200
                }
            };
            var expected = new RateLimitOptionsBuilder()
                .WithClientIdHeader("ClientIdHeader")
                .WithClientWhiteList(() => fileRoute.RateLimitOptions.ClientWhitelist)
                .WithDisableRateLimitHeaders(true)
                .WithEnableRateLimiting(true)
                .WithHttpStatusCode(200)
                .WithQuotaExceededMessage("QuotaExceededMessage")
                .WithRateLimitCounterPrefix("RateLimitCounterPrefix")
                .WithRateLimitRule(new RateLimitRule(fileRoute.RateLimitOptions.Period,
                       fileRoute.RateLimitOptions.PeriodTimespan,
                       fileRoute.RateLimitOptions.Limit))
                .Build();

            this.Given(x => x.GivenTheFollowingFileRoute(fileRoute))
                .And(x => x.GivenTheFollowingFileGlobalConfig(fileGlobalConfig))
                .And(x => x.GivenRateLimitingIsEnabled())
                .When(x => x.WhenICreate())
                .Then(x => x.ThenTheFollowingIsReturned(expected))
                .BDDfy();
        }

        private void GivenTheFollowingFileRoute(FileRoute fileRoute)
        {
            _fileRoute = fileRoute;
        }

        private void GivenTheFollowingFileGlobalConfig(FileGlobalConfiguration fileGlobalConfig)
        {
            _fileGlobalConfig = fileGlobalConfig;
        }

        private void GivenRateLimitingIsEnabled()
        {
            _enabled = true;
        }

        private void WhenICreate()
        {
            _result = _creator.Create(_fileRoute.RateLimitOptions, _fileGlobalConfig);
        }

        private void ThenTheFollowingIsReturned(RateLimitOptions expected)
        {
            _result.ClientIdHeader.ShouldBe(expected.ClientIdHeader);
            _result.ClientWhitelist.ShouldBe(expected.ClientWhitelist);
            _result.DisableRateLimitHeaders.ShouldBe(expected.DisableRateLimitHeaders);
            _result.EnableRateLimiting.ShouldBe(expected.EnableRateLimiting);
            _result.HttpStatusCode.ShouldBe(expected.HttpStatusCode);
            _result.QuotaExceededMessage.ShouldBe(expected.QuotaExceededMessage);
            _result.RateLimitCounterPrefix.ShouldBe(expected.RateLimitCounterPrefix);
            _result.RateLimitRule.Limit.ShouldBe(expected.RateLimitRule.Limit);
            _result.RateLimitRule.Period.ShouldBe(expected.RateLimitRule.Period);
            TimeSpan.FromSeconds(_result.RateLimitRule.PeriodTimespan).Ticks.ShouldBe(TimeSpan.FromSeconds(expected.RateLimitRule.PeriodTimespan).Ticks);
        }
    }
}

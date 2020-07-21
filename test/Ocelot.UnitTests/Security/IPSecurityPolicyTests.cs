namespace Ocelot.UnitTests.Security
{
    using Microsoft.AspNetCore.Http;
    using Ocelot.Configuration;
    using Ocelot.Configuration.Builder;
    using Ocelot.Middleware;
    using Ocelot.Request.Middleware;
    using Ocelot.Responses;
    using Ocelot.Security.IPSecurity;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using TestStack.BDDfy;
    using Xunit;

    public class IPSecurityPolicyTests
    {
        private readonly DownstreamRouteBuilder _downstreamRouteBuilder;
        private readonly IPSecurityPolicy _ipSecurityPolicy;
        private Response response;
        private HttpContext _httpContext;

        public IPSecurityPolicyTests()
        {
            _httpContext = new DefaultHttpContext();
            _httpContext.Items.UpsertDownstreamRequest(new DownstreamRequest(new HttpRequestMessage(HttpMethod.Get, "http://test.com")));
            _httpContext.Connection.RemoteIpAddress = Dns.GetHostAddresses("192.168.1.1")[0];
            _downstreamRouteBuilder = new DownstreamRouteBuilder();
            _ipSecurityPolicy = new IPSecurityPolicy();
        }

        [Fact]
        public void should_No_blocked_Ip_and_allowed_Ip()
        {
            this.Given(x => x.GivenSetDownstreamRoute())
                .When(x => x.WhenTheSecurityPolicy())
                .Then(x => x.ThenSecurityPassing())
                .BDDfy();
        }

        [Fact]
        public void should_blockedIp_clientIp_block()
        {
            _httpContext.Connection.RemoteIpAddress = Dns.GetHostAddresses("192.168.1.1")[0];
            this.Given(x => x.GivenSetBlockedIP())
                .Given(x => x.GivenSetDownstreamRoute())
                .When(x => x.WhenTheSecurityPolicy())
                .Then(x => x.ThenNotSecurityPassing())
                .BDDfy();
        }

        [Fact]
        public void should_blockedIp_clientIp_Not_block()
        {
            _httpContext.Connection.RemoteIpAddress = Dns.GetHostAddresses("192.168.1.2")[0];
            this.Given(x => x.GivenSetBlockedIP())
                .Given(x => x.GivenSetDownstreamRoute())
                .When(x => x.WhenTheSecurityPolicy())
                .Then(x => x.ThenSecurityPassing())
                .BDDfy();
        }

        [Fact]
        public void should_allowedIp_clientIp_block()
        {
            _httpContext.Connection.RemoteIpAddress = Dns.GetHostAddresses("192.168.1.1")[0];
            this.Given(x => x.GivenSetAllowedIP())
                .Given(x => x.GivenSetDownstreamRoute())
                .When(x => x.WhenTheSecurityPolicy())
                .Then(x => x.ThenSecurityPassing())
                .BDDfy();
        }

        [Fact]
        public void should_allowedIp_clientIp_Not_block()
        {
            _httpContext.Connection.RemoteIpAddress = Dns.GetHostAddresses("192.168.1.2")[0];
            this.Given(x => x.GivenSetAllowedIP())
                .Given(x => x.GivenSetDownstreamRoute())
                .When(x => x.WhenTheSecurityPolicy())
                .Then(x => x.ThenNotSecurityPassing())
                .BDDfy();
        }

        private void GivenSetAllowedIP()
        {
            _downstreamRouteBuilder.WithSecurityOptions(new SecurityOptions(new List<string> { "192.168.1.1" }, new List<string>()));
        }

        private void GivenSetBlockedIP()
        {
            _downstreamRouteBuilder.WithSecurityOptions(new SecurityOptions(new List<string>(), new List<string> { "192.168.1.1" }));
        }

        private void GivenSetDownstreamRoute()
        {
            _httpContext.Items.UpsertDownstreamRoute(_downstreamRouteBuilder.Build());
        }

        private void WhenTheSecurityPolicy()
        {
            response = _ipSecurityPolicy.Security(_httpContext.Items.DownstreamRoute(), _httpContext).GetAwaiter().GetResult();
        }

        private void ThenSecurityPassing()
        {
            Assert.False(response.IsError);
        }

        private void ThenNotSecurityPassing()
        {
            Assert.True(response.IsError);
        }
    }
}


namespace Ocelot.UnitTests.Authorization
{
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Ocelot.Authorization;
    using Ocelot.Authorization.Middleware;
    using Ocelot.Configuration;
    using Ocelot.Configuration.Builder;
    using Ocelot.DownstreamRouteFinder.Middleware;
    using Ocelot.DownstreamRouteFinder.UrlMatcher;
    using Ocelot.Logging;
    using Ocelot.Middleware;
    using Ocelot.Responses;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using TestStack.BDDfy;
    using Xunit;

    public class AuthorizationMiddlewareTests
    {
        private readonly Mock<IClaimsAuthorizer> _authService;
        private readonly Mock<IScopesAuthorizer> _authScopesService;
        private Mock<IOcelotLoggerFactory> _loggerFactory;
        private Mock<IOcelotLogger> _logger;
        private readonly AuthorizationMiddleware _middleware;
        private RequestDelegate _next;
        private HttpContext _httpContext;
        private readonly Mock<IRolesAuthorizer> _authRolesService;

        public AuthorizationMiddlewareTests()
        {
            _httpContext = new DefaultHttpContext();
            _authService = new Mock<IClaimsAuthorizer>();
            _authScopesService = new Mock<IScopesAuthorizer>();
            _authRolesService = new Mock<IRolesAuthorizer>();
            _loggerFactory = new Mock<IOcelotLoggerFactory>();
            _logger = new Mock<IOcelotLogger>();
            _loggerFactory.Setup(x => x.CreateLogger<AuthorizationMiddleware>()).Returns(_logger.Object);
            _next = context => Task.CompletedTask;
            _middleware = new AuthorizationMiddleware(_next, _authService.Object, _authScopesService.Object,_authRolesService.Object, _loggerFactory.Object);
        }

        [Fact]
        public void should_call_authorization_service()
        {
            this.Given(x => x.GivenTheDownStreamRouteIs(new List<PlaceholderNameAndValue>(),
                new DownstreamRouteBuilder()
                    .WithUpstreamPathTemplate(new UpstreamPathTemplateBuilder().Build())
                    .WithIsAuthorized(true)
                    .WithUpstreamHttpMethod(new List<string> { "Get" })
                    .Build()))
                .And(x => x.GivenTheAuthServiceReturns(new OkResponse<bool>(true)))
                .When(x => x.WhenICallTheMiddleware())
                .Then(x => x.ThenTheAuthServiceIsCalledCorrectly())
                .BDDfy();
        }

        private void WhenICallTheMiddleware()
        {
            _middleware.Invoke(_httpContext).GetAwaiter().GetResult();
        }

        private void GivenTheDownStreamRouteIs(List<PlaceholderNameAndValue> templatePlaceholderNameAndValues, DownstreamRoute downstreamRoute)
        {
            _httpContext.Items.UpsertTemplatePlaceholderNameAndValues(templatePlaceholderNameAndValues);
            _httpContext.Items.UpsertDownstreamRoute(downstreamRoute);
        }

        private void GivenTheAuthServiceReturns(Response<bool> expected)
        {
            _authService
                .Setup(x => x.Authorize(
                           It.IsAny<ClaimsPrincipal>(),
                           It.IsAny<Dictionary<string, string>>(),
                           It.IsAny<List<PlaceholderNameAndValue>>()))
                .Returns(expected);
        }

        private void ThenTheAuthServiceIsCalledCorrectly()
        {
            _authService
                .Verify(x => x.Authorize(
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<Dictionary<string, string>>(),
                    It.IsAny<List<PlaceholderNameAndValue>>())
                        , Times.Once);
        }
    }
}

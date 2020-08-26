using Ocelot.Errors;
using Ocelot.Responder;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Net;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.Responder
{
    public class ErrorsToHttpStatusCodeMapperTests
    {
        private readonly IErrorsToHttpStatusCodeMapper _codeMapper;
        private int _result;
        private List<Error> _errors;

        public ErrorsToHttpStatusCodeMapperTests()
        {
            _codeMapper = new ErrorsToHttpStatusCodeMapper();
        }

        [Theory]
        [InlineData(OcelotErrorCode.UnauthenticatedError)]
        public void should_return_unauthorized(OcelotErrorCode errorCode)
        {
            ShouldMapErrorToStatusCode(errorCode, HttpStatusCode.Unauthorized);
        }

        [Theory]
        [InlineData(OcelotErrorCode.CannotFindClaimError)]
        [InlineData(OcelotErrorCode.ClaimValueNotAuthorisedError)]
        [InlineData(OcelotErrorCode.ScopeNotAuthorisedError)]
        [InlineData(OcelotErrorCode.UnauthorizedError)]
        [InlineData(OcelotErrorCode.UserDoesNotHaveClaimError)]
        public void should_return_forbidden(OcelotErrorCode errorCode)
        {
            ShouldMapErrorToStatusCode(errorCode, HttpStatusCode.Forbidden);
        }

        [Theory]
        [InlineData(OcelotErrorCode.RequestTimedOutError)]
        public void should_return_service_unavailable(OcelotErrorCode errorCode)
        {
            ShouldMapErrorToStatusCode(errorCode, HttpStatusCode.ServiceUnavailable);
        }

        [Theory]
        [InlineData(OcelotErrorCode.UnableToCompleteRequestError)]
        [InlineData(OcelotErrorCode.CouldNotFindLoadBalancerCreator)]
        [InlineData(OcelotErrorCode.ErrorInvokingLoadBalancerCreator)]
        public void should_return_internal_server_error(OcelotErrorCode errorCode)
        {
            ShouldMapErrorToStatusCode(errorCode, HttpStatusCode.InternalServerError);
        }

        [Theory]
        [InlineData(OcelotErrorCode.ConnectionToDownstreamServiceError)]
        public void should_return_bad_gateway_error(OcelotErrorCode errorCode)
        {
            ShouldMapErrorToStatusCode(errorCode, HttpStatusCode.BadGateway);
        }

        [Theory]
        [InlineData(OcelotErrorCode.CannotAddDataError)]
        [InlineData(OcelotErrorCode.CannotFindDataError)]
        [InlineData(OcelotErrorCode.DownstreamHostNullOrEmptyError)]
        [InlineData(OcelotErrorCode.DownstreamPathNullOrEmptyError)]
        [InlineData(OcelotErrorCode.DownstreampathTemplateAlreadyUsedError)]
        [InlineData(OcelotErrorCode.DownstreamPathTemplateContainsSchemeError)]
        [InlineData(OcelotErrorCode.DownstreamSchemeNullOrEmptyError)]
        [InlineData(OcelotErrorCode.FileValidationFailedError)]
        [InlineData(OcelotErrorCode.InstructionNotForClaimsError)]
        [InlineData(OcelotErrorCode.NoInstructionsError)]
        [InlineData(OcelotErrorCode.ParsingConfigurationHeaderError)]
        [InlineData(OcelotErrorCode.RateLimitOptionsError)]
        [InlineData(OcelotErrorCode.ServicesAreEmptyError)]
        [InlineData(OcelotErrorCode.ServicesAreNullError)]
        [InlineData(OcelotErrorCode.UnableToCreateAuthenticationHandlerError)]
        [InlineData(OcelotErrorCode.UnableToFindDownstreamRouteError)]
        [InlineData(OcelotErrorCode.UnableToFindLoadBalancerError)]
        [InlineData(OcelotErrorCode.UnableToFindServiceDiscoveryProviderError)]
        [InlineData(OcelotErrorCode.UnableToFindQoSProviderError)]
        [InlineData(OcelotErrorCode.UnknownError)]
        [InlineData(OcelotErrorCode.UnmappableRequestError)]
        [InlineData(OcelotErrorCode.UnsupportedAuthenticationProviderError)]
        public void should_return_not_found(OcelotErrorCode errorCode)
        {
            ShouldMapErrorToStatusCode(errorCode, HttpStatusCode.NotFound);
        }

        [Fact]
        public void AuthenticationErrorsHaveHighestPriority()
        {
            var errors = new List<OcelotErrorCode>
            {
                OcelotErrorCode.CannotAddDataError,
                OcelotErrorCode.CannotFindClaimError,
                OcelotErrorCode.UnauthenticatedError,
                OcelotErrorCode.RequestTimedOutError,
            };

            ShouldMapErrorsToStatusCode(errors, HttpStatusCode.Unauthorized);
        }

        [Fact]
        public void AuthorisationErrorsHaveSecondHighestPriority()
        {
            var errors = new List<OcelotErrorCode>
            {
                OcelotErrorCode.CannotAddDataError,
                OcelotErrorCode.CannotFindClaimError,
                OcelotErrorCode.RequestTimedOutError
            };

            ShouldMapErrorsToStatusCode(errors, HttpStatusCode.Forbidden);
        }

        [Fact]
        public void ServiceUnavailableErrorsHaveThirdHighestPriority()
        {
            var errors = new List<OcelotErrorCode>
            {
                OcelotErrorCode.CannotAddDataError,
                OcelotErrorCode.RequestTimedOutError,
            };

            ShouldMapErrorsToStatusCode(errors, HttpStatusCode.ServiceUnavailable);
        }

        [Fact]
        public void check_we_have_considered_all_errors_in_these_tests()
        {
            // If this test fails then it's because the number of error codes has changed.
            // You should make the appropriate changes to the test cases here to ensure
            // they cover all the error codes, and then modify this assertion.
            Enum.GetNames(typeof(OcelotErrorCode)).Length.ShouldBe(41, "Looks like the number of error codes has changed. Do you need to modify ErrorsToHttpStatusCodeMapper?");
        }

        private void ShouldMapErrorToStatusCode(OcelotErrorCode errorCode, HttpStatusCode expectedHttpStatusCode)
        {
            ShouldMapErrorsToStatusCode(new List<OcelotErrorCode> { errorCode }, expectedHttpStatusCode);
        }

        private void ShouldMapErrorsToStatusCode(List<OcelotErrorCode> errorCodes, HttpStatusCode expectedHttpStatusCode)
        {
            var errors = new List<Error>();

            foreach (var errorCode in errorCodes)
            {
                errors.Add(new AnyError(errorCode));
            }

            this.Given(x => x.GivenThereAreErrors(errors))
               .When(x => x.WhenIGetErrorStatusCode())
               .Then(x => x.ThenTheResponseIsStatusCodeIs(expectedHttpStatusCode))
               .BDDfy();
        }

        private void GivenThereAreErrors(List<Error> errors)
        {
            _errors = errors;
        }

        private void WhenIGetErrorStatusCode()
        {
            _result = _codeMapper.Map(_errors);
        }

        private void ThenTheResponseIsStatusCodeIs(int expectedCode)
        {
            _result.ShouldBe(expectedCode);
        }

        private void ThenTheResponseIsStatusCodeIs(HttpStatusCode expectedCode)
        {
            _result.ShouldBe((int)expectedCode);
        }
    }
}

using FluentValidation.Results;
using Ocelot.Configuration.File;
using Ocelot.Configuration.Validator;
using Shouldly;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.Configuration.Validation
{
    public class HostAndPortValidatorTests
    {
        private HostAndPortValidator _validator;
        private ValidationResult _result;
        private FileHostAndPort _hostAndPort;

        public HostAndPortValidatorTests()
        {
            _validator = new HostAndPortValidator();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void should_be_invalid_because_host_empty(string host)
        {
            var fileHostAndPort = new FileHostAndPort
            {
                Host = host
            };

            this.Given(_ => GivenThe(fileHostAndPort))
               .When(_ => WhenIValidate())
               .Then(_ => ThenTheResultIsInValid())
               .And(_ => ThenTheErorrIs())
               .BDDfy();
        }

        [Fact]
        public void should_be_valid_because_host_set()
        {
            var fileHostAndPort = new FileHostAndPort
            {
                Host = "test"
            };

            this.Given(_ => GivenThe(fileHostAndPort))
                .When(_ => WhenIValidate())
                .Then(_ => ThenTheResultIsValid())
                .BDDfy();
        }

        private void GivenThe(FileHostAndPort hostAndPort)
        {
            _hostAndPort = hostAndPort;
        }

        private void WhenIValidate()
        {
            _result = _validator.Validate(_hostAndPort);
        }

        private void ThenTheResultIsValid()
        {
            _result.IsValid.ShouldBeTrue();
        }

        private void ThenTheErorrIs()
        {
            _result.Errors[0].ErrorMessage.ShouldBe("When not using service discovery Host must be set on DownstreamHostAndPorts if you are not using Route.Host or Ocelot cannot find your service!");
        }

        private void ThenTheResultIsInValid()
        {
            _result.IsValid.ShouldBeFalse();
        }
    }
}

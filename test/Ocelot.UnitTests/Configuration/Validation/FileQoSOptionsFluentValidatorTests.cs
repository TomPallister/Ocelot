using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Ocelot.Configuration.File;
using Ocelot.Configuration.Validator;
using Ocelot.Requester;
using Shouldly;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.Configuration.Validation
{
    public class FileQoSOptionsFluentValidatorTests
    {
        private FileQoSOptionsFluentValidator _validator;
        private ServiceCollection _services;
        private ValidationResult _result;
        private FileQoSOptions _qosOptions;

        public FileQoSOptionsFluentValidatorTests()
        {
            _services = new ServiceCollection();
            var provider = _services.BuildServiceProvider();
            _validator = new FileQoSOptionsFluentValidator(provider);
        }

        [Fact]
        public void should_be_valid_as_nothing_set()
        {
            this.Given(_ => GivenThe(new FileQoSOptions()))
                .When(_ => WhenIValidate())
                .Then(_ => ThenTheResultIsValid())
                .BDDfy();
        }

        [Fact]
        public void should_be_valid_as_qos_delegate_set()
        {
            var qosOptions = new FileQoSOptions
            {
                TimeoutValue = 1,
                ExceptionsAllowedBeforeBreaking = 1
            };

            this.Given(_ => GivenThe(qosOptions))
                .And(_ => GivenAQosDelegate())
                .When(_ => WhenIValidate())
                .Then(_ => ThenTheResultIsValid())
                .BDDfy();
        }

        [Fact]
        public void should_be_invalid_as_no_qos_delegate()
        {
            var qosOptions = new FileQoSOptions
            {
                TimeoutValue = 1,
                ExceptionsAllowedBeforeBreaking = 1
            };

            this.Given(_ => GivenThe(qosOptions))
                .When(_ => WhenIValidate())
                .Then(_ => ThenTheResultIsInValid())
                .And(_ => ThenTheErrorIs())
                .BDDfy();
        }

        private void ThenTheErrorIs()
        {
            _result.Errors[0].ErrorMessage.ShouldBe("Unable to start Ocelot because either a Route or GlobalConfiguration are using QoSOptions but no QosDelegatingHandlerDelegate has been registered in dependency injection container. Are you missing a package like Ocelot.Provider.Polly and services.AddPolly()?");
        }

        private void ThenTheResultIsInValid()
        {
            _result.IsValid.ShouldBeFalse();
        }

        private void GivenAQosDelegate()
        {
            QosDelegatingHandlerDelegate fake = (a, b) =>
            {
                return null;
            };
            _services.AddSingleton<QosDelegatingHandlerDelegate>(fake);
            var provider = _services.BuildServiceProvider();
            _validator = new FileQoSOptionsFluentValidator(provider);
        }

        private void GivenThe(FileQoSOptions qosOptions)
        {
            _qosOptions = qosOptions;
        }

        private void WhenIValidate()
        {
            _result = _validator.Validate(_qosOptions);
        }

        private void ThenTheResultIsValid()
        {
            _result.IsValid.ShouldBeTrue();
        }
    }
}

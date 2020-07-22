namespace Ocelot.Configuration.Validator
{
    using File;
    using FluentValidation;

    public class FileGlobalConfigurationFluentValidator : AbstractValidator<FileGlobalConfiguration>
    {
        public FileGlobalConfigurationFluentValidator(FileQoSOptionsFluentValidator fileQoSOptionsFluentValidator,
            FileAuthenticationOptionsValidator fileAuthenticationOptionsValidator)
        {
            RuleFor(configuration => configuration.QoSOptions)
                .SetValidator(fileQoSOptionsFluentValidator);

            RuleFor(configuration => configuration.AuthenticationOptions)
                .SetValidator(fileAuthenticationOptionsValidator);
        }
    }
}

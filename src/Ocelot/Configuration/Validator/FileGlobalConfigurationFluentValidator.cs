namespace Ocelot.Configuration.Validator
{
    using File;
    using FluentValidation;

    public class FileGlobalConfigurationFluentValidator : AbstractValidator<FileGlobalConfiguration>
    {
        public FileGlobalConfigurationFluentValidator(FileQoSOptionsFluentValidator fileQoSOptionsFluentValidator)
        {
            RuleFor(configuration => configuration.QoSOptions)
                .SetValidator(fileQoSOptionsFluentValidator);
        }
    }
}

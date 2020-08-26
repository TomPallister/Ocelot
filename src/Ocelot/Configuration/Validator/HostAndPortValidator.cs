namespace Ocelot.Configuration.Validator
{
    using FluentValidation;
    using Ocelot.Configuration.File;

    public class HostAndPortValidator : AbstractValidator<FileHostAndPort>
    {
        public HostAndPortValidator()
        {
            RuleFor(r => r.Host)
                .NotEmpty()
                .WithMessage("When not using service discovery Host must be set on DownstreamHostAndPorts if you are not using Route.Host or Ocelot cannot find your service!");
        }
    }
}

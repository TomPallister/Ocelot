using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Ocelot.Configuration.File;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ocelot.Configuration.Validator
{
    public class FileAuthenticationOptionsValidator : AbstractValidator<FileAuthenticationOptions>
    {
        private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;

        public FileAuthenticationOptionsValidator(IAuthenticationSchemeProvider authenticationSchemeProvider)
        {
            _authenticationSchemeProvider = authenticationSchemeProvider;

            RuleFor(authOptions => authOptions.AuthenticationProviderKey)
                .MustAsync(IsSupportedAuthenticationProviders)
                .WithMessage("{PropertyName}: {PropertyValue} is unsupported authentication provider");
        }

        private async Task<bool> IsSupportedAuthenticationProviders(string authenticationProviderKey, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(authenticationProviderKey))
            {
                return true;
            }

            var schemes = await _authenticationSchemeProvider.GetAllSchemesAsync();

            var supportedSchemes = schemes.Select(scheme => scheme.Name).ToList();

            return supportedSchemes.Contains(authenticationProviderKey);
        }
    }
}

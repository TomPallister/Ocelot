using Ocelot.Configuration.File;

namespace Ocelot.Configuration.Creator
{
    public class AuthenticationOptionsCreator : IAuthenticationOptionsCreator
    {
        public AuthenticationOptions Create(FileRoute route)
        {
            return new AuthenticationOptions(route.AuthenticationOptions.AllowedScopes, route.AuthenticationOptions.AuthenticationProviderKey);
        }
    }
}

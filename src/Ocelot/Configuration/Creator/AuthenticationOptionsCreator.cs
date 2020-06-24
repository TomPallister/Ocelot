using Ocelot.Configuration.File;
using System.Linq;

namespace Ocelot.Configuration.Creator
{
    public class AuthenticationOptionsCreator : IAuthenticationOptionsCreator
    {
        public AuthenticationOptions Create(FileAuthenticationOptions routeAuthOptions, 
                                            FileAuthenticationOptions globalConfAuthOptions)
        {
            var routeAuthOptionsEmpty = string.IsNullOrEmpty(routeAuthOptions.AuthenticationProviderKey)
                && !routeAuthOptions.AllowedScopes.Any();
            
            var resultAuthOptions = routeAuthOptionsEmpty ? globalConfAuthOptions : routeAuthOptions;

            // Important! if you add a property to FileAuthenticationOptions, you must add checking its value in routeAuthOptionsEmpty variable (above)
            return new AuthenticationOptions(resultAuthOptions.AllowedScopes, resultAuthOptions.AuthenticationProviderKey);
        }
    }
}

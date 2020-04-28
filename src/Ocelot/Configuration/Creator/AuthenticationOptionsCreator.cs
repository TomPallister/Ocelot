using Ocelot.Configuration.File;
using System.Linq;

namespace Ocelot.Configuration.Creator
{
    public class AuthenticationOptionsCreator : IAuthenticationOptionsCreator
    {
        public AuthenticationOptions Create(FileAuthenticationOptions reRouteAuthOptions, 
                                            FileAuthenticationOptions globalConfAuthOptions)
        {
            var reRouteAuthOptionsEmpty = string.IsNullOrEmpty(reRouteAuthOptions.AuthenticationProviderKey)
                && !reRouteAuthOptions.AllowedScopes.Any();
            
            var resultAuthOptions = reRouteAuthOptionsEmpty ? globalConfAuthOptions : reRouteAuthOptions;

            // Important! if you add a property to FileAuthenticationOptions, you must add checking its value in reRouteAuthOptionsEmpty variable (above)
            return new AuthenticationOptions(resultAuthOptions.AllowedScopes, resultAuthOptions.AuthenticationProviderKey);
        }
    }
}

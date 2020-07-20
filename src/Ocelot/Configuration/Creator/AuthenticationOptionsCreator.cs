using Ocelot.Configuration.File;
using System.Linq;

namespace Ocelot.Configuration.Creator
{
    public class AuthenticationOptionsCreator : IAuthenticationOptionsCreator
    {
        public AuthenticationOptions Create(FileAuthenticationOptions routeAuthOptions, 
                                            FileAuthenticationOptions globalConfAuthOptions)
        {
            var routeAuthOptionsEmpty = string.IsNullOrEmpty(routeAuthOptions.AuthenticationProviderKey);
            
            var resultAuthOptions = routeAuthOptionsEmpty ? globalConfAuthOptions : routeAuthOptions;
            
            return new AuthenticationOptions(resultAuthOptions.AllowedScopes, resultAuthOptions.AuthenticationProviderKey);
        }
    }
}

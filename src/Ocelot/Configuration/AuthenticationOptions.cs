using System.Collections.Generic;

namespace Ocelot.Configuration
{
    public class AuthenticationOptions
    {
        public AuthenticationOptions(List<string> allowedScopes, List<string> requiredRole, string authenticationProviderKey, string scopeKey, string roleKey, string policyName)
        {
            PolicyName = policyName;
            AllowedScopes = allowedScopes;
            RequiredRole = requiredRole;
            AuthenticationProviderKey = authenticationProviderKey;
            ScopeKey = scopeKey;
            RoleKey = roleKey;
        }

        public List<string> AllowedScopes { get; private set; }
        public string AuthenticationProviderKey { get; private set; }
        public List<string> RequiredRole { get; private set; }
        public string ScopeKey { get; private set; }
        public string RoleKey { get; private set; }
        public string PolicyName { get; private set; }
    }
}

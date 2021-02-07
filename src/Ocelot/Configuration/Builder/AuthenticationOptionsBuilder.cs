using System.Collections.Generic;

namespace Ocelot.Configuration.Builder
{
    public class AuthenticationOptionsBuilder
    {
        private List<string> _allowedScopes = new List<string>();
        private List<string> _requiredRole = new List<string>();
        private string _authenticationProviderKey;
        private string _roleKey;
        private string _scopeKey;
        private string _policyName;


        public AuthenticationOptionsBuilder WithAllowedScopes(List<string> allowedScopes)
        {
            _allowedScopes = allowedScopes;
            return this;
        }

        public AuthenticationOptionsBuilder WithRequiredRole(List<string> requiredRole)
        {
            _requiredRole = requiredRole;
            return this;
        }

        public AuthenticationOptionsBuilder WithAuthenticationProviderKey(string authenticationProviderKey)
        {
            _authenticationProviderKey = authenticationProviderKey;
            return this;
        }

        public AuthenticationOptionsBuilder WithRoleKey(string roleKey)
        {
            _roleKey = roleKey;
            return this;
        }

        public AuthenticationOptionsBuilder WithScopeKey(string scopeKey)
        {
            _scopeKey = scopeKey;
            return this;
        }
        public AuthenticationOptionsBuilder WithPolicyName(string policyName)
        {
            _policyName = policyName;
            return this;
        }
        public AuthenticationOptions Build()
        {
            return new AuthenticationOptions(_allowedScopes, _requiredRole, _authenticationProviderKey, _scopeKey, _roleKey, _policyName);
        }
    }
}

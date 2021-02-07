using Ocelot.Infrastructure.Extensions;
using System.Collections.Generic;
using System.Text;

namespace Ocelot.Configuration.File
{
    public class FileAuthenticationOptions
    {
        public FileAuthenticationOptions()
        {
            AllowedScopes = new List<string>();
            RequiredRole = new List<string>();
        }

        public string AuthenticationProviderKey { get; set; }
        public List<string> AllowedScopes { get; set; }
        public List<string> RequiredRole { get; set; }
        public string ScopeKey { get; set; }
        public string RoleKey { get; set; }
        public string PolicyName { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"{nameof(AuthenticationProviderKey)}:{AuthenticationProviderKey},{nameof(AllowedScopes)}:[");
            sb.AppendJoin(',', AllowedScopes);
            sb.Append("]");
            sb.Append($",{nameof(RequiredRole)}:[");
            sb.AppendJoin(',', RequiredRole);
            sb.Append("]");
            sb.Append($",{nameof(ScopeKey)}:[");
            sb.AppendJoin(',', ScopeKey);
            sb.Append("]");
            sb.Append($",{nameof(RoleKey)}:[");
            sb.AppendJoin(',', RoleKey);
            sb.Append("]");
            sb.Append($",{nameof(PolicyName)}:[");
            sb.AppendJoin(',', PolicyName);
            sb.Append("]");
            return sb.ToString();
        }
    }
}

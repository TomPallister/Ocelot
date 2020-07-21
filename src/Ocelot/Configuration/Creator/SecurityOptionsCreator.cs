using Ocelot.Configuration.File;

namespace Ocelot.Configuration.Creator
{
    public class SecurityOptionsCreator : ISecurityOptionsCreator
    {
        public SecurityOptions Create(FileSecurityOptions securityOptions)
        {
            return new SecurityOptions(securityOptions.IPAllowedList, securityOptions.IPBlockedList);
        }
    }
}

using System.Collections.Generic;

namespace Ocelot.Configuration
{
    public class SecurityOptions
    {
        public SecurityOptions(List<string> allowedList, List<string> blockedList)
        {
            this.IPAllowedList = allowedList;
            this.IPBlockedList = blockedList;
        }

        public List<string> IPAllowedList { get; private set; }

        public List<string> IPBlockedList { get; private set; }
    }
}

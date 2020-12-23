using System.Collections.Generic;
using System.Linq;
using NetTools;

namespace Ocelot.Configuration
{
    public class SecurityOptions
    {
        public SecurityOptions(List<string> allowedList, List<string> blockedList, bool excludeAllowedFromBlocked)
        {
            this.IPAllowedList = new List<string>();
            this.IPBlockedList = new List<string>();
            this.ExceludeAllowedFromBlocked = excludeAllowedFromBlocked;

            foreach (var allowed in allowedList)
            {
                if (IPAddressRange.TryParse(allowed, out var allowedIpAddressRange))
                {
                    var allowedIps = allowedIpAddressRange.AsEnumerable().Select(x => x.ToString());

                    this.IPAllowedList.AddRange(allowedIps);
                }
            }

            foreach (var blocked in blockedList)
            {
                if (IPAddressRange.TryParse(blocked, out var blockedIpAddressRange))
                {
                    var blockedIps = blockedIpAddressRange.AsEnumerable().Select(x => x.ToString());

                    this.IPBlockedList.AddRange(blockedIps);
                }
            }

            if (this.ExceludeAllowedFromBlocked)
            {
                this.IPBlockedList = this.IPBlockedList.Except(this.IPAllowedList).ToList();
            }
        }

        public List<string> IPAllowedList { get; private set; }

        public List<string> IPBlockedList { get; private set; }

        public bool ExceludeAllowedFromBlocked { get; private set; }
    }
}

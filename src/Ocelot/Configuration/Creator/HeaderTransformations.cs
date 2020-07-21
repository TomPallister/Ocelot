using System.Collections.Generic;

namespace Ocelot.Configuration.Creator
{
    public class HeaderTransformations
    {
        public HeaderTransformations(
            List<HeaderFindAndReplace> upstream,
            List<HeaderFindAndReplace> downstream,
            List<AddHeader> addHeaderToDownstream,
            List<AddHeader> addHeaderToUpstream)
        {
            AddHeadersToDownstream = addHeaderToDownstream;
            AddHeadersToUpstream = addHeaderToUpstream;
            Upstream = upstream;
            Downstream = downstream;
        }

        public List<HeaderFindAndReplace> Upstream { get; }

        public List<HeaderFindAndReplace> Downstream { get; }

        public List<AddHeader> AddHeadersToDownstream { get; }
        public List<AddHeader> AddHeadersToUpstream { get; }
    }
}

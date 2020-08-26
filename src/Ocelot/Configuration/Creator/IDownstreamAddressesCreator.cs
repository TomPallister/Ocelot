using Ocelot.Configuration.File;
using System.Collections.Generic;

namespace Ocelot.Configuration.Creator
{
    public interface IDownstreamAddressesCreator
    {
        List<DownstreamHostAndPort> Create(FileRoute route);
    }
}

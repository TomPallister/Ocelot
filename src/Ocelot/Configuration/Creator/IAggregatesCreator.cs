using Ocelot.Configuration.File;
using System.Collections.Generic;

namespace Ocelot.Configuration.Creator
{
    public interface IAggregatesCreator
    {
        List<Route> Create(FileConfiguration fileConfiguration, List<Route> routes);
    }
}

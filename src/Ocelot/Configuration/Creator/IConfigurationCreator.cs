using Ocelot.Configuration.File;
using System.Collections.Generic;

namespace Ocelot.Configuration.Creator
{
    public interface IConfigurationCreator
    {
        InternalConfiguration Create(FileConfiguration fileConfiguration, List<Route> routes);
    }
}

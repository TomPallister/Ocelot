using Ocelot.Configuration.File;
using Ocelot.Values;
using System.Collections.Generic;

namespace Ocelot.Configuration.Creator
{
    public interface IUpstreamHeaderTemplatePatternCreator
    {
        Dictionary<string, UpstreamHeaderTemplate> Create(IRoute route);
    }
}

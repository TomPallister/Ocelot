using Ocelot.Configuration.File;
using System.Collections.Generic;

namespace Ocelot.Configuration.Creator
{
    public interface IQoSOptionsCreator
    {
        QoSOptions Create(FileQoSOptions options);

        QoSOptions Create(FileQoSOptions options, string pathTemplate, List<string> httpMethods);

        QoSOptions Create(QoSOptions options, string pathTemplate, List<string> httpMethods);
    }
}

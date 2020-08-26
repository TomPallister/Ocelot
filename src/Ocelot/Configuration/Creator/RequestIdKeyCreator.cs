using Ocelot.Configuration.File;

namespace Ocelot.Configuration.Creator
{
    public class RequestIdKeyCreator : IRequestIdKeyCreator
    {
        public string Create(FileRoute fileRoute, FileGlobalConfiguration globalConfiguration)
        {
            var routeId = !string.IsNullOrEmpty(fileRoute.RequestIdKey);

            var requestIdKey = routeId
               ? fileRoute.RequestIdKey
               : globalConfiguration.RequestIdKey;

            return requestIdKey;
        }
    }
}

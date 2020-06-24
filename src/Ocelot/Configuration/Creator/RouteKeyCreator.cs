using Ocelot.Configuration.File;
using Ocelot.LoadBalancer.LoadBalancers;
using System.Linq;

namespace Ocelot.Configuration.Creator
{
    public class RouteKeyCreator : IRouteKeyCreator
    {
        public string Create(FileRoute fileRoute)
        {
            if (IsStickySession(fileRoute))
            {
                return $"{nameof(CookieStickySessions)}:{fileRoute.LoadBalancerOptions.Key}";
            }

            return $"{fileRoute.UpstreamPathTemplate}|{string.Join(",", fileRoute.UpstreamHttpMethod)}|{string.Join(",", fileRoute.DownstreamHostAndPorts.Select(x => $"{x.Host}:{x.Port}"))}";
        }

        private bool IsStickySession(FileRoute fileRoute)
        {
            if (!string.IsNullOrEmpty(fileRoute.LoadBalancerOptions.Type)
                && !string.IsNullOrEmpty(fileRoute.LoadBalancerOptions.Key)
                && fileRoute.LoadBalancerOptions.Type == nameof(CookieStickySessions))
            {
                return true;
            }

            return false;
        }
    }
}

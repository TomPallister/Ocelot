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

            return $"{fileRoute.UpstreamPathTemplate}|{ToUpstreamHttpMethodPart(fileRoute)}|{ToDownstreamHostPart(fileRoute)}";
        }

        private static string ToDownstreamHostPart(FileRoute fileRoute)
        {
            return string.Join(",", fileRoute.DownstreamHostAndPorts.Select(x => string.IsNullOrWhiteSpace(x.GlobalHostKey) ? $"{x.Host}:{x.Port}": x.GlobalHostKey));
        }

        private static string ToUpstreamHttpMethodPart(FileRoute fileRoute)
        {
            return string.Join(",", fileRoute.UpstreamHttpMethod);
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

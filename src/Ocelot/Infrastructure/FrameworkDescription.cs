using System.Runtime.InteropServices;

namespace Ocelot.Infrastructure
{
    public class FrameworkDescription : IFrameworkDescription
    {
        public string Get()
        {
            return RuntimeInformation.FrameworkDescription;
        }
    }
}

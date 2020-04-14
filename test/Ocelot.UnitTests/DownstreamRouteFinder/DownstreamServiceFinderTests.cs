using Ocelot.DownstreamRouteFinder.Finder;
using Xunit;

namespace Ocelot.UnitTests.DownstreamRouteFinder
{
    public class DownstreamServiceFinderTests
    {
        private readonly DownstreamServiceFinder _serviceFinder;

        public DownstreamServiceFinderTests()
        {
            _serviceFinder = new DownstreamServiceFinder();
        }

        [Fact]
        public void should_return_empty_when_without_path()
        {
            var serviceName = _serviceFinder.GetServiceName("/", null, null, null, null);
            Assert.Equal("", serviceName);
        }

        [Fact]
        public void should_return_node_name_when_single_node()
        {
            var serviceName = _serviceFinder.GetServiceName("/service-name", null, null, null, null);
            Assert.Equal("service-name", serviceName);
        }

        [Fact]
        public void should_return_first_node_name_when_multiple_nodes()
        {
            var serviceName = _serviceFinder.GetServiceName("/service-name/some/longer/path", null, null, null, null);
            Assert.Equal("service-name", serviceName);
        }
    }
}

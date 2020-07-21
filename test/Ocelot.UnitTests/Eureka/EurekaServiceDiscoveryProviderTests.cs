namespace Ocelot.UnitTests.Eureka
{
    using Moq;
    using Provider.Eureka;
    using Shouldly;
    using Steeltoe.Common.Discovery;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TestStack.BDDfy;
    using Values;
    using Xunit;

    public class EurekaServiceDiscoveryProviderTests
    {
        private readonly Eureka _provider;
        private readonly Mock<IDiscoveryClient> _client;
        private readonly string _serviceId;
        private List<IServiceInstance> _instances;
        private List<Service> _result;

        public EurekaServiceDiscoveryProviderTests()
        {
            _serviceId = "Laura";
            _client = new Mock<IDiscoveryClient>();
            _provider = new Eureka(_serviceId, _client.Object);
        }

        [Fact]
        public void should_return_empty_services()
        {
            this.When(_ => WhenIGet())
                .Then(_ => ThenTheCountIs(0))
                .BDDfy();
        }

        [Fact]
        public void should_return_service_from_client()
        {
            var instances = new List<IServiceInstance>
            {
                new EurekaService(_serviceId, "somehost", 801, false, new Uri("http://somehost:801"), new Dictionary<string, string>())
            };

            this.Given(_ => GivenThe(instances))
                .When(_ => WhenIGet())
                .Then(_ => ThenTheCountIs(1))
                .And(_ => ThenTheClientIsCalledCorrectly())
                .And(_ => ThenTheServiceIsMapped())
                .BDDfy();
        }

        [Fact]
        public void should_return_services_from_client()
        {
            var instances = new List<IServiceInstance>
            {
                new EurekaService(_serviceId, "somehost", 801, false, new Uri("http://somehost:801"), new Dictionary<string, string>()),
                new EurekaService(_serviceId, "somehost", 801, false, new Uri("http://somehost:801"), new Dictionary<string, string>())
            };

            this.Given(_ => GivenThe(instances))
                .When(_ => WhenIGet())
                .Then(_ => ThenTheCountIs(2))
                .And(_ => ThenTheClientIsCalledCorrectly())
                .BDDfy();
        }

        private void ThenTheServiceIsMapped()
        {
            _result[0].HostAndPort.DownstreamHost.ShouldBe("somehost");
            _result[0].HostAndPort.DownstreamPort.ShouldBe(801);
            _result[0].Name.ShouldBe(_serviceId);
        }

        private void ThenTheCountIs(int expected)
        {
            _result.Count.ShouldBe(expected);
        }

        private void ThenTheClientIsCalledCorrectly()
        {
            _client.Verify(x => x.GetInstances(_serviceId), Times.Once);
        }

        private async Task WhenIGet()
        {
            _result = await _provider.Get();
        }

        private void GivenThe(List<IServiceInstance> instances)
        {
            _instances = instances;
            _client.Setup(x => x.GetInstances(It.IsAny<string>())).Returns(instances);
        }
    }

    public class EurekaService : IServiceInstance
    {
        public EurekaService(string serviceId, string host, int port, bool isSecure, Uri uri, IDictionary<string, string> metadata)
        {
            ServiceId = serviceId;
            Host = host;
            Port = port;
            IsSecure = isSecure;
            Uri = uri;
            Metadata = metadata;
        }

        public string ServiceId { get; }
        public string Host { get; }
        public int Port { get; }
        public bool IsSecure { get; }
        public Uri Uri { get; }
        public IDictionary<string, string> Metadata { get; }
    }
}

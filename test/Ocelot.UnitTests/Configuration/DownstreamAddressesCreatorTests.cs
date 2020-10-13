using Moq;
using Ocelot.Logging;

namespace Ocelot.UnitTests.Configuration
{
    using Ocelot.Configuration;
    using Ocelot.Configuration.Creator;
    using Ocelot.Configuration.File;
    using Shouldly;
    using System.Collections.Generic;
    using TestStack.BDDfy;
    using Xunit;

    public class DownstreamAddressesCreatorTests
    {
        private DownstreamAddressesCreator _creator;
        private FileRoute _route;
        private List<DownstreamHostAndPort> _result;
        private Mock<IOcelotLoggerFactory> _factory;
        private Mock<IOcelotLogger> _logger;
        private FileGlobalConfiguration _globalConfiguration;

        public DownstreamAddressesCreatorTests()
        {
            _logger = new Mock<IOcelotLogger>();
            _factory = new Mock<IOcelotLoggerFactory>();
            _factory.Setup(x => x.CreateLogger<DownstreamAddressesCreator>()).Returns(_logger.Object);
            _creator = new DownstreamAddressesCreator(_factory.Object);
        }

        [Fact]
        public void should_do_nothing()
        {
            var route = new FileRoute
            {
            };

            var expected = new List<DownstreamHostAndPort>
            {
            };

            this.Given(x => GivenTheFollowingRoute(route))
                .When(x => WhenICreate())
                .Then(x => TheThenFollowingIsReturned(expected))
                .BDDfy();
        }

        [Fact]
        public void should_create_downstream_addresses_from_old_downstream_path_and_port()
        {
            var route = new FileRoute
            {
                DownstreamHostAndPorts = new List<FileDownstreamHostConfig>
                {
                    new FileDownstreamHostConfig
                    {
                        Host = "test",
                        Port = 80
                    }
                },
            };

            var expected = new List<DownstreamHostAndPort>
            {
                new DownstreamHostAndPort("test", 80),
            };

            this.Given(x => GivenTheFollowingRoute(route))
                .When(x => WhenICreate())
                .Then(x => TheThenFollowingIsReturned(expected))
                .BDDfy();
        }

        [Fact]
        public void should_create_downstream_addresses_from_downstream_host_and_ports()
        {
            var route = new FileRoute
            {
                DownstreamHostAndPorts = new List<FileDownstreamHostConfig>
                {
                    new FileDownstreamHostConfig
                    {
                        Host = "test",
                        Port = 80
                    },
                    new FileDownstreamHostConfig
                    {
                        Host = "west",
                        Port = 443
                    }
                }
            };

            var expected = new List<DownstreamHostAndPort>
            {
                new DownstreamHostAndPort("test", 80),
                new DownstreamHostAndPort("west", 443)
            };

            this.Given(x => GivenTheFollowingRoute(route))
                .When(x => WhenICreate())
                .Then(x => TheThenFollowingIsReturned(expected))
                .BDDfy();
        }
        
        [Fact]
        public void should_create_downstream_addresses_from_reference_to_global_downstream_host_configuration()
        {
            var route = new FileRoute
            {
                DownstreamHostAndPorts = new List<FileDownstreamHostConfig>
                {
                    new FileDownstreamHostConfig
                    {
                        GlobalHostKey = "TestHost"
                    }
                }
            };

            var globalConfig = new FileGlobalConfiguration
            {
                DownstreamHosts = new Dictionary<string, FileGlobalDownstreamHostConfig>
                {
                    ["TestHost"] = new FileGlobalDownstreamHostConfig
                    {
                        Host = "some.service.test.com",
                        Port = 9090
                    }
                }
            };

            var expected = new List<DownstreamHostAndPort>
            {
                new DownstreamHostAndPort("some.service.test.com", 9090),
            };

            this.Given(x => GivenTheFollowingRoute(route))
                .Given(x => GivenTheFollowingGlobalConfiguration(globalConfig))
                .When(x => WhenICreate())
                .Then(x => TheThenFollowingIsReturned(expected))
                .BDDfy();
        }

        private void GivenTheFollowingRoute(FileRoute route)
        {
            _route = route;
        }
        
        private void GivenTheFollowingGlobalConfiguration(FileGlobalConfiguration globalConfiguration)
        {
            _globalConfiguration = globalConfiguration;
        }

        private void WhenICreate()
        {
            _result = _creator.Create(_route, _globalConfiguration ?? new FileGlobalConfiguration());
        }

        private void TheThenFollowingIsReturned(List<DownstreamHostAndPort> expecteds)
        {
            _result.Count.ShouldBe(expecteds.Count);

            for (int i = 0; i < _result.Count; i++)
            {
                var result = _result[i];
                var expected = expecteds[i];

                result.Host.ShouldBe(expected.Host);
                result.Port.ShouldBe(expected.Port);
            }
        }
    }
}

namespace Ocelot.UnitTests.Consul
{
    using global::Consul;
    using Microsoft.Extensions.Options;
    using Moq;
    using Newtonsoft.Json;
    using Ocelot.Cache;
    using Ocelot.Configuration;
    using Ocelot.Configuration.Builder;
    using Ocelot.Configuration.File;
    using Ocelot.Configuration.Repository;
    using Ocelot.Logging;
    using Provider.Consul;
    using Responses;
    using Shouldly;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using TestStack.BDDfy;
    using Xunit;

    public class ConsulFileConfigurationRepositoryTests
    {
        private ConsulFileConfigurationRepository _repo;
        private Mock<IOptions<FileConfiguration>> _options;
        private Mock<IOcelotCache<FileConfiguration>> _cache;
        private Mock<IConsulClientFactory> _factory;
        private Mock<IOcelotLoggerFactory> _loggerFactory;
        private Mock<IConsulClient> _client;
        private Mock<IKVEndpoint> _kvEndpoint;
        private FileConfiguration _fileConfiguration;
        private Response _setResult;
        private Response<FileConfiguration> _getResult;

        public ConsulFileConfigurationRepositoryTests()
        {
            _cache = new Mock<IOcelotCache<FileConfiguration>>();
            _loggerFactory = new Mock<IOcelotLoggerFactory>();

            _options = new Mock<IOptions<FileConfiguration>>();
            _factory = new Mock<IConsulClientFactory>();
            _client = new Mock<IConsulClient>();
            _kvEndpoint = new Mock<IKVEndpoint>();

            _client
                .Setup(x => x.KV)
                .Returns(_kvEndpoint.Object);

            _factory
                .Setup(x => x.Get(It.IsAny<ConsulRegistryConfiguration>()))
                .Returns(_client.Object);

            _options
                .SetupGet(x => x.Value)
                .Returns(() => _fileConfiguration);
        }

        [Fact]
        public void should_set_config()
        {
            var config = FakeFileConfiguration();

            this.Given(_ => GivenIHaveAConfiguration(config))
                .And(_ => GivenWritingToConsulSucceeds())
                .When(_ => WhenISetTheConfiguration())
                .Then(_ => ThenTheConfigurationIsStoredAs(config))
                .BDDfy();
        }

        [Fact]
        public void should_get_config()
        {
            var config = FakeFileConfiguration();

            this.Given(_ => GivenIHaveAConfiguration(config))
               .And(_ => GivenFetchFromConsulSucceeds())
               .When(_ => WhenIGetTheConfiguration())
               .Then(_ => ThenTheConfigurationIs(config))
               .BDDfy();
        }

        [Fact]
        public void should_get_null_config()
        {
            var config = FakeFileConfiguration();

            this.Given(_ => GivenIHaveAConfiguration(config))
               .Given(_ => GivenFetchFromConsulReturnsNull())
               .When(_ => WhenIGetTheConfiguration())
               .Then(_ => ThenTheConfigurationIsNull())
               .BDDfy();
        }

        [Fact]
        public void should_get_config_from_cache()
        {
            var config = FakeFileConfiguration();

            this.Given(_ => GivenIHaveAConfiguration(config))
               .And(_ => GivenFetchFromCacheSucceeds())
               .When(_ => WhenIGetTheConfiguration())
               .Then(_ => ThenTheConfigurationIs(config))
               .BDDfy();
        }

        [Fact]
        public void should_set_config_key()
        {
            var config = FakeFileConfiguration();

            this.Given(_ => GivenIHaveAConfiguration(config))
                .And(_ => GivenTheConfigKeyComesFromFileConfig("Tom"))
                .And(_ => GivenFetchFromConsulSucceeds())
                .When(_ => WhenIGetTheConfiguration())
                .And(_ => ThenTheConfigKeyIs("Tom"))
                .BDDfy();
        }

        [Fact]
        public void should_set_default_config_key()
        {
            var config = FakeFileConfiguration();

            this.Given(_ => GivenIHaveAConfiguration(config))
                .And(_ => GivenFetchFromConsulSucceeds())
                .When(_ => WhenIGetTheConfiguration())
                .And(_ => ThenTheConfigKeyIs("InternalConfiguration"))
                .BDDfy();
        }

        private void ThenTheConfigKeyIs(string expected)
        {
            _kvEndpoint
                .Verify(x => x.Get(expected, It.IsAny<CancellationToken>()), Times.Once);
        }

        private void GivenTheConfigKeyComesFromFileConfig(string key)
        {
            _fileConfiguration.GlobalConfiguration.ServiceDiscoveryProvider.ConfigurationKey = key;
            _repo = new ConsulFileConfigurationRepository(_options.Object, _cache.Object, _factory.Object, _loggerFactory.Object);
        }

        private void ThenTheConfigurationIsNull()
        {
            _getResult.Data.ShouldBeNull();
        }

        private void ThenTheConfigurationIs(FileConfiguration config)
        {
            var expected = JsonConvert.SerializeObject(config, Formatting.Indented);
            var result = JsonConvert.SerializeObject(_getResult.Data, Formatting.Indented);
            result.ShouldBe(expected);
        }

        private async Task WhenIGetTheConfiguration()
        {
            _getResult = await _repo.Get();
        }

        private void GivenWritingToConsulSucceeds()
        {
            var response = new WriteResult<bool>();
            response.Response = true;

            _kvEndpoint
                .Setup(x => x.Put(It.IsAny<KVPair>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);
        }

        private void GivenFetchFromCacheSucceeds()
        {
            _cache.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>())).Returns(_fileConfiguration);
        }

        private void GivenFetchFromConsulReturnsNull()
        {
            QueryResult<KVPair> result = new QueryResult<KVPair>();

            _kvEndpoint
                .Setup(x => x.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);
        }

        private void GivenFetchFromConsulSucceeds()
        {
            var json = JsonConvert.SerializeObject(_fileConfiguration, Formatting.Indented);

            var bytes = Encoding.UTF8.GetBytes(json);

            var kvp = new KVPair("OcelotConfiguration");
            kvp.Value = bytes;

            var query = new QueryResult<KVPair>();
            query.Response = kvp;

            _kvEndpoint
                .Setup(x => x.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(query);
        }

        private void ThenTheConfigurationIsStoredAs(FileConfiguration config)
        {
            var json = JsonConvert.SerializeObject(config, Formatting.Indented);

            var bytes = Encoding.UTF8.GetBytes(json);

            _kvEndpoint
                .Verify(x => x.Put(It.Is<KVPair>(k => k.Value.SequenceEqual(bytes)), It.IsAny<CancellationToken>()), Times.Once);
        }

        private async Task WhenISetTheConfiguration()
        {
            _setResult = await _repo.Set(_fileConfiguration);
        }

        private void GivenIHaveAConfiguration(FileConfiguration config)
        {
            _fileConfiguration = config;

            _repo = new ConsulFileConfigurationRepository(_options.Object, _cache.Object, _factory.Object, _loggerFactory.Object);
        }

        private FileConfiguration FakeFileConfiguration()
        {
            var routes = new List<FileRoute>
            {
                new FileRoute
                {
                    DownstreamHostAndPorts = new List<FileHostAndPort>
                    {
                        new FileHostAndPort
                        {
                            Host = "123.12.12.12",
                            Port = 80,
                        }
                    },
                    DownstreamScheme = "https",
                    DownstreamPathTemplate = "/asdfs/test/{test}"
                }
            };

            var globalConfiguration = new FileGlobalConfiguration
            {
                ServiceDiscoveryProvider = new FileServiceDiscoveryProvider
                {
                    Scheme = "https",
                    Port = 198,
                    Host = "blah"
                }
            };

            return new FileConfiguration
            {
                GlobalConfiguration = globalConfiguration,
                Routes = routes
            };
        }
    }
}

using Ocelot.Configuration;
using Ocelot.Configuration.Builder;
using Ocelot.Configuration.Creator;
using Ocelot.Configuration.File;
using Shouldly;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.Configuration
{
    public class ServiceProviderCreatorTests
    {
        private readonly ServiceProviderConfigurationCreator _creator;
        private FileGlobalConfiguration _globalConfig;
        private ServiceProviderConfiguration _result;

        public ServiceProviderCreatorTests()
        {
            _creator = new ServiceProviderConfigurationCreator();
        }

        [Fact]
        public void should_create_service_provider_config()
        {
            var globalConfig = new FileGlobalConfiguration
            {
                ServiceDiscoveryProvider = new FileServiceDiscoveryProvider
                {
                    Scheme = "https",
                    Host = "127.0.0.1",
                    Port = 1234,
                    Type = "ServiceFabric",
                    Token = "testtoken",
                    ConfigurationKey = "woo",
                    Namespace = "default"
                }
            };

            var expected = new ServiceProviderConfigurationBuilder()
                .WithScheme("https")
                .WithHost("127.0.0.1")
                .WithPort(1234)
                .WithType("ServiceFabric")
                .WithToken("testtoken")
                .WithConfigurationKey("woo")
                .WithNamespace("default")
                .Build();

            this.Given(x => x.GivenTheFollowingGlobalConfig(globalConfig))
                .When(x => x.WhenICreate())
                .Then(x => x.ThenTheConfigIs(expected))
                .BDDfy();
        }

        private void GivenTheFollowingGlobalConfig(FileGlobalConfiguration fileGlobalConfig)
        {
            _globalConfig = fileGlobalConfig;
        }

        private void WhenICreate()
        {
            _result = _creator.Create(_globalConfig);
        }

        private void ThenTheConfigIs(ServiceProviderConfiguration expected)
        {
            _result.Scheme.ShouldBe(expected.Scheme);
            _result.Host.ShouldBe(expected.Host);
            _result.Port.ShouldBe(expected.Port);
            _result.Token.ShouldBe(expected.Token);
            _result.Type.ShouldBe(expected.Type);
            _result.Namespace.ShouldBe(expected.Namespace);
            _result.ConfigurationKey.ShouldBe(expected.ConfigurationKey);
        }
    }
}

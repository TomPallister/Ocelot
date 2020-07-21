namespace Ocelot.UnitTests.Configuration
{
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using Ocelot.Configuration;
    using Ocelot.Configuration.Creator;
    using Ocelot.Configuration.File;
    using Ocelot.DependencyInjection;
    using Shouldly;
    using System.Collections.Generic;
    using TestStack.BDDfy;
    using Xunit;

    public class ConfigurationCreatorTests
    {
        private ConfigurationCreator _creator;
        private InternalConfiguration _result;
        private readonly Mock<IServiceProviderConfigurationCreator> _spcCreator;
        private readonly Mock<IQoSOptionsCreator> _qosCreator;
        private readonly Mock<IHttpHandlerOptionsCreator> _hhoCreator;
        private readonly Mock<ILoadBalancerOptionsCreator> _lboCreator;
        private readonly Mock<IVersionCreator> _vCreator;
        private FileConfiguration _fileConfig;
        private List<Route> _routes;
        private ServiceProviderConfiguration _spc;
        private LoadBalancerOptions _lbo;
        private QoSOptions _qoso;
        private HttpHandlerOptions _hho;
        private AdministrationPath _adminPath;
        private readonly ServiceCollection _serviceCollection;

        public ConfigurationCreatorTests()
        {
            _vCreator = new Mock<IVersionCreator>();
            _lboCreator = new Mock<ILoadBalancerOptionsCreator>();
            _hhoCreator = new Mock<IHttpHandlerOptionsCreator>();
            _qosCreator = new Mock<IQoSOptionsCreator>();
            _spcCreator = new Mock<IServiceProviderConfigurationCreator>();
            _serviceCollection = new ServiceCollection();
        }

        [Fact]
        public void should_build_configuration_with_no_admin_path()
        {
            this.Given(_ => GivenTheDependenciesAreSetUp())
                .When(_ => WhenICreate())
                .Then(_ => ThenTheDepdenciesAreCalledCorrectly())
                .And(_ => ThenThePropertiesAreSetCorrectly())
                .And(_ => ThenTheAdminPathIsNull())
                .BDDfy();
        }

        [Fact]
        public void should_build_configuration_with_admin_path()
        {
            this.Given(_ => GivenTheDependenciesAreSetUp())
                .And(_ => GivenTheAdminPath())
                .When(_ => WhenICreate())
                .Then(_ => ThenTheDepdenciesAreCalledCorrectly())
                .And(_ => ThenThePropertiesAreSetCorrectly())
                .And(_ => ThenTheAdminPathIsSet())
                .BDDfy();
        }

        private void ThenTheAdminPathIsNull()
        {
            _result.AdministrationPath.ShouldBeNull();
        }

        private void ThenThePropertiesAreSetCorrectly()
        {
            _result.ShouldNotBeNull();
            _result.ServiceProviderConfiguration.ShouldBe(_spc);
            _result.LoadBalancerOptions.ShouldBe(_lbo);
            _result.QoSOptions.ShouldBe(_qoso);
            _result.HttpHandlerOptions.ShouldBe(_hho);
            _result.Routes.ShouldBe(_routes);
            _result.RequestId.ShouldBe(_fileConfig.GlobalConfiguration.RequestIdKey);
            _result.DownstreamScheme.ShouldBe(_fileConfig.GlobalConfiguration.DownstreamScheme);
        }

        private void ThenTheAdminPathIsSet()
        {
            _result.AdministrationPath.ShouldBe("wooty");
        }

        private void ThenTheDepdenciesAreCalledCorrectly()
        {
            _spcCreator.Verify(x => x.Create(_fileConfig.GlobalConfiguration), Times.Once);
            _lboCreator.Verify(x => x.Create(_fileConfig.GlobalConfiguration.LoadBalancerOptions), Times.Once);
            _qosCreator.Verify(x => x.Create(_fileConfig.GlobalConfiguration.QoSOptions), Times.Once);
            _hhoCreator.Verify(x => x.Create(_fileConfig.GlobalConfiguration.HttpHandlerOptions), Times.Once);
        }

        private void GivenTheAdminPath()
        {
            _adminPath = new AdministrationPath("wooty");
            _serviceCollection.AddSingleton<IAdministrationPath>(_adminPath);
        }

        private void GivenTheDependenciesAreSetUp()
        {
            _fileConfig = new FileConfiguration
            {
                GlobalConfiguration = new FileGlobalConfiguration()
            };
            _routes = new List<Route>();
            _spc = new ServiceProviderConfiguration("", "", "", 1, "", "", 1);
            _lbo = new LoadBalancerOptionsBuilder().Build();
            _qoso = new QoSOptions(1, 1, 1, "");
            _hho = new HttpHandlerOptionsBuilder().Build();

            _spcCreator.Setup(x => x.Create(It.IsAny<FileGlobalConfiguration>())).Returns(_spc);
            _lboCreator.Setup(x => x.Create(It.IsAny<FileLoadBalancerOptions>())).Returns(_lbo);
            _qosCreator.Setup(x => x.Create(It.IsAny<FileQoSOptions>())).Returns(_qoso);
            _hhoCreator.Setup(x => x.Create(It.IsAny<FileHttpHandlerOptions>())).Returns(_hho);
        }

        private void WhenICreate()
        {
            var serviceProvider = _serviceCollection.BuildServiceProvider();
            _creator = new ConfigurationCreator(_spcCreator.Object, _qosCreator.Object, _hhoCreator.Object, serviceProvider, _lboCreator.Object, _vCreator.Object);
            _result = _creator.Create(_fileConfig, _routes);
        }
    }
}

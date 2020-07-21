namespace Ocelot.UnitTests.Administration
{
    using IdentityServer4.AccessTokenValidation;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using Ocelot.Administration;
    using Ocelot.DependencyInjection;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using TestStack.BDDfy;
    using Xunit;

    public class OcelotAdministrationBuilderTests
    {
        private readonly IServiceCollection _services;
        private IServiceProvider _serviceProvider;
        private readonly IConfiguration _configRoot;
        private IOcelotBuilder _ocelotBuilder;
        private Exception _ex;

        public OcelotAdministrationBuilderTests()
        {
            _configRoot = new ConfigurationRoot(new List<IConfigurationProvider>());
            _services = new ServiceCollection();
            _services.AddSingleton<IWebHostEnvironment>(GetHostingEnvironment());
            _services.AddSingleton(_configRoot);
        }
        
        private IWebHostEnvironment GetHostingEnvironment()
        {
            var environment = new Mock<IWebHostEnvironment>();
            environment
                .Setup(e => e.ApplicationName)
                .Returns(typeof(OcelotAdministrationBuilderTests).GetTypeInfo().Assembly.GetName().Name);

            return environment.Object;
        }

        //keep
        [Fact]
        public void should_set_up_administration_with_identity_server_options()
        {
            Action<IdentityServerAuthenticationOptions> options = o => { };

            this.Given(x => WhenISetUpOcelotServices())
                .When(x => WhenISetUpAdministration(options))
                .Then(x => ThenAnExceptionIsntThrown())
                .Then(x => ThenTheCorrectAdminPathIsRegitered())
                .BDDfy();
        }

        //keep
        [Fact]
        public void should_set_up_administration()
        {
            this.Given(x => WhenISetUpOcelotServices())
                .When(x => WhenISetUpAdministration())
                .Then(x => ThenAnExceptionIsntThrown())
                .Then(x => ThenTheCorrectAdminPathIsRegitered())
                .BDDfy();
        }

        private void WhenISetUpAdministration()
        {
            _ocelotBuilder.AddAdministration("/administration", "secret");
        }

        private void WhenISetUpAdministration(Action<IdentityServerAuthenticationOptions> options)
        {
            _ocelotBuilder.AddAdministration("/administration", options);
        }

        private void ThenTheCorrectAdminPathIsRegitered()
        {
            _serviceProvider = _services.BuildServiceProvider();
            var path = _serviceProvider.GetService<IAdministrationPath>();
            path.Path.ShouldBe("/administration");
        }

        private void WhenISetUpOcelotServices()
        {
            try
            {
                _ocelotBuilder = _services.AddOcelot(_configRoot);
            }
            catch (Exception e)
            {
                _ex = e;
            }
        }

        private void ThenAnExceptionIsntThrown()
        {
            _ex.ShouldBeNull();
        }
    }
}

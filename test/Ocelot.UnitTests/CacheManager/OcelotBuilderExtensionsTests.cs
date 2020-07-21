namespace Ocelot.UnitTests.CacheManager
{
    using global::CacheManager.Core;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting.Internal;
    using Moq;
    using Ocelot.Cache;
    using Ocelot.Cache.CacheManager;
    using Ocelot.Configuration;
    using Ocelot.Configuration.File;
    using Ocelot.DependencyInjection;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using TestStack.BDDfy;
    using Xunit;

    public class OcelotBuilderExtensionsTests
    {
        private readonly IServiceCollection _services;
        private IServiceProvider _serviceProvider;
        private readonly IConfiguration _configRoot;
        private IOcelotBuilder _ocelotBuilder;
        private readonly int _maxRetries;
        private Exception _ex;

        public OcelotBuilderExtensionsTests()
        {
            _configRoot = new ConfigurationRoot(new List<IConfigurationProvider>());
            _services = new ServiceCollection();
            _services.AddSingleton<IWebHostEnvironment>(GetHostingEnvironment());
            _services.AddSingleton(_configRoot);
            _maxRetries = 100;
        }

        private IWebHostEnvironment GetHostingEnvironment()
        {
            var environment = new Mock<IWebHostEnvironment>();
            environment
                .Setup(e => e.ApplicationName)
                .Returns(typeof(OcelotBuilderExtensionsTests).GetTypeInfo().Assembly.GetName().Name);

            return environment.Object;
        }

        [Fact]
        public void should_set_up_cache_manager()
        {
            this.Given(x => WhenISetUpOcelotServices())
                .When(x => WhenISetUpCacheManager())
                .Then(x => ThenAnExceptionIsntThrown())
                .And(x => OnlyOneVersionOfEachCacheIsRegistered())
                .BDDfy();
        }

        private void OnlyOneVersionOfEachCacheIsRegistered()
        {
            var outputCache = _services.Single(x => x.ServiceType == typeof(IOcelotCache<CachedResponse>));
            var outputCacheManager = _services.Single(x => x.ServiceType == typeof(ICacheManager<CachedResponse>));
            var instance = (ICacheManager<CachedResponse>)outputCacheManager.ImplementationInstance;
            var ocelotConfigCache = _services.Single(x => x.ServiceType == typeof(IOcelotCache<IInternalConfiguration>));
            var ocelotConfigCacheManager = _services.Single(x => x.ServiceType == typeof(ICacheManager<IInternalConfiguration>));
            var fileConfigCache = _services.Single(x => x.ServiceType == typeof(IOcelotCache<FileConfiguration>));
            var fileConfigCacheManager = _services.Single(x => x.ServiceType == typeof(ICacheManager<FileConfiguration>));

            instance.Configuration.MaxRetries.ShouldBe(_maxRetries);
            outputCache.ShouldNotBeNull();
            ocelotConfigCache.ShouldNotBeNull();
            ocelotConfigCacheManager.ShouldNotBeNull();
            fileConfigCache.ShouldNotBeNull();
            fileConfigCacheManager.ShouldNotBeNull();
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

        private void WhenISetUpCacheManager()
        {
            try
            {
                _ocelotBuilder.AddCacheManager(x =>
                {
                    x.WithMaxRetries(_maxRetries);
                    x.WithDictionaryHandle();
                });
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

namespace Ocelot.UnitTests.DependencyInjection
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using Ocelot.Configuration.Setter;
    using Ocelot.DependencyInjection;
    using Ocelot.Infrastructure;
    using Ocelot.Multiplexer;
    using Ocelot.Requester;
    using Ocelot.UnitTests.Requester;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using Microsoft.AspNetCore.Http;
    using TestStack.BDDfy;
    using Xunit;
    using System.Threading.Tasks;
    using Ocelot.LoadBalancer.LoadBalancers;
    using Ocelot.Responses;
    using Ocelot.Values;
    using static Ocelot.UnitTests.Multiplexing.UserDefinedResponseAggregatorTests;

    public class OcelotBuilderTests
    {
        private readonly IServiceCollection _services;
        private IServiceProvider _serviceProvider;
        private readonly IConfiguration _configRoot;
        private IOcelotBuilder _ocelotBuilder;
        private readonly int _maxRetries;
        private Exception _ex;

        public OcelotBuilderTests()
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
                .Returns(typeof(OcelotBuilderTests).GetTypeInfo().Assembly.GetName().Name);

            return environment.Object;
        }

        [Fact]
        public void should_add_specific_delegating_handlers_transient()
        {
            this.Given(x => WhenISetUpOcelotServices())
                .When(x => AddSpecificTransientDelegatingHandler<FakeDelegatingHandler>())
                .And(x => AddSpecificTransientDelegatingHandler<FakeDelegatingHandlerTwo>())
                .Then(x => ThenTheProviderIsRegisteredAndReturnsSpecificHandlers<FakeDelegatingHandler, FakeDelegatingHandlerTwo>())
                .And(x => ThenTheSpecificHandlersAreTransient())
                .BDDfy();
        }

        [Fact]
        public void should_add_type_specific_delegating_handlers_transient()
        {
            this.Given(x => WhenISetUpOcelotServices())
                .When(x => AddTypeSpecificTransientDelegatingHandler(typeof(FakeDelegatingHandler)))
                .And(x => AddTypeSpecificTransientDelegatingHandler(typeof(FakeDelegatingHandlerTwo)))
                .Then(x => ThenTheProviderIsRegisteredAndReturnsSpecificHandlers<FakeDelegatingHandler, FakeDelegatingHandlerTwo>())
                .And(x => ThenTheSpecificHandlersAreTransient())
                .BDDfy();
        }

        [Fact]
        public void should_add_global_delegating_handlers_transient()
        {
            this.Given(x => WhenISetUpOcelotServices())
                .When(x => AddTransientGlobalDelegatingHandler<FakeDelegatingHandler>())
                .And(x => AddTransientGlobalDelegatingHandler<FakeDelegatingHandlerTwo>())
                .Then(x => ThenTheProviderIsRegisteredAndReturnsHandlers<FakeDelegatingHandler, FakeDelegatingHandlerTwo>())
                .And(x => ThenTheGlobalHandlersAreTransient())
                .BDDfy();
        }

        [Fact]
        public void should_add_global_type_delegating_handlers_transient()
        {
            this.Given(x => WhenISetUpOcelotServices())
                .When(x => AddTransientGlobalDelegatingHandler<FakeDelegatingHandler>())
                .And(x => AddTransientGlobalDelegatingHandler<FakeDelegatingHandlerTwo>())
                .Then(x => ThenTheProviderIsRegisteredAndReturnsHandlers<FakeDelegatingHandler, FakeDelegatingHandlerTwo>())
                .And(x => ThenTheGlobalHandlersAreTransient())
                .BDDfy();
        }

        [Fact]
        public void should_set_up_services()
        {
            this.When(x => WhenISetUpOcelotServices())
                .Then(x => ThenAnExceptionIsntThrown())
                .BDDfy();
        }

        [Fact]
        public void should_return_ocelot_builder()
        {
            this.When(x => WhenISetUpOcelotServices())
                .Then(x => ThenAnOcelotBuilderIsReturned())
                .BDDfy();
        }

        [Fact]
        public void should_use_logger_factory()
        {
            this.Given(x => WhenISetUpOcelotServices())
                .When(x => WhenIValidateScopes())
                .When(x => WhenIAccessLoggerFactory())
                .Then(x => ThenAnExceptionIsntThrown())
                .BDDfy();
        }

        [Fact]
        public void should_set_up_without_passing_in_config()
        {
            this.When(x => WhenISetUpOcelotServicesWithoutConfig())
                .Then(x => ThenAnExceptionIsntThrown())
                .BDDfy();
        }

        [Fact]
        public void should_add_singleton_defined_aggregators()
        {
            this.Given(x => WhenISetUpOcelotServices())
                .When(x => AddSingletonDefinedAggregator<TestDefinedAggregator>())
                .When(x => AddSingletonDefinedAggregator<TestDefinedAggregator>())
                .Then(x => ThenTheProviderIsRegisteredAndReturnsSpecificAggregators<TestDefinedAggregator, TestDefinedAggregator>())
                .And(x => ThenTheAggregatorsAreSingleton<TestDefinedAggregator, TestDefinedAggregator>())
                .BDDfy();
        }

        [Fact]
        public void should_add_transient_defined_aggregators()
        {
            this.Given(x => WhenISetUpOcelotServices())
                .When(x => AddTransientDefinedAggregator<TestDefinedAggregator>())
                .When(x => AddTransientDefinedAggregator<TestDefinedAggregator>())
                .Then(x => ThenTheProviderIsRegisteredAndReturnsSpecificAggregators<TestDefinedAggregator, TestDefinedAggregator>())
                .And(x => ThenTheAggregatorsAreTransient<TestDefinedAggregator, TestDefinedAggregator>())
                .BDDfy();
        }

        [Fact]
        public void should_add_custom_load_balancer_creators_by_default_ctor()
        {
            this.Given(x => WhenISetUpOcelotServices())
                .When(x => _ocelotBuilder.AddCustomLoadBalancer<FakeCustomLoadBalancer>())
                .Then(x => ThenTheProviderIsRegisteredAndReturnsBothBuiltInAndCustomLoadBalancerCreators())
                .BDDfy();
        }

        [Fact]
        public void should_add_custom_load_balancer_creators_by_factory_method()
        {
            this.Given(x => WhenISetUpOcelotServices())
                .When(x => _ocelotBuilder.AddCustomLoadBalancer(() => new FakeCustomLoadBalancer()))
                .Then(x => ThenTheProviderIsRegisteredAndReturnsBothBuiltInAndCustomLoadBalancerCreators())
                .BDDfy();
        }

        [Fact]
        public void should_add_custom_load_balancer_creators_by_di_factory_method()
        {
            this.Given(x => WhenISetUpOcelotServices())
                .When(x => _ocelotBuilder.AddCustomLoadBalancer(provider => new FakeCustomLoadBalancer()))
                .Then(x => ThenTheProviderIsRegisteredAndReturnsBothBuiltInAndCustomLoadBalancerCreators())
                .BDDfy();
        }

        [Fact]
        public void should_add_custom_load_balancer_creators_by_factory_method_with_arguments()
        {
            this.Given(x => WhenISetUpOcelotServices())
                .When(x => _ocelotBuilder.AddCustomLoadBalancer((route, discoveryProvider) => new FakeCustomLoadBalancer()))
                .Then(x => ThenTheProviderIsRegisteredAndReturnsBothBuiltInAndCustomLoadBalancerCreators())
                .BDDfy();
        }
        
        [Fact]
        public void should_replace_iplaceholder()
        {
            this.Given(x => x.WhenISetUpOcelotServices())
                .When(x => AddConfigPlaceholders())
                .Then(x => ThenAnExceptionIsntThrown())
                .And(x => ThenTheIPlaceholderInstanceIsReplaced())
                .BDDfy();
        }

        [Fact]
        public void should_add_custom_load_balancer_creators()
        {
            this.Given(x => WhenISetUpOcelotServices())
                .When(x => _ocelotBuilder.AddCustomLoadBalancer((provider, route, discoveryProvider) => new FakeCustomLoadBalancer()))
                .Then(x => ThenTheProviderIsRegisteredAndReturnsBothBuiltInAndCustomLoadBalancerCreators())
                .BDDfy();
        }

        private void AddSingletonDefinedAggregator<T>()
            where T : class, IDefinedAggregator
        {
            _ocelotBuilder.AddSingletonDefinedAggregator<T>();
        }
        
        private void AddTransientDefinedAggregator<T>()
            where T : class, IDefinedAggregator
        {
            _ocelotBuilder.AddTransientDefinedAggregator<T>();
        }

        private void AddConfigPlaceholders()
        {
            _ocelotBuilder.AddConfigPlaceholders();
        }

        private void ThenTheSpecificHandlersAreTransient()
        {
            var handlers = _serviceProvider.GetServices<DelegatingHandler>().ToList();
            var first = handlers[0];
            handlers = _serviceProvider.GetServices<DelegatingHandler>().ToList();
            var second = handlers[0];
            first.ShouldNotBe(second);
        }

        private void ThenTheGlobalHandlersAreTransient()
        {
            var handlers = _serviceProvider.GetServices<GlobalDelegatingHandler>().ToList();
            var first = handlers[0].DelegatingHandler;
            handlers = _serviceProvider.GetServices<GlobalDelegatingHandler>().ToList();
            var second = handlers[0].DelegatingHandler;
            first.ShouldNotBe(second);
        }

        private void AddTransientGlobalDelegatingHandler<T>()
            where T : DelegatingHandler
        {
            _ocelotBuilder.AddDelegatingHandler<T>(true);
        }

        private void AddSpecificTransientDelegatingHandler<T>()
            where T : DelegatingHandler
        {
            _ocelotBuilder.AddDelegatingHandler<T>();
        }

        private void AddTypeTransientGlobalDelegatingHandler(Type type)
        {
            _ocelotBuilder.AddDelegatingHandler(type, true);
        }

        private void AddTypeSpecificTransientDelegatingHandler(Type type)
        {
            _ocelotBuilder.AddDelegatingHandler(type);
        }

        private void ThenTheProviderIsRegisteredAndReturnsHandlers<TOne, TWo>()
        {
            _serviceProvider = _services.BuildServiceProvider();
            var handlers = _serviceProvider.GetServices<GlobalDelegatingHandler>().ToList();
            handlers[0].DelegatingHandler.ShouldBeOfType<TOne>();
            handlers[1].DelegatingHandler.ShouldBeOfType<TWo>();
        }

        private void ThenTheProviderIsRegisteredAndReturnsSpecificHandlers<TOne, TWo>()
        {
            _serviceProvider = _services.BuildServiceProvider();
            var handlers = _serviceProvider.GetServices<DelegatingHandler>().ToList();
            handlers[0].ShouldBeOfType<TOne>();
            handlers[1].ShouldBeOfType<TWo>();
        }

        private void ThenTheProviderIsRegisteredAndReturnsSpecificAggregators<TOne, TWo>()
        {
            _serviceProvider = _services.BuildServiceProvider();
            var handlers = _serviceProvider.GetServices<IDefinedAggregator>().ToList();
            handlers[0].ShouldBeOfType<TOne>();
            handlers[1].ShouldBeOfType<TWo>();
        }
        
        private void ThenTheProviderIsRegisteredAndReturnsBothBuiltInAndCustomLoadBalancerCreators()
        {
            _serviceProvider = _services.BuildServiceProvider();
            var creators = _serviceProvider.GetServices<ILoadBalancerCreator>().ToList();
            creators.Count(c => c.GetType() == typeof(NoLoadBalancerCreator)).ShouldBe(1);
            creators.Count(c => c.GetType() == typeof(RoundRobinCreator)).ShouldBe(1);
            creators.Count(c => c.GetType() == typeof(CookieStickySessionsCreator)).ShouldBe(1);
            creators.Count(c => c.GetType() == typeof(LeastConnectionCreator)).ShouldBe(1);
            creators.Count(c => c.GetType() == typeof(DelegateInvokingLoadBalancerCreator<FakeCustomLoadBalancer>)).ShouldBe(1);
        }

        private void ThenTheAggregatorsAreTransient<TOne, TWo>()
        {
            var aggregators = _serviceProvider.GetServices<IDefinedAggregator>().ToList();
            var first = aggregators[0];
            aggregators = _serviceProvider.GetServices<IDefinedAggregator>().ToList();
            var second = aggregators[0];
            first.ShouldNotBe(second);
        }

        private void ThenTheAggregatorsAreSingleton<TOne, TWo>()
        {
            var aggregators = _serviceProvider.GetServices<IDefinedAggregator>().ToList();
            var first = aggregators[0];
            aggregators = _serviceProvider.GetServices<IDefinedAggregator>().ToList();
            var second = aggregators[0];
            first.ShouldBe(second);
        }

        private void ThenAnOcelotBuilderIsReturned()
        {
            _ocelotBuilder.ShouldBeOfType<OcelotBuilder>();
        }

        private void ThenTheIPlaceholderInstanceIsReplaced()
        {
            _serviceProvider = _services.BuildServiceProvider();
            var placeholders = _serviceProvider.GetService<IPlaceholders>();
            placeholders.ShouldBeOfType<ConfigAwarePlaceholders>();
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

        private void WhenISetUpOcelotServicesWithoutConfig()
        {
            try
            {
                _ocelotBuilder = _services.AddOcelot();
            }
            catch (Exception e)
            {
                _ex = e;
            }
        }

        private void WhenIAccessLoggerFactory()
        {
            try
            {
                _serviceProvider = _services.BuildServiceProvider();
                var logger = _serviceProvider.GetService<IFileConfigurationSetter>();
                logger.ShouldNotBeNull();
            }
            catch (Exception e)
            {
                _ex = e;
            }
        }

        private void WhenIValidateScopes()
        {
            try
            {
                _serviceProvider = _services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
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

        private class FakeCustomLoadBalancer : ILoadBalancer
        {
            public Task<Response<ServiceHostAndPort>> Lease(HttpContext httpContext)
            {
                // Not relevant for these tests
                throw new NotImplementedException();
            }

            public void Release(ServiceHostAndPort hostAndPort)
            {
                // Not relevant for these tests
                throw new NotImplementedException();
            }
        }
    }
}

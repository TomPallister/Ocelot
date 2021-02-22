namespace Ocelot.DependencyInjection
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Options;
    using Ocelot.Authorization;
    using Ocelot.Cache;
    using Ocelot.Claims;
    using Ocelot.Configuration;
    using Ocelot.ServiceDiscovery.Providers;
    using Ocelot.Configuration.ChangeTracking;
    using Ocelot.Configuration.Creator;
    using Ocelot.Configuration.File;
    using Ocelot.Configuration.Parser;
    using Ocelot.Configuration.Repository;
    using Ocelot.Configuration.Setter;
    using Ocelot.Configuration.Validator;
    using Ocelot.DownstreamRouteFinder.Finder;
    using Ocelot.DownstreamRouteFinder.UrlMatcher;
    using Ocelot.DownstreamUrlCreator.UrlTemplateReplacer;
    using Ocelot.Headers;
    using Ocelot.Infrastructure;
    using Ocelot.Infrastructure.Claims.Parser;
    using Ocelot.Infrastructure.RequestData;
    using Ocelot.LoadBalancer.LoadBalancers;
    using Ocelot.Logging;
    using Ocelot.Middleware;
    using Ocelot.Multiplexer;
    using Ocelot.PathManipulation;
    using Ocelot.QueryStrings;
    using Ocelot.RateLimit;
    using Ocelot.Request.Creator;
    using Ocelot.Request.Mapper;
    using Ocelot.Requester;
    using Ocelot.Requester.QoS;
    using Ocelot.Responder;
    using Ocelot.Security;
    using Ocelot.Security.IPSecurity;
    using Ocelot.ServiceDiscovery;
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;

    public class OcelotBuilder : IOcelotBuilder
    {
        public IServiceCollection Services { get; }
        public IConfiguration Configuration { get; }
        public IMvcCoreBuilder MvcCoreBuilder { get; }

        public OcelotBuilder(IServiceCollection services, IConfiguration configurationRoot)
        {
            Configuration = configurationRoot;
            Services = services;
            Services.Configure<FileConfiguration>(configurationRoot);

            Services.TryAddSingleton<IOcelotCache<FileConfiguration>, AspMemoryCache<FileConfiguration>>();
            Services.TryAddSingleton<IOcelotCache<CachedResponse>, AspMemoryCache<CachedResponse>>();
            Services.TryAddSingleton<IHttpResponseHeaderReplacer, HttpResponseHeaderReplacer>();
            Services.TryAddSingleton<IHttpContextRequestHeaderReplacer, HttpContextRequestHeaderReplacer>();
            Services.TryAddSingleton<IHeaderFindAndReplaceCreator, HeaderFindAndReplaceCreator>();
            Services.TryAddSingleton<IInternalConfigurationCreator, FileInternalConfigurationCreator>();
            Services.TryAddSingleton<IInternalConfigurationRepository, InMemoryInternalConfigurationRepository>();
            Services.TryAddSingleton<IConfigurationValidator, FileConfigurationFluentValidator>();
            Services.TryAddSingleton<HostAndPortValidator>();
            Services.TryAddSingleton<IRoutesCreator, RoutesCreator>();
            Services.TryAddSingleton<IAggregatesCreator, AggregatesCreator>();
            Services.TryAddSingleton<IRouteKeyCreator, RouteKeyCreator>();
            Services.TryAddSingleton<IConfigurationCreator, ConfigurationCreator>();
            Services.TryAddSingleton<IDynamicsCreator, DynamicsCreator>();
            Services.TryAddSingleton<ILoadBalancerOptionsCreator, LoadBalancerOptionsCreator>();
            Services.TryAddSingleton<RouteFluentValidator>();
            Services.TryAddSingleton<FileGlobalConfigurationFluentValidator>();
            Services.TryAddSingleton<FileQoSOptionsFluentValidator>();
            Services.TryAddSingleton<IClaimsToThingCreator, ClaimsToThingCreator>();
            Services.TryAddSingleton<IAuthenticationOptionsCreator, AuthenticationOptionsCreator>();
            Services.TryAddSingleton<IUpstreamTemplatePatternCreator, UpstreamTemplatePatternCreator>();
            Services.TryAddSingleton<IRequestIdKeyCreator, RequestIdKeyCreator>();
            Services.TryAddSingleton<IConnectionCloseCreator, ConnectionCloseCreator>();
            Services.TryAddSingleton<IServiceProviderConfigurationCreator, ServiceProviderConfigurationCreator>();
            Services.TryAddSingleton<IQoSOptionsCreator, QoSOptionsCreator>();
            Services.TryAddSingleton<IRouteOptionsCreator, RouteOptionsCreator>();
            Services.TryAddSingleton<IRateLimitOptionsCreator, RateLimitOptionsCreator>();
            Services.TryAddSingleton<IBaseUrlFinder, BaseUrlFinder>();
            Services.TryAddSingleton<IRegionCreator, RegionCreator>();
            Services.TryAddSingleton<IFileConfigurationRepository, DiskFileConfigurationRepository>();
            Services.TryAddSingleton<IFileConfigurationSetter, FileAndInternalConfigurationSetter>();
            Services.TryAddSingleton<IServiceDiscoveryProviderFactory, ServiceDiscoveryProviderFactory>();
            Services.AddSingleton<ILoadBalancerCreator, NoLoadBalancerCreator>();
            Services.AddSingleton<ILoadBalancerCreator, RoundRobinCreator>();
            Services.AddSingleton<ILoadBalancerCreator, CookieStickySessionsCreator>();
            Services.AddSingleton<ILoadBalancerCreator, LeastConnectionCreator>();
            Services.TryAddSingleton<ILoadBalancerFactory, LoadBalancerFactory>();
            Services.TryAddSingleton<ILoadBalancerHouse, LoadBalancerHouse>();
            Services.TryAddSingleton<IOcelotLoggerFactory, AspDotNetLoggerFactory>();
            Services.TryAddSingleton<IRemoveOutputHeaders, RemoveOutputHeaders>();
            Services.TryAddSingleton<IClaimToThingConfigurationParser, ClaimToThingConfigurationParser>();
            Services.TryAddSingleton<IClaimsAuthorizer, ClaimsAuthorizer>();
            Services.TryAddSingleton<IScopesAuthorizer, ScopesAuthorizer>();
            Services.TryAddSingleton<IAddClaimsToRequest, AddClaimsToRequest>();
            Services.TryAddSingleton<IAddHeadersToRequest, AddHeadersToRequest>();
            Services.TryAddSingleton<IAddQueriesToRequest, AddQueriesToRequest>();
            Services.TryAddSingleton<IChangeDownstreamPathTemplate, ChangeDownstreamPathTemplate>();
            Services.TryAddSingleton<IClaimsParser, ClaimsParser>();
            Services.TryAddSingleton<IUrlPathToUrlTemplateMatcher, RegExUrlMatcher>();
            Services.TryAddSingleton<IPlaceholderNameAndValueFinder, UrlPathPlaceholderNameAndValueFinder>();
            Services.TryAddSingleton<IDownstreamPathPlaceholderReplacer, DownstreamTemplatePathPlaceholderReplacer>();
            Services.AddSingleton<IDownstreamRouteProvider, DownstreamRouteFinder>();
            Services.AddSingleton<IDownstreamRouteProvider, DownstreamRouteCreator>();
            Services.TryAddSingleton<IDownstreamRouteProviderFactory, DownstreamRouteProviderFactory>();
            Services.TryAddSingleton<IHttpRequester, HttpClientHttpRequester>();
            Services.TryAddSingleton<IHttpResponder, HttpContextResponder>();
            Services.TryAddSingleton<IErrorsToHttpStatusCodeMapper, ErrorsToHttpStatusCodeMapper>();
            Services.TryAddSingleton<IRateLimitCounterHandler, MemoryCacheRateLimitCounterHandler>();
            Services.TryAddSingleton<IHttpClientCache, MemoryHttpClientCache>();
            Services.TryAddSingleton<IRequestMapper, RequestMapper>();
            Services.TryAddSingleton<IHttpHandlerOptionsCreator, HttpHandlerOptionsCreator>();
            Services.TryAddSingleton<IDownstreamAddressesCreator, DownstreamAddressesCreator>();
            Services.TryAddSingleton<IDelegatingHandlerHandlerFactory, DelegatingHandlerHandlerFactory>();
            Services.TryAddSingleton<ICacheKeyGenerator, CacheKeyGenerator>();
            Services.TryAddSingleton<IOcelotConfigurationChangeTokenSource, OcelotConfigurationChangeTokenSource>();
            Services.TryAddSingleton<IOptionsMonitor<IInternalConfiguration>, OcelotConfigurationMonitor>();

            // see this for why we register this as singleton http://stackoverflow.com/questions/37371264/invalidoperationexception-unable-to-resolve-service-for-type-microsoft-aspnetc
            // could maybe use a scoped data repository
            Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            Services.TryAddSingleton<IRequestScopedDataRepository, HttpDataRepository>();
            Services.AddMemoryCache();
            Services.TryAddSingleton<OcelotDiagnosticListener>();
            Services.TryAddSingleton<IResponseAggregator, SimpleJsonResponseAggregator>();
            Services.TryAddSingleton<ITracingHandlerFactory, TracingHandlerFactory>();
            Services.TryAddSingleton<IFileConfigurationPollerOptions, InMemoryFileConfigurationPollerOptions>();
            Services.TryAddSingleton<IAddHeadersToResponse, AddHeadersToResponse>();
            Services.TryAddSingleton<IPlaceholders, Placeholders>();
            Services.TryAddSingleton<IResponseAggregatorFactory, InMemoryResponseAggregatorFactory>();
            Services.TryAddSingleton<IDefinedAggregatorProvider, ServiceLocatorDefinedAggregatorProvider>();
            Services.TryAddSingleton<IDownstreamRequestCreator, DownstreamRequestCreator>();
            Services.TryAddSingleton<IFrameworkDescription, FrameworkDescription>();
            Services.TryAddSingleton<IQoSFactory, QoSFactory>();
            Services.TryAddSingleton<IExceptionToErrorMapper, HttpExeptionToErrorMapper>();
            Services.TryAddSingleton<IVersionCreator, HttpVersionCreator>();

            //add security
            this.AddSecurity();

            //add asp.net services..
            var assembly = typeof(FileConfigurationController).GetTypeInfo().Assembly;

            this.MvcCoreBuilder = Services.AddMvcCore()
                  .AddApplicationPart(assembly)
                  .AddControllersAsServices()
                  .AddAuthorization()
                  .AddNewtonsoftJson(); 

            Services.AddLogging();
            Services.AddMiddlewareAnalysis();
            Services.AddWebEncoders();
        }

        public IOcelotBuilder AddSingletonDefinedAggregator<T>()
            where T : class, IDefinedAggregator
        {
            Services.AddSingleton<IDefinedAggregator, T>();
            return this;
        }

        public IOcelotBuilder AddTransientDefinedAggregator<T>()
            where T : class, IDefinedAggregator
        {
            Services.AddTransient<IDefinedAggregator, T>();
            return this;
        }

        public IOcelotBuilder AddCustomLoadBalancer<T>()
            where T : ILoadBalancer, new()
        {
            AddCustomLoadBalancer((provider, route, serviceDiscoveryProvider) => new T());
            return this;
        }
        
        public IOcelotBuilder AddCustomLoadBalancer<T>(Func<T> loadBalancerFactoryFunc) 
            where T : ILoadBalancer
        {
            AddCustomLoadBalancer((provider, route, serviceDiscoveryProvider) =>
                loadBalancerFactoryFunc());
            return this;
        }

        public IOcelotBuilder AddCustomLoadBalancer<T>(Func<IServiceProvider, T> loadBalancerFactoryFunc) 
            where T : ILoadBalancer
        {
            AddCustomLoadBalancer((provider, route, serviceDiscoveryProvider) =>
                loadBalancerFactoryFunc(provider));
            return this;
        }

        public IOcelotBuilder AddCustomLoadBalancer<T>(Func<DownstreamRoute, IServiceDiscoveryProvider, T> loadBalancerFactoryFunc)
            where T : ILoadBalancer
        {
            AddCustomLoadBalancer((provider, route, serviceDiscoveryProvider) =>
                loadBalancerFactoryFunc(route, serviceDiscoveryProvider));
            return this;
        }

        public IOcelotBuilder AddCustomLoadBalancer<T>(Func<IServiceProvider, DownstreamRoute, IServiceDiscoveryProvider, T> loadBalancerFactoryFunc)
            where T : ILoadBalancer
        {
            Services.AddSingleton<ILoadBalancerCreator>(provider =>
                new DelegateInvokingLoadBalancerCreator<T>(
                    (route, serviceDiscoveryProvider) => 
                        loadBalancerFactoryFunc(provider, route, serviceDiscoveryProvider)));
            return this;
        }

        private void AddSecurity()
        {
            Services.TryAddSingleton<ISecurityOptionsCreator, SecurityOptionsCreator>();
            Services.TryAddSingleton<ISecurityPolicy, IPSecurityPolicy>();
        }

        public IOcelotBuilder AddDelegatingHandler(Type delegateType, bool global = false)
        {
            if (!typeof(DelegatingHandler).IsAssignableFrom(delegateType)) throw new ArgumentOutOfRangeException(nameof(delegateType), delegateType.Name, "It is not a delegatin handler");

            if (global)
            {
                Services.AddTransient(delegateType);
                Services.AddTransient<GlobalDelegatingHandler>(s =>
                {

                    var service = s.GetService(delegateType) as DelegatingHandler;
                    return new GlobalDelegatingHandler(service);
                });
            }
            else
            {
                Services.AddTransient(typeof(DelegatingHandler), delegateType);
            }

            return this;
        }

        public IOcelotBuilder AddDelegatingHandler<THandler>(bool global = false)
            where THandler : DelegatingHandler
        {
            if (global)
            {
                Services.AddTransient<THandler>();
                Services.AddTransient<GlobalDelegatingHandler>(s =>
                {
                    var service = s.GetService<THandler>();
                    return new GlobalDelegatingHandler(service);
                });
            }
            else
            {
                Services.AddTransient<DelegatingHandler, THandler>();
            }

            return this;
        }

        public IOcelotBuilder AddConfigPlaceholders()
        {
            // see: https://greatrexpectations.com/2018/10/25/decorators-in-net-core-with-dependency-injection
            var wrappedDescriptor = Services.First(x => x.ServiceType == typeof(IPlaceholders));

            var objectFactory = ActivatorUtilities.CreateFactory(
                typeof(ConfigAwarePlaceholders),
                new[] { typeof(IPlaceholders) });

            Services.Replace(ServiceDescriptor.Describe(
                typeof(IPlaceholders),
                s => (IPlaceholders) objectFactory(s,
                    new[] {CreateInstance(s, wrappedDescriptor)}),
                wrappedDescriptor.Lifetime
            ));

            return this;
        }

        private static object CreateInstance(IServiceProvider services, ServiceDescriptor descriptor)
        {
            if (descriptor.ImplementationInstance != null)
            {
                return descriptor.ImplementationInstance;
            }

            if (descriptor.ImplementationFactory != null)
            {
                return descriptor.ImplementationFactory(services);
            }

            return ActivatorUtilities.GetServiceOrCreateInstance(services, descriptor.ImplementationType);
        }
    }
}

//namespace Ocelot.UnitTests.Eureka
//{
//    using Microsoft.Extensions.Configuration;
//    using Microsoft.Extensions.DependencyInjection;
//    using Ocelot.DependencyInjection;
//    using Ocelot.Middleware;
//    using Ocelot.Middleware.Pipeline;
//    using Steeltoe.Discovery.Client;
//    using Shouldly;
//    using Steeltoe.Common.Discovery;
//    using Steeltoe.Discovery.Eureka;
//    using TestStack.BDDfy;
//    using Xunit;

//    public class OcelotPipelineExtensionsTests
//    {
//        private OcelotPipelineBuilder _builder;
//        private OcelotRequestDelegate _handlers;

//        [Fact]
//        public void should_set_up_pipeline()
//        {
//            this.Given(_ => GivenTheDepedenciesAreSetUp())
//                 .When(_ => WhenIBuild())
//                 .Then(_ => ThenThePipelineIsBuilt())
//                 .BDDfy();
//        }

//        private void ThenThePipelineIsBuilt()
//        {
//            _handlers.ShouldNotBeNull();
//        }

//        private void WhenIBuild()
//        {
//            _handlers = _builder.BuildOcelotPipeline(new OcelotPipelineConfiguration());
//        }

//        private void GivenTheDepedenciesAreSetUp()
//        {
//            IConfigurationBuilder test = new ConfigurationBuilder();
//            var root = test.Build();
//            var services = new ServiceCollection();
//            services.AddSingleton<IConfiguration>(root);
//            services.AddDiscoveryClient(new DiscoveryOptions
//            {
//                ClientType = DiscoveryClientType.EUREKA,
//                ClientOptions = new EurekaClientOptions()
//                {
//                    ShouldFetchRegistry = false,
//                    ShouldRegisterWithEureka = false
//                }
//            });
//            services.AddOcelot();
//            var provider = services.BuildServiceProvider();
//            _builder = new OcelotPipelineBuilder(provider);
//        }
//    }
//}

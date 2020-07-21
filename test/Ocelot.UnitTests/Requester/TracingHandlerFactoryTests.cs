namespace Ocelot.UnitTests.Requester
{
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using Ocelot.Infrastructure.RequestData;
    using Ocelot.Logging;
    using Ocelot.Requester;
    using Shouldly;
    using System;
    using Xunit;

    public class TracingHandlerFactoryTests
    {
        private readonly TracingHandlerFactory _factory;
        private Mock<ITracer> _tracer;
        private IServiceCollection _serviceCollection;
        private IServiceProvider _serviceProvider;
        private Mock<IRequestScopedDataRepository> _repo;

        public TracingHandlerFactoryTests()
        {
            _tracer = new Mock<ITracer>();
            _serviceCollection = new ServiceCollection();
            _serviceCollection.AddSingleton<ITracer>(_tracer.Object);
            _serviceProvider = _serviceCollection.BuildServiceProvider();
            _repo = new Mock<IRequestScopedDataRepository>();
            _factory = new TracingHandlerFactory(_serviceProvider, _repo.Object);
        }

        [Fact]
        public void should_return()
        {
            var handler = _factory.Get();
            handler.ShouldBeOfType<OcelotHttpTracingHandler>();
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Ocelot.Logging;
using System;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.Logging
{
    public class OcelotDiagnosticListenerTests
    {
        private readonly OcelotDiagnosticListener _listener;
        private Mock<IOcelotLoggerFactory> _factory;
        private readonly Mock<IOcelotLogger> _logger;
        private IServiceCollection _serviceCollection;
        private IServiceProvider _serviceProvider;
        private string _name;
        private Exception _exception;
        private HttpContext _httpContext;

        public OcelotDiagnosticListenerTests()
        {
            _httpContext = new DefaultHttpContext();
            _factory = new Mock<IOcelotLoggerFactory>();
            _logger = new Mock<IOcelotLogger>();
            _serviceCollection = new ServiceCollection();
            _serviceProvider = _serviceCollection.BuildServiceProvider();
            _factory.Setup(x => x.CreateLogger<OcelotDiagnosticListener>()).Returns(_logger.Object);
            _listener = new OcelotDiagnosticListener(_factory.Object, _serviceProvider);
        }

        [Fact]
        public void should_trace_middleware_started()
        {
            this.Given(_ => GivenAMiddlewareName())
                .When(_ => WhenMiddlewareStartedCalled())
                .Then(_ => ThenTheLogIs($"MiddlewareStarting: {_name}; {_httpContext.Request.Path}"))
                .BDDfy();
        }

        [Fact]
        public void should_trace_middleware_finished()
        {
            this.Given(_ => GivenAMiddlewareName())
                .When(_ => WhenMiddlewareFinishedCalled())
                .Then(_ => ThenTheLogIs($"MiddlewareFinished: {_name}; {_httpContext.Response.StatusCode}"))
                .BDDfy();
        }

        [Fact]
        public void should_trace_middleware_exception()
        {
            this.Given(_ => GivenAMiddlewareName())
                .And(_ => GivenAException(new Exception("oh no")))
                .When(_ => WhenMiddlewareExceptionCalled())
                .Then(_ => ThenTheLogIs($"MiddlewareException: {_name}; {_exception.Message};"))
                .BDDfy();
        }

        private void GivenAException(Exception exception)
        {
            _exception = exception;
        }

        private void WhenMiddlewareStartedCalled()
        {
            _listener.OnMiddlewareStarting(_httpContext, _name);
        }

        private void WhenMiddlewareFinishedCalled()
        {
            _listener.OnMiddlewareFinished(_httpContext, _name);
        }

        private void WhenMiddlewareExceptionCalled()
        {
            _listener.OnMiddlewareException(_exception, _name);
        }

        private void GivenAMiddlewareName()
        {
            _name = "name";
        }

        private void ThenTheLogIs(string expected)
        {
            _logger.Verify(
                x => x.LogTrace(expected));
        }
    }
}

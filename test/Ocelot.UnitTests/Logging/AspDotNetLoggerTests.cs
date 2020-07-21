namespace Ocelot.UnitTests.Logging
{
    using Microsoft.Extensions.Logging;
    using Moq;
    using Ocelot.Infrastructure.RequestData;
    using Ocelot.Logging;
    using System;
    using Xunit;

    public class AspDotNetLoggerTests
    {
        private readonly Mock<ILogger<object>> _coreLogger;
        private readonly Mock<IRequestScopedDataRepository> _repo;
        private readonly AspDotNetLogger _logger;
        private readonly string _b;
        private readonly string _a;
        private readonly Exception _ex;

        public AspDotNetLoggerTests()
        {
            _a = "tom";
            _b = "laura";
            _ex = new Exception("oh no");
            _coreLogger = new Mock<ILogger<object>>();
            _repo = new Mock<IRequestScopedDataRepository>();
            _logger = new AspDotNetLogger(_coreLogger.Object, _repo.Object);
        }

        [Fact]
        public void should_log_trace()
        {
            _logger.LogTrace($"a message from {_a} to {_b}");

            ThenLevelIsLogged("requestId: no request id, previousRequestId: no previous request id, message: a message from tom to laura", LogLevel.Trace);
        }

        [Fact]
        public void should_log_info()
        {
            _logger.LogInformation($"a message from {_a} to {_b}");

            ThenLevelIsLogged("requestId: no request id, previousRequestId: no previous request id, message: a message from tom to laura", LogLevel.Information);
        }

        [Fact]
        public void should_log_warning()
        {
            _logger.LogWarning($"a message from {_a} to {_b}");

            ThenLevelIsLogged("requestId: no request id, previousRequestId: no previous request id, message: a message from tom to laura", LogLevel.Warning);
        }

        [Fact]
        public void should_log_error()
        {
            _logger.LogError($"a message from {_a} to {_b}", _ex);

            ThenLevelIsLogged("requestId: no request id, previousRequestId: no previous request id, message: a message from tom to laura", LogLevel.Error, _ex);
        }

        [Fact]
        public void should_log_critical()
        {
            _logger.LogCritical($"a message from {_a} to {_b}", _ex);

            ThenLevelIsLogged("requestId: no request id, previousRequestId: no previous request id, message: a message from tom to laura", LogLevel.Critical, _ex);
        }

        private void ThenLevelIsLogged(string expected, LogLevel expectedLogLevel, Exception ex = null)
        {
            _coreLogger.Verify(
                x => x.Log(
                    expectedLogLevel,
                    default(EventId),
                    expected,
                    ex,
                    It.IsAny<Func<string, Exception, string>>()), Times.Once);
        }
    }
}

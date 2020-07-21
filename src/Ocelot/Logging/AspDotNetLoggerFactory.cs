using Microsoft.Extensions.Logging;
using Ocelot.Infrastructure.RequestData;

namespace Ocelot.Logging
{
    public class AspDotNetLoggerFactory : IOcelotLoggerFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IRequestScopedDataRepository _scopedDataRepository;

        public AspDotNetLoggerFactory(ILoggerFactory loggerFactory, IRequestScopedDataRepository scopedDataRepository)
        {
            _loggerFactory = loggerFactory;
            _scopedDataRepository = scopedDataRepository;
        }

        public IOcelotLogger CreateLogger<T>()
        {
            var logger = _loggerFactory.CreateLogger<T>();
            return new AspDotNetLogger(logger, _scopedDataRepository);
        }
    }
}
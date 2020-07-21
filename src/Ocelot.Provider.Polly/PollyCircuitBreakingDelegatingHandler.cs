using Ocelot.Logging;
using Polly;
using Polly.CircuitBreaker;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Ocelot.Provider.Polly
{
    public class PollyCircuitBreakingDelegatingHandler : DelegatingHandler
    {
        private readonly PollyQoSProvider _qoSProvider;
        private readonly IOcelotLogger _logger;

        public PollyCircuitBreakingDelegatingHandler(
            PollyQoSProvider qoSProvider,
            IOcelotLoggerFactory loggerFactory)
        {
            _qoSProvider = qoSProvider;
            _logger = loggerFactory.CreateLogger<PollyCircuitBreakingDelegatingHandler>();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                return await Policy
                    .WrapAsync(_qoSProvider.CircuitBreaker.Policies)
                    .ExecuteAsync(() => base.SendAsync(request, cancellationToken));
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogError($"Reached to allowed number of exceptions. Circuit is open", ex);
                throw;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Error in CircuitBreakingDelegatingHandler.SendAync", ex);
                throw;
            }
        }
    }
}

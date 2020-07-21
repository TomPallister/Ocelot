namespace Ocelot.Requester
{
    using Logging;
    using Ocelot.Infrastructure.RequestData;
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class OcelotHttpTracingHandler : DelegatingHandler, ITracingHandler
    {
        private readonly ITracer _tracer;
        private readonly IRequestScopedDataRepository _repo;

        public OcelotHttpTracingHandler(
            ITracer tracer,
            IRequestScopedDataRepository repo,
            HttpMessageHandler httpMessageHandler = null)
        {
            _repo = repo;
            _tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
            InnerHandler = httpMessageHandler ?? new HttpClientHandler();
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return _tracer.SendAsync(request, cancellationToken, x => _repo.Add("TraceId", x), (r, c) => base.SendAsync(r, c));
        }
    }
}

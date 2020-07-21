namespace Ocelot.Tracing.Butterfly
{
    using global::Butterfly.Client.AspNetCore;
    using global::Butterfly.Client.Tracing;
    using global::Butterfly.OpenTracing;
    using Infrastructure.Extensions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class ButterflyTracer : DelegatingHandler, Logging.ITracer
    {
        private readonly IServiceTracer _tracer;
        private const string PrefixSpanId = "ot-spanId";

        public ButterflyTracer(IServiceProvider services)
        {
            _tracer = services.GetService<IServiceTracer>();
        }

        public void Event(HttpContext httpContext, string @event)
        {
            // todo - if the user isnt using tracing the code gets here and will blow up on
            // _tracer.Tracer.TryExtract..
            if (_tracer == null)
            {
                return;
            }

            var span = httpContext.GetSpan();

            if (span == null)
            {
                var spanBuilder = new SpanBuilder($"server {httpContext.Request.Method} {httpContext.Request.Path}");
                if (_tracer.Tracer.TryExtract(out var spanContext, httpContext.Request.Headers, (c, k) => c[k].GetValue(),
                    c => c.Select(x => new KeyValuePair<string, string>(x.Key, x.Value.GetValue())).GetEnumerator()))
                {
                    spanBuilder.AsChildOf(spanContext);
                }

                span = _tracer.Start(spanBuilder);
                httpContext.SetSpan(span);
            }

            span?.Log(LogField.CreateNew().Event(@event));
        }

        public Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken,
            Action<string> addTraceIdToRepo,
            Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> baseSendAsync)
        {
            return _tracer.ChildTraceAsync($"httpclient {request.Method}", DateTimeOffset.UtcNow, span => TracingSendAsync(span, request, cancellationToken, addTraceIdToRepo, baseSendAsync));
        }

        protected virtual async Task<HttpResponseMessage> TracingSendAsync(
            ISpan span,
            HttpRequestMessage request,
            CancellationToken cancellationToken,
            Action<string> addTraceIdToRepo,
            Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> baseSendAsync)
        {
            if (request.Headers.Contains(PrefixSpanId))
            {
                request.Headers.Remove(PrefixSpanId);
                request.Headers.TryAddWithoutValidation(PrefixSpanId, span.SpanContext.SpanId);
            }

            addTraceIdToRepo(span.SpanContext.TraceId);

            span.Tags.Client().Component("HttpClient")
                .HttpMethod(request.Method.Method)
                .HttpUrl(request.RequestUri.OriginalString)
                .HttpHost(request.RequestUri.Host)
                .HttpPath(request.RequestUri.PathAndQuery)
                .PeerAddress(request.RequestUri.OriginalString)
                .PeerHostName(request.RequestUri.Host)
                .PeerPort(request.RequestUri.Port);

            _tracer.Tracer.Inject(span.SpanContext, request.Headers, (c, k, v) =>
            {
                if (!c.Contains(k))
                {
                    c.Add(k, v);
                }
            });

            span.Log(LogField.CreateNew().ClientSend());

            var responseMessage = await baseSendAsync(request, cancellationToken);

            span.Log(LogField.CreateNew().ClientReceive());

            return responseMessage;
        }
    }
}

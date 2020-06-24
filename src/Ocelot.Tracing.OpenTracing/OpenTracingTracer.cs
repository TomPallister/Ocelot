namespace Ocelot.Tracing.OpenTracing
{
    using global::OpenTracing;
    using global::OpenTracing.Propagation;
    using global::OpenTracing.Tag;
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    class OpenTracingTracer : Logging.ITracer
    {
        private readonly ITracer _tracer;

        public OpenTracingTracer(ITracer tracer)
        {
           _tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
        }

        public void Event(HttpContext httpContext, string @event)
        {
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, 
            CancellationToken cancellationToken,
            Action<string> addTraceIdToRepo, 
            Func<HttpRequestMessage, 
            CancellationToken, 
            Task<HttpResponseMessage>> baseSendAsync)
        {
            using (IScope scope = _tracer.BuildSpan(request.RequestUri.AbsoluteUri).StartActive(finishSpanOnDispose: true))
            {
                var span = scope.Span;

                span.SetTag(Tags.SpanKind, Tags.SpanKindClient)
                    .SetTag(Tags.HttpMethod, request.Method.Method)
                    .SetTag(Tags.HttpUrl, request.RequestUri.OriginalString);

                addTraceIdToRepo(span.Context.SpanId);

                var headers = new Dictionary<string, string>();

                _tracer.Inject(span.Context, BuiltinFormats.HttpHeaders, new TextMapInjectAdapter(headers));

                foreach (var item in headers)
                {
                    request.Headers.Add(item.Key, item.Value);
                }

                try
                {
                    var response = await baseSendAsync(request, cancellationToken);

                    span.SetTag(Tags.HttpStatus, (int)response.StatusCode);

                    return response;
                }
                catch (HttpRequestException ex)
                {
                    Tags.Error.Set(scope.Span, true);

                    span.Log(new Dictionary<string, object>(3)
                        {
                            { LogFields.Event, Tags.Error.Key },
                            { LogFields.ErrorKind, ex.GetType().Name },
                            { LogFields.ErrorObject, ex }
                        });
                    throw;
                }
            }
        }
    }
}

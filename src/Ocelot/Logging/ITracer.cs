namespace Ocelot.Logging
{
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ITracer
    {
        void Event(HttpContext httpContext, string @event);

        Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken,
            Action<string> addTraceIdToRepo,
            Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> baseSendAsync);
    }
}

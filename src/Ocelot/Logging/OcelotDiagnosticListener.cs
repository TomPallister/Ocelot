namespace Ocelot.Logging
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DiagnosticAdapter;
    using System;

    public class OcelotDiagnosticListener
    {
        private readonly IOcelotLogger _logger;
        private readonly ITracer _tracer;

        public OcelotDiagnosticListener(IOcelotLoggerFactory factory, IServiceProvider serviceProvider)
        {
            _logger = factory.CreateLogger<OcelotDiagnosticListener>();
            _tracer = serviceProvider.GetService<ITracer>();
        }

        [DiagnosticName("Microsoft.AspNetCore.MiddlewareAnalysis.MiddlewareStarting")]
        public virtual void OnMiddlewareStarting(HttpContext httpContext, string name)
        {
            _logger.LogTrace($"MiddlewareStarting: {name}; {httpContext.Request.Path}");
            Event(httpContext, $"MiddlewareStarting: {name}; {httpContext.Request.Path}");
        }

        [DiagnosticName("Microsoft.AspNetCore.MiddlewareAnalysis.MiddlewareException")]
        public virtual void OnMiddlewareException(Exception exception, string name)
        {
            _logger.LogTrace($"MiddlewareException: {name}; {exception.Message};");
        }

        [DiagnosticName("Microsoft.AspNetCore.MiddlewareAnalysis.MiddlewareFinished")]
        public virtual void OnMiddlewareFinished(HttpContext httpContext, string name)
        {
            _logger.LogTrace($"MiddlewareFinished: {name}; {httpContext.Response.StatusCode}");
            Event(httpContext, $"MiddlewareFinished: {name}; {httpContext.Response.StatusCode}");
        }

        private void Event(HttpContext httpContext, string @event)
        {
            _tracer?.Event(httpContext, @event);
        }
    }
}

namespace Ocelot.Errors.Middleware
{
    using Ocelot.Configuration;
    using Ocelot.Infrastructure.RequestData;
    using Ocelot.Logging;
    using Ocelot.Middleware;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Ocelot.DownstreamRouteFinder.Middleware;

    /// <summary>
    /// Catches all unhandled exceptions thrown by middleware, logs and returns a 500.
    /// </summary>
    public class ExceptionHandlerMiddleware : OcelotMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IRequestScopedDataRepository _repo;

        public ExceptionHandlerMiddleware(RequestDelegate next,
            IOcelotLoggerFactory loggerFactory,
            IRequestScopedDataRepository repo)
                : base(loggerFactory.CreateLogger<ExceptionHandlerMiddleware>())
        {
            _next = next;
            _repo = repo;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                httpContext.RequestAborted.ThrowIfCancellationRequested();

                var internalConfiguration = httpContext.Items.IInternalConfiguration();

                TrySetGlobalRequestId(httpContext, internalConfiguration);

                Logger.LogDebug("ocelot pipeline started");

                await _next.Invoke(httpContext);
            }
            catch (OperationCanceledException) when (httpContext.RequestAborted.IsCancellationRequested)
            {
                Logger.LogDebug("operation canceled");
                if (!httpContext.Response.HasStarted)
                {
                    httpContext.Response.StatusCode = 499;
                }
            }
            catch (Exception e)
            {
                Logger.LogDebug("error calling middleware");

                var message = CreateMessage(httpContext, e);

                Logger.LogError(message, e);

                SetInternalServerErrorOnResponse(httpContext);
            }

            Logger.LogDebug("ocelot pipeline finished");
        }

        private void TrySetGlobalRequestId(HttpContext httpContext, IInternalConfiguration configuration)
        {
            var key = configuration.RequestId;

            if (!string.IsNullOrEmpty(key) && httpContext.Request.Headers.TryGetValue(key, out var upstreamRequestIds))
            {
                httpContext.TraceIdentifier = upstreamRequestIds.First();
            }

            _repo.Add("RequestId", httpContext.TraceIdentifier);
        }

        private void SetInternalServerErrorOnResponse(HttpContext httpContext)
        {
            if (!httpContext.Response.HasStarted)
            {
                httpContext.Response.StatusCode = 500;
            }
        }

        private string CreateMessage(HttpContext httpContext, Exception e)
        {
            var message =
                $"Exception caught in global error handler, exception message: {e.Message}, exception stack: {e.StackTrace}";

            if (e.InnerException != null)
            {
                message =
                    $"{message}, inner exception message {e.InnerException.Message}, inner exception stack {e.InnerException.StackTrace}";
            }

            return $"{message} RequestId: {httpContext.TraceIdentifier}";
        }
    }
}

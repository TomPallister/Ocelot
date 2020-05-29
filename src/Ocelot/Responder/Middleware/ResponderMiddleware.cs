namespace Ocelot.Responder.Middleware
{
    using Microsoft.AspNetCore.Http;
    using Ocelot.DownstreamRouteFinder.Middleware;
    using Ocelot.Errors;
    using Ocelot.Infrastructure.Extensions;
    using Ocelot.Logging;
    using Ocelot.Middleware;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Completes and returns the request and request body, if any pipeline errors occured then sets the appropriate HTTP status code instead.
    /// </summary>
    public class ResponderMiddleware : OcelotMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHttpResponder _responder;
        private readonly IErrorsToHttpStatusCodeMapper _codeMapper;

        public ResponderMiddleware(RequestDelegate next,
            IHttpResponder responder,
            IOcelotLoggerFactory loggerFactory,
            IErrorsToHttpStatusCodeMapper codeMapper
           )
            : base(loggerFactory.CreateLogger<ResponderMiddleware>())
        {
            _next = next;
            _responder = responder;
            _codeMapper = codeMapper;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            await _next.Invoke(httpContext);

            var errors = httpContext.Items.Errors();
            // todo check errors is ok
            if (errors.Count > 0)
            {
                Logger.LogWarning($"{errors.ToErrorString()} errors found in {MiddlewareName}. Setting error response for request path:{httpContext.Request.Path}, request method: {httpContext.Request.Method}");

                SetErrorResponse(httpContext, errors);
            }
            else
            {
                Logger.LogDebug("no pipeline errors, setting and returning completed response");

                var downstreamResponse = httpContext.Items.DownstreamResponse();

                await _responder.SetResponseOnHttpContext(httpContext, downstreamResponse);
            }
        }

        private void SetErrorResponse(HttpContext context, List<Error> errors)
        {
            //todo - refactor this all teh way down because its shit
            var statusCode = _codeMapper.Map(errors);
            _responder.SetErrorResponseOnContext(context, statusCode);

            if (errors.Any(e => e.Code == OcelotErrorCode.QuotaExceededError))
            {
                var downstreamResponse = context.Items.DownstreamResponse();
                _responder.SetErrorResponseOnContext(context, downstreamResponse);
            }
        }
    }
}

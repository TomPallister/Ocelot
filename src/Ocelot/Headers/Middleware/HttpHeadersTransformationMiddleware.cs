namespace Ocelot.Headers.Middleware
{
    using Microsoft.AspNetCore.Http;
    using Ocelot.DownstreamRouteFinder.Middleware;
    using Ocelot.Logging;
    using Ocelot.Middleware;
    using System.Threading.Tasks;

    public class HttpHeadersTransformationMiddleware : OcelotMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHttpContextRequestHeaderReplacer _preReplacer;
        private readonly IHttpResponseHeaderReplacer _postReplacer;
        private readonly IAddHeadersToResponse _addHeadersToResponse;
        private readonly IAddHeadersToRequest _addHeadersToRequest;

        public HttpHeadersTransformationMiddleware(RequestDelegate next,
            IOcelotLoggerFactory loggerFactory,
            IHttpContextRequestHeaderReplacer preReplacer,
            IHttpResponseHeaderReplacer postReplacer,
            IAddHeadersToResponse addHeadersToResponse,
            IAddHeadersToRequest addHeadersToRequest
            )
                : base(loggerFactory.CreateLogger<HttpHeadersTransformationMiddleware>())
        {
            _addHeadersToResponse = addHeadersToResponse;
            _addHeadersToRequest = addHeadersToRequest;
            _next = next;
            _postReplacer = postReplacer;
            _preReplacer = preReplacer;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var downstreamRoute = httpContext.Items.DownstreamRoute();

            var preFAndRs = downstreamRoute.UpstreamHeadersFindAndReplace;

            //todo - this should be on httprequestmessage not httpcontext?
            _preReplacer.Replace(httpContext, preFAndRs);

            _addHeadersToRequest.SetHeadersOnDownstreamRequest(downstreamRoute.AddHeadersToUpstream, httpContext);

            await _next.Invoke(httpContext);

            // todo check errors is ok
            //todo put this check on the base class?
            if (httpContext.Items.Errors().Count > 0)
            {
                return;
            }

            var postFAndRs = downstreamRoute.DownstreamHeadersFindAndReplace;

            _postReplacer.Replace(httpContext, postFAndRs);

            var downstreamResponse = httpContext.Items.DownstreamResponse();

            _addHeadersToResponse.Add(downstreamRoute.AddHeadersToDownstream, downstreamResponse);
        }
    }
}

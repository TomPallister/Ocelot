namespace Ocelot.Requester
{
    using Ocelot.Configuration;
    using Microsoft.AspNetCore.Http;
    using Ocelot.Logging;
    using Ocelot.Middleware;
    using Ocelot.Responses;
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Ocelot.DownstreamRouteFinder.Middleware;

    public class HttpClientHttpRequester : IHttpRequester
    {
        private readonly IHttpClientCache _cacheHandlers;
        private readonly IOcelotLogger _logger;
        private readonly IDelegatingHandlerHandlerFactory _factory;
        private readonly IExceptionToErrorMapper _mapper;

        public HttpClientHttpRequester(IOcelotLoggerFactory loggerFactory,
            IHttpClientCache cacheHandlers,
            IDelegatingHandlerHandlerFactory factory,
            IExceptionToErrorMapper mapper)
        {
            _logger = loggerFactory.CreateLogger<HttpClientHttpRequester>();
            _cacheHandlers = cacheHandlers;
            _factory = factory;
            _mapper = mapper;
        }

        public async Task<Response<HttpResponseMessage>> GetResponse(HttpContext httpContext)
        {
            var builder = new HttpClientBuilder(_factory, _cacheHandlers, _logger);

            var downstreamRoute = httpContext.Items.DownstreamRoute();

            var downstreamRequest = httpContext.Items.DownstreamRequest();

            var httpClient = builder.Create(downstreamRoute);

            try
            {
                var response = await httpClient.SendAsync(downstreamRequest.ToHttpRequestMessage(), httpContext.RequestAborted);
                return new OkResponse<HttpResponseMessage>(response);
            }
            catch (Exception exception)
            {
                var error = _mapper.Map(exception);
                return new ErrorResponse<HttpResponseMessage>(error);
            }
            finally
            {
                builder.Save();
            }
        }
    }
}

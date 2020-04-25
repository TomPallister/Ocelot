namespace Ocelot.Request.Middleware
{
    using Ocelot.Logging;
    using Ocelot.Middleware;
    using Ocelot.Request.Creator;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Ocelot.DownstreamRouteFinder.Middleware;

    public class DownstreamRequestInitialiserMiddleware : OcelotMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Mapper.IRequestMapper _requestMapper;
        private readonly IDownstreamRequestCreator _creator;

        public DownstreamRequestInitialiserMiddleware(RequestDelegate next,
            IOcelotLoggerFactory loggerFactory,
            Mapper.IRequestMapper requestMapper,
            IDownstreamRequestCreator creator)
                : base(loggerFactory.CreateLogger<DownstreamRequestInitialiserMiddleware>())
        {
            _next = next;
            _requestMapper = requestMapper;
            _creator = creator;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var downstreamReRoute = httpContext.Items.DownstreamReRoute();

            var httpRequestMessage = await _requestMapper.Map(httpContext.Request, downstreamReRoute);

            if (httpRequestMessage.IsError)
            {
                httpContext.Items.SetErrors(httpRequestMessage.Errors);
                return;
            }

            var downstreamRequest = _creator.Create(httpRequestMessage.Data);

            httpContext.Items.SetDownstreamRequest(downstreamRequest);

            await _next.Invoke(httpContext);
        }
    }
}

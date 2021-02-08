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
            var downstreamRoute = httpContext.Items.DownstreamRoute();

            var httpRequestMessage = _requestMapper.Map(httpContext.Request, downstreamRoute);

            if (httpRequestMessage.IsError)
            {
                httpContext.Items.UpsertErrors(httpRequestMessage.Errors);
                return;
            }

            var downstreamRequest = _creator.Create(httpRequestMessage.Data);

            httpContext.Items.UpsertDownstreamRequest(downstreamRequest);

            await _next.Invoke(httpContext);
        }
    }
}

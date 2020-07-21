namespace Ocelot.Configuration.Creator
{
    using Logging;
    using Microsoft.Extensions.DependencyInjection;
    using Ocelot.Configuration.File;
    using System;

    public class HttpHandlerOptionsCreator : IHttpHandlerOptionsCreator
    {
        private readonly ITracer _tracer;

        public HttpHandlerOptionsCreator(IServiceProvider services)
        {
            _tracer = services.GetService<ITracer>();
        }

        public HttpHandlerOptions Create(FileHttpHandlerOptions options)
        {
            var useTracing = _tracer != null && options.UseTracing;

            //be sure that maxConnectionPerServer is in correct range of values
            int maxConnectionPerServer = (options.MaxConnectionsPerServer > 0) ? maxConnectionPerServer = options.MaxConnectionsPerServer : maxConnectionPerServer = int.MaxValue;

            return new HttpHandlerOptions(options.AllowAutoRedirect,
                options.UseCookieContainer, useTracing, options.UseProxy, maxConnectionPerServer);
        }
    }
}

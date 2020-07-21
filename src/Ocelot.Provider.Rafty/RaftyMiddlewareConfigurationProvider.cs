namespace Ocelot.Provider.Rafty
{
    using global::Rafty.Concensus.Node;
    using global::Rafty.Infrastructure;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Middleware;
    using System.Threading.Tasks;

    public static class RaftyMiddlewareConfigurationProvider
    {
        public static OcelotMiddlewareConfigurationDelegate Get = builder =>
        {
            if (UsingRafty(builder))
            {
                SetUpRafty(builder);
            }

            return Task.CompletedTask;
        };

        private static bool UsingRafty(IApplicationBuilder builder)
        {
            var node = builder.ApplicationServices.GetService<INode>();
            if (node != null)
            {
                return true;
            }

            return false;
        }

        private static void SetUpRafty(IApplicationBuilder builder)
        {
            var applicationLifetime = builder.ApplicationServices.GetService<IApplicationLifetime>();
            applicationLifetime.ApplicationStopping.Register(() => OnShutdown(builder));
            var node = builder.ApplicationServices.GetService<INode>();
            var nodeId = builder.ApplicationServices.GetService<NodeId>();
            node.Start(nodeId);
        }

        private static void OnShutdown(IApplicationBuilder app)
        {
            var node = app.ApplicationServices.GetService<INode>();
            node.Stop();
        }
    }
}

namespace Ocelot.Middleware
{
    using Microsoft.AspNetCore.Builder;
    using System.Threading.Tasks;

    public delegate Task OcelotMiddlewareConfigurationDelegate(IApplicationBuilder builder);
}

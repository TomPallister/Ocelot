using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Ocelot.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IOcelotBuilder AddOcelot(this IServiceCollection services)
        {
            var configuration = services.BuildServiceProvider()
                .GetRequiredService<IConfiguration>();
            return new OcelotBuilder(services, configuration);
        }

        public static IOcelotBuilder AddOcelot(this IServiceCollection services, IConfiguration configuration)
        {
            return new OcelotBuilder(services, configuration);
        }
    }
}

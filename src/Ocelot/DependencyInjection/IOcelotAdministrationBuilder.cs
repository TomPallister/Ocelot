namespace Ocelot.DependencyInjection
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public interface IOcelotAdministrationBuilder
    {
        IServiceCollection Services { get; }
        IConfiguration ConfigurationRoot { get; }
    }
}

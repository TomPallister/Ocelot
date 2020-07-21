namespace Ocelot.Provider.Rafty
{
    using Configuration.Setter;
    using DependencyInjection;
    using global::Rafty.Concensus.Node;
    using global::Rafty.FiniteStateMachine;
    using global::Rafty.Infrastructure;
    using global::Rafty.Log;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    public static class OcelotAdministrationBuilderExtensions
    {
        public static IOcelotAdministrationBuilder AddRafty(this IOcelotAdministrationBuilder builder)
        {
            var settings = new InMemorySettings(4000, 6000, 100, 10000);
            builder.Services.RemoveAll<IFileConfigurationSetter>();
            builder.Services.AddSingleton<IFileConfigurationSetter, RaftyFileConfigurationSetter>();
            builder.Services.AddSingleton<ILog, SqlLiteLog>();
            builder.Services.AddSingleton<IFiniteStateMachine, OcelotFiniteStateMachine>();
            builder.Services.AddSingleton<ISettings>(settings);
            builder.Services.AddSingleton<IPeersProvider, FilePeersProvider>();
            builder.Services.AddSingleton<INode, Node>();
            builder.Services.Configure<FilePeers>(builder.ConfigurationRoot);
            return builder;
        }
    }
}

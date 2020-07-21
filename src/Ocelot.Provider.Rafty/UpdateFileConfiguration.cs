namespace Ocelot.Provider.Rafty
{
    using Configuration.File;
    using global::Rafty.FiniteStateMachine;

    public class UpdateFileConfiguration : ICommand
    {
        public UpdateFileConfiguration(FileConfiguration configuration)
        {
            Configuration = configuration;
        }

        public FileConfiguration Configuration { get; private set; }
    }
}

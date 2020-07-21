namespace Ocelot.Configuration.Repository
{
    public class InMemoryFileConfigurationPollerOptions : IFileConfigurationPollerOptions
    {
        public int Delay => 1000;
    }
}

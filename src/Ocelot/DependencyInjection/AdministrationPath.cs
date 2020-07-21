namespace Ocelot.DependencyInjection
{
    public class AdministrationPath : IAdministrationPath
    {
        public AdministrationPath(string path)
        {
            Path = path;
        }

        public string Path { get; private set; }
    }
}

using Ocelot.Configuration.File;

namespace Ocelot.Configuration.Creator
{
    public interface ISecurityOptionsCreator
    {
        SecurityOptions Create(FileSecurityOptions securityOptions);
    }
}

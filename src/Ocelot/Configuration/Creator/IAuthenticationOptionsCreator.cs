using Ocelot.Configuration.File;

namespace Ocelot.Configuration.Creator
{
    public interface IAuthenticationOptionsCreator
    {
        AuthenticationOptions Create(FileAuthenticationOptions routeAuthOptions, FileAuthenticationOptions globalConfAuthOptions);
    }
}

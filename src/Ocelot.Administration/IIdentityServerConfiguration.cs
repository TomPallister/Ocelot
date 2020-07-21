namespace Ocelot.Administration
{
    using System.Collections.Generic;

    public interface IIdentityServerConfiguration
    {
        string ApiName { get; }
        string ApiSecret { get; }
        bool RequireHttps { get; }
        List<string> AllowedScopes { get; }
        string CredentialsSigningCertificateLocation { get; }
        string CredentialsSigningCertificatePassword { get; }
    }
}

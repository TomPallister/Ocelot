namespace Ocelot.Administration
{
    using System.Collections.Generic;

    public class IdentityServerConfiguration : IIdentityServerConfiguration
    {
        public IdentityServerConfiguration(
            string apiName,
            bool requireHttps,
            string apiSecret,
            List<string> allowedScopes,
            string credentialsSigningCertificateLocation,
            string credentialsSigningCertificatePassword)
        {
            ApiName = apiName;
            RequireHttps = requireHttps;
            ApiSecret = apiSecret;
            AllowedScopes = allowedScopes;
            CredentialsSigningCertificateLocation = credentialsSigningCertificateLocation;
            CredentialsSigningCertificatePassword = credentialsSigningCertificatePassword;
        }

        public string ApiName { get; }
        public bool RequireHttps { get; }
        public List<string> AllowedScopes { get; }
        public string ApiSecret { get; }
        public string CredentialsSigningCertificateLocation { get; }
        public string CredentialsSigningCertificatePassword { get; }
    }
}

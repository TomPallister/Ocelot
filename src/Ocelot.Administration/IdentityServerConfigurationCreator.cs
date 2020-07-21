using System;
using System.Collections.Generic;

namespace Ocelot.Administration
{
    public static class IdentityServerConfigurationCreator
    {
        public static IdentityServerConfiguration GetIdentityServerConfiguration(string secret)
        {
            var credentialsSigningCertificateLocation = Environment.GetEnvironmentVariable("OCELOT_CERTIFICATE");
            var credentialsSigningCertificatePassword = Environment.GetEnvironmentVariable("OCELOT_CERTIFICATE_PASSWORD");

            return new IdentityServerConfiguration(
                "admin",
                false,
                secret,
                new List<string> { "admin", "openid", "offline_access" },
                credentialsSigningCertificateLocation,
                credentialsSigningCertificatePassword
            );
        }
    }
}

using Ocelot.DependencyInjection;
using IdentityServer4.AccessTokenValidation;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ocelot.Middleware;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Ocelot.Administration
{
    public static class OcelotBuilderExtensions
    {
        public static IOcelotAdministrationBuilder AddAdministration(this IOcelotBuilder builder, string path, string secret)
        {
            var administrationPath = new AdministrationPath(path);

            builder.Services.AddSingleton<OcelotMiddlewareConfigurationDelegate>(IdentityServerMiddlewareConfigurationProvider.Get);

            //add identity server for admin area
            var identityServerConfiguration = IdentityServerConfigurationCreator.GetIdentityServerConfiguration(secret);

            if (identityServerConfiguration != null)
            {
                AddIdentityServer(identityServerConfiguration, administrationPath, builder, builder.Configuration);
            }

            builder.Services.AddSingleton<IAdministrationPath>(administrationPath);
            return new OcelotAdministrationBuilder(builder.Services, builder.Configuration);
        }

        public static IOcelotAdministrationBuilder AddAdministration(this IOcelotBuilder builder, string path, Action<JwtBearerOptions> configureOptions)
        {
            var administrationPath = new AdministrationPath(path);
            builder.Services.AddSingleton<OcelotMiddlewareConfigurationDelegate>(IdentityServerMiddlewareConfigurationProvider.Get);

            if (configureOptions != null)
            {
                AddIdentityServer(configureOptions, builder);
            }

            builder.Services.AddSingleton<IAdministrationPath>(administrationPath);
            return new OcelotAdministrationBuilder(builder.Services, builder.Configuration);
        }

        private static void AddIdentityServer(Action<JwtBearerOptions> configOptions, IOcelotBuilder builder)
        {
            builder.Services
                .AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddJwtBearer("Bearer", configOptions);
        }

        private static void AddIdentityServer(IIdentityServerConfiguration identityServerConfiguration, IAdministrationPath adminPath, IOcelotBuilder builder, IConfiguration configuration)
        {
            builder.Services.TryAddSingleton<IIdentityServerConfiguration>(identityServerConfiguration);
            var identityServerBuilder = builder.Services
                .AddIdentityServer(o =>
                {
                    o.IssuerUri = "Ocelot";
                    o.EmitStaticAudienceClaim = true;
                })
                .AddInMemoryApiScopes(ApiScopes(identityServerConfiguration))
                .AddInMemoryApiResources(Resources(identityServerConfiguration))
                .AddInMemoryClients(Client(identityServerConfiguration));

            var urlFinder = new BaseUrlFinder(configuration);
            var baseSchemeUrlAndPort = urlFinder.Find();
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            builder.Services
                .AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = baseSchemeUrlAndPort + adminPath.Path;
                    options.RequireHttpsMetadata = identityServerConfiguration.RequireHttps;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false,
                    };
                });

            //todo - refactor naming..
            if (string.IsNullOrEmpty(identityServerConfiguration.CredentialsSigningCertificateLocation) || string.IsNullOrEmpty(identityServerConfiguration.CredentialsSigningCertificatePassword))
            {
                identityServerBuilder.AddDeveloperSigningCredential();
            }
            else
            {
                //todo - refactor so calls method?
                var cert = new X509Certificate2(identityServerConfiguration.CredentialsSigningCertificateLocation, identityServerConfiguration.CredentialsSigningCertificatePassword, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
                identityServerBuilder.AddSigningCredential(cert);
            }
        }

        private static IEnumerable<ApiScope> ApiScopes(IIdentityServerConfiguration identityServerConfiguration)
        {
            return identityServerConfiguration.AllowedScopes.Select(s => new ApiScope(s));
        }

        private static List<ApiResource> Resources(IIdentityServerConfiguration identityServerConfiguration)
        {
            return new List<ApiResource>
            {
                new ApiResource(identityServerConfiguration.ApiName, identityServerConfiguration.ApiName)
                {
                    ApiSecrets = new List<Secret>
                    {
                        new Secret
                        {
                            Value = identityServerConfiguration.ApiSecret.Sha256(),
                        },
                    },
                },
            };
        }

        private static List<Client> Client(IIdentityServerConfiguration identityServerConfiguration)
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = identityServerConfiguration.ApiName,
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = new List<Secret> {new Secret(identityServerConfiguration.ApiSecret.Sha256())},
                    AllowedScopes = identityServerConfiguration.AllowedScopes,
                },
            };
        }
    }
}

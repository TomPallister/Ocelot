using System.Net.Http;
using Microsoft.AspNetCore.Authentication;

namespace Ocelot.Authentication.Extensions.ApiKey
{
    public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        public const string DefaultScheme = "API Key";
        public string Scheme => DefaultScheme;
        public string AuthenticationType = DefaultScheme;
        public string Authority { get; set; }
        public HttpMethod Method { get; set; } = HttpMethod.Get;
        public string ApiKeyHeaderName { get; set; } = "X-Api-Key";
        public string ApiKeyQueryName { get; set; } = "key";
    }
}

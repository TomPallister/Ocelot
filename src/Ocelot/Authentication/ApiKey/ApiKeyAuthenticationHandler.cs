using System;
using System.Security.Claims;
using System.Net.Http;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ocelot.Logging;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Ocelot.Authentication.Extensions.ApiKey
{
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
    {
        private const string ProblemContentType = "application/problem+json";

        private readonly IOcelotLogger _logger;

        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<ApiKeyAuthenticationOptions> options,
            IOcelotLoggerFactory loggerFactory,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock) : base(options, logger, encoder, clock)
        {
            _logger = loggerFactory.CreateLogger<ApiKeyAuthenticationHandler>();
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var headerPresent = true;
            var queryPresent = true;

            if (!Request.Headers.TryGetValue(Options.ApiKeyHeaderName, out var apiKeyHeaderValues))
            {
                headerPresent = false;
            }

            if (!Request.Query.TryGetValue(Options.ApiKeyQueryName, out var apiKeyQueryValues))
            {
                queryPresent = false;
            }

            if (!headerPresent && !queryPresent)
            {
                _logger.LogError("No Api Key present in header or query parameters", new Exception());
                return AuthenticateResult.NoResult();
            }

            var values = apiKeyHeaderValues.Concat(apiKeyQueryValues);
            var providedApiKey = values.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(providedApiKey))
            {
                return AuthenticateResult.NoResult();
            }

            using var client = new HttpClient();

            StringContent body = null;
            string url = Options.Authority;

            if (Options.Method == HttpMethod.Get)
            {
                url = $"{url}?key={providedApiKey}";
            }
            else
            {
                var bodyData = new
                {
                    key = providedApiKey,
                };
                body = new StringContent(JsonConvert.SerializeObject(bodyData), Encoding.UTF8, "application/json");
            }

            var message = new HttpRequestMessage(Options.Method, url)
            {
                Content = body,
            };

            var response = await client.SendAsync(message);

            if (!response.IsSuccessStatusCode)
            {
                return AuthenticateResult.Fail("Invalid API Key provided.");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<ApiKeyValidationResponse>(responseBody);

            var claims = new List<Claim>
                {
                    new Claim("Owner", responseObject.Owner),
                };

            claims.AddRange(responseObject.Roles.Select(role => new Claim("Role", role)));

            var identity = new ClaimsIdentity(claims, Options.AuthenticationType);
            var identities = new List<ClaimsIdentity> { identity };
            var principal = new ClaimsPrincipal(identities);
            var ticket = new AuthenticationTicket(principal, Options.Scheme);

            return AuthenticateResult.Success(ticket);
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = 401;
            Response.ContentType = ProblemContentType;

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Type = $"https://httpstatuses.com/401",
                Detail = "Unauthorized",
            };

            await Response.WriteAsync(JsonConvert.SerializeObject(problemDetails));
        }

        protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = 403;
            Response.ContentType = ProblemContentType;

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Type = $"https://httpstatuses.com/403",
                Detail = "Forbidden",
            };

            await Response.WriteAsync(JsonConvert.SerializeObject(problemDetails));
        }
    }
}

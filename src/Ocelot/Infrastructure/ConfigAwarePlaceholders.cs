namespace Ocelot.Infrastructure
{
    using System;
    using System.Text.RegularExpressions;
    using Microsoft.Extensions.Configuration;
    using Request.Middleware;
    using Responses;

    public class ConfigAwarePlaceholders : IPlaceholders
    {
        private readonly IConfiguration _configuration;
        private readonly IPlaceholders _placeholders;
        private static readonly Regex _regex = new Regex(@"[{}]", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));

        public ConfigAwarePlaceholders(IConfiguration configuration, IPlaceholders placeholders)
        {
            _configuration = configuration;
            _placeholders = placeholders;
        }
        
        public Response<string> Get(string key)
        {
            var placeholderResponse = _placeholders.Get(key);

            if (!placeholderResponse.IsError)
            {
                return placeholderResponse;
            }

            return GetFromConfig(CleanKey(key));
        }

        public Response<string> Get(string key, DownstreamRequest request)
        {
            var placeholderResponse = _placeholders.Get(key, request);
            
            if (!placeholderResponse.IsError)
            {
                return placeholderResponse;
            }

            return GetFromConfig(CleanKey(key));
        }

        public Response Add(string key, Func<Response<string>> func)
            => _placeholders.Add(key, func);

        public Response Remove(string key)
            => _placeholders.Remove(key);

        private string CleanKey(string key) 
            => _regex.Replace(key , string.Empty);

        private Response<string> GetFromConfig(string key)
        {
            var valueFromConfig = _configuration[key];
            return valueFromConfig == null
                ? (Response<string>) new ErrorResponse<string>(new CouldNotFindPlaceholderError(key))
                : new OkResponse<string>(valueFromConfig);
        }
    }
}

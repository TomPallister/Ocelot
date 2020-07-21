using Microsoft.Extensions.Primitives;
using Ocelot.Configuration;
using Ocelot.Infrastructure.Claims.Parser;
using Ocelot.Request.Middleware;
using Ocelot.Responses;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Ocelot.QueryStrings
{
    public class AddQueriesToRequest : IAddQueriesToRequest
    {
        private readonly IClaimsParser _claimsParser;

        public AddQueriesToRequest(IClaimsParser claimsParser)
        {
            _claimsParser = claimsParser;
        }

        public Response SetQueriesOnDownstreamRequest(List<ClaimToThing> claimsToThings, IEnumerable<Claim> claims, DownstreamRequest downstreamRequest)
        {
            var queryDictionary = ConvertQueryStringToDictionary(downstreamRequest.Query);

            foreach (var config in claimsToThings)
            {
                var value = _claimsParser.GetValue(claims, config.NewKey, config.Delimiter, config.Index);

                if (value.IsError)
                {
                    return new ErrorResponse(value.Errors);
                }

                var exists = queryDictionary.FirstOrDefault(x => x.Key == config.ExistingKey);

                if (!string.IsNullOrEmpty(exists.Key))
                {
                    queryDictionary[exists.Key] = value.Data;
                }
                else
                {
                    queryDictionary.Add(config.ExistingKey, value.Data);
                }
            }

            downstreamRequest.Query = ConvertDictionaryToQueryString(queryDictionary);

            return new OkResponse();
        }

        private Dictionary<string, StringValues> ConvertQueryStringToDictionary(string queryString)
        {
            var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers
                .ParseQuery(queryString);

            return query;
        }

        private string ConvertDictionaryToQueryString(Dictionary<string, StringValues> queryDictionary)
        {
            var builder = new StringBuilder();

            builder.Append("?");

            int outerCount = 0;

            foreach (var query in queryDictionary)
            {
                for (int innerCount = 0; innerCount < query.Value.Count; innerCount++)
                {
                    builder.Append($"{query.Key}={query.Value[innerCount]}");

                    if (innerCount < (query.Value.Count - 1))
                    {
                        builder.Append("&");
                    }
                }

                if (outerCount < (queryDictionary.Count - 1))
                {
                    builder.Append("&");
                }

                outerCount++;
            }

            return builder.ToString();
        }
    }
}

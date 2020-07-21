namespace Ocelot.Infrastructure.Claims.Parser
{
    using Microsoft.Extensions.Primitives;
    using Responses;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;

    public class ClaimsParser : IClaimsParser
    {
        public Response<string> GetValue(IEnumerable<Claim> claims, string key, string delimiter, int index)
        {
            var claimResponse = GetValue(claims, key);

            if (claimResponse.IsError)
            {
                return claimResponse;
            }

            if (string.IsNullOrEmpty(delimiter))
            {
                return claimResponse;
            }

            var splits = claimResponse.Data.Split(delimiter.ToCharArray());

            if (splits.Length <= index || index < 0)
            {
                return new ErrorResponse<string>(new CannotFindClaimError($"Cannot find claim for key: {key}, delimiter: {delimiter}, index: {index}"));
            }

            var value = splits[index];

            return new OkResponse<string>(value);
        }

        public Response<List<string>> GetValuesByClaimType(IEnumerable<Claim> claims, string claimType)
        {
            List<string> values = new List<string>();

            values.AddRange(claims.Where(x => x.Type == claimType).Select(x => x.Value).ToList());

            return new OkResponse<List<string>>(values);
        }

        private Response<string> GetValue(IEnumerable<Claim> claims, string key)
        {
            var claimValues = claims.Where(c => c.Type == key).Select(c => c.Value).ToArray();

            if (claimValues.Length > 0)
            {
                return new OkResponse<string>(new StringValues(claimValues).ToString());
            }

            return new ErrorResponse<string>(new CannotFindClaimError($"Cannot find claim for key: {key}"));
        }
    }
}

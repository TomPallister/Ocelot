using Ocelot.Errors;
using System.Collections.Generic;
using System.Linq;

namespace Ocelot.Infrastructure.Extensions
{
    public static class ErrorListExtensions
    {
        public static string ToErrorString(this List<Error> errors)
        {
            var listOfErrorStrings = errors.Select(x => "Error Code: " + x.Code.ToString() + " Message: " + x.Message);
            return string.Join(" ", listOfErrorStrings);
        }
    }
}

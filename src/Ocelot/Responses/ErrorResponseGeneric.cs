using Ocelot.Errors;
using System.Collections.Generic;

namespace Ocelot.Responses
{
#pragma warning disable SA1649 // File name must match first type name

    public class ErrorResponse<T> : Response<T>
#pragma warning restore SA1649 // File name must match first type name
    {
        public ErrorResponse(Error error)
            : base(new List<Error> { error })
        {
        }

        public ErrorResponse(List<Error> errors)
            : base(errors)
        {
        }
    }
}

using Ocelot.Errors;
using System.Collections.Generic;

namespace Ocelot.Responses
{
#pragma warning disable SA1649 // File name must match first type name

    public abstract class Response<T> : Response
#pragma warning restore SA1649 // File name must match first type name
    {
        protected Response(T data)
        {
            Data = data;
        }

        protected Response(List<Error> errors) : base(errors)
        {
        }

        public T Data { get; private set; }
    }
}

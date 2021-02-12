using Ocelot.Errors;
using System.Collections.Generic;

namespace Ocelot.Responses
{
    public abstract class Response
    {
        protected Response()
        {
            Errors = new List<Error>();
        }

        protected Response(List<Error> errors)
        {
            Errors = errors ?? new List<Error>();
        }

        public List<Error> Errors { get; }

        public bool IsError => Errors.Count > 0;
    }
}

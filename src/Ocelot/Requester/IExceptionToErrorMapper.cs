using Ocelot.Errors;
using System;

namespace Ocelot.Requester
{
    public interface IExceptionToErrorMapper
    {
        Error Map(Exception exception);
    }
}

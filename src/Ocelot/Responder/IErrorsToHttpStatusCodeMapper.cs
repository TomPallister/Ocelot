namespace Ocelot.Responder
{
    using System.Net;
    using Ocelot.Errors;
    using System.Collections.Generic;

    /// <summary>
    /// Map a list OceoltErrors to a single appropriate HTTP status code
    /// </summary>
    public interface IErrorsToHttpStatusCodeMapper
    {
        int Map(List<Error> errors);
    }
}

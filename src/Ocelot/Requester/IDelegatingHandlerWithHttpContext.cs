using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ocelot.Requester
{
    public interface IDelegatingHandlerWithHttpContext
    {
        HttpContext HttpContext { get; set; }
    }
}

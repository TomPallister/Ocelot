using System;
using System.Collections.Generic;

namespace Ocelot.Authentication.Extensions.ApiKey
{
    public class ApiKeyValidationResponse
    {
        public string Owner { get; set; }
        public IReadOnlyCollection<string> Roles { get; set; }
    }
}

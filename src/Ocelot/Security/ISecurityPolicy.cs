namespace Ocelot.Security
{
    using Microsoft.AspNetCore.Http;
    using Ocelot.Responses;
    using System.Threading.Tasks;
    using Ocelot.Configuration;

    public interface ISecurityPolicy
    {
        Task<Response> Security(DownstreamRoute downstreamRoute, HttpContext httpContext);
    }
}

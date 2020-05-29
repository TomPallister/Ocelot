namespace Ocelot.Requester
{
    using Microsoft.AspNetCore.Http;
    using Ocelot.Responses;
    using System.Net.Http;
    using System.Threading.Tasks;

    public interface IHttpRequester
    {
        Task<Response<HttpResponseMessage>> GetResponse(HttpContext httpContext);
    }
}

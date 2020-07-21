namespace Ocelot.Responder
{
    using Microsoft.AspNetCore.Http;
    using Ocelot.Middleware;
    using System.Threading.Tasks;

    public interface IHttpResponder
    {
        Task SetResponseOnHttpContext(HttpContext context, DownstreamResponse response);

        void SetErrorResponseOnContext(HttpContext context, int statusCode);

        Task SetErrorResponseOnContext(HttpContext context, DownstreamResponse response);
    }
}

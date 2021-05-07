namespace Ocelot.Requester
{
    using Microsoft.AspNetCore.Http;
    using Ocelot.Configuration;

    public interface IHttpClientBuilder
    {
        IHttpClient Create(DownstreamRoute downstreamRoute, HttpContext httpContext);

        void Save();
    }
}

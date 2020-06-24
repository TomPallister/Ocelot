namespace Ocelot.Request.Mapper
{
    using Microsoft.AspNetCore.Http;
    using Ocelot.Configuration;
    using Ocelot.Responses;
    using System.Net.Http;
    using System.Threading.Tasks;

    public interface IRequestMapper
    {
        Task<Response<HttpRequestMessage>> Map(HttpRequest request, DownstreamRoute downstreamRoute);
    }
}

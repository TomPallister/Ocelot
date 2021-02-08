namespace Ocelot.Request.Mapper
{
    using Microsoft.AspNetCore.Http;
    using Ocelot.Configuration;
    using Ocelot.Responses;
    using System.Net.Http;

    public interface IRequestMapper
    {
        Response<HttpRequestMessage> Map(HttpRequest request, DownstreamRoute downstreamRoute);
    }
}

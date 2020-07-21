namespace Ocelot.Request.Creator
{
    using Ocelot.Request.Middleware;
    using System.Net.Http;

    public interface IDownstreamRequestCreator
    {
        DownstreamRequest Create(HttpRequestMessage request);
    }
}

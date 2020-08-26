namespace Ocelot.Requester.QoS
{
    using Configuration;
    using Responses;
    using System.Net.Http;

    public interface IQoSFactory
    {
        Response<DelegatingHandler> Get(DownstreamRoute request);
    }
}

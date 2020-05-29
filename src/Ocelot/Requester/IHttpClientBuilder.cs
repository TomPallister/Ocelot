namespace Ocelot.Requester
{
    using Ocelot.Configuration;

    public interface IHttpClientBuilder
    {
        IHttpClient Create(DownstreamRoute downstreamRoute);

        void Save();
    }
}

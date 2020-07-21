namespace Ocelot.Requester
{
    using Configuration;
    using System;

    public interface IHttpClientCache
    {
        IHttpClient Get(DownstreamRoute key);

        void Set(DownstreamRoute key, IHttpClient handler, TimeSpan expirationTime);
    }
}

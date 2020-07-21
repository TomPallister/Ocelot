namespace Ocelot.Provider.Rafty
{
    using Administration;
    using Configuration.Repository;
    using global::Rafty.Concensus.Peers;
    using global::Rafty.Infrastructure;
    using Microsoft.Extensions.Options;
    using Middleware;
    using System.Collections.Generic;
    using System.Net.Http;

    public class FilePeersProvider : IPeersProvider
    {
        private readonly IOptions<FilePeers> _options;
        private readonly List<IPeer> _peers;
        private IBaseUrlFinder _finder;
        private IInternalConfigurationRepository _repo;
        private IIdentityServerConfiguration _identityServerConfig;

        public FilePeersProvider(IOptions<FilePeers> options, IBaseUrlFinder finder, IInternalConfigurationRepository repo, IIdentityServerConfiguration identityServerConfig)
        {
            _identityServerConfig = identityServerConfig;
            _repo = repo;
            _finder = finder;
            _options = options;
            _peers = new List<IPeer>();

            var config = _repo.Get();
            foreach (var item in _options.Value.Peers)
            {
                var httpClient = new HttpClient();

                //todo what if this errors?
                var httpPeer = new HttpPeer(item.HostAndPort, httpClient, _finder, config.Data, _identityServerConfig);
                _peers.Add(httpPeer);
            }
        }

        public List<IPeer> Get()
        {
            return _peers;
        }
    }
}

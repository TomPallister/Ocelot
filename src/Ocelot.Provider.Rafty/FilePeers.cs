namespace Ocelot.Provider.Rafty
{
    using System.Collections.Generic;

    public class FilePeers
    {
        public FilePeers()
        {
            Peers = new List<FilePeer>();
        }

        public List<FilePeer> Peers { get; set; }
    }
}

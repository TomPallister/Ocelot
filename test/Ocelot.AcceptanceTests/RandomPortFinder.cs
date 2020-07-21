namespace Ocelot.AcceptanceTests
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;

    public static class RandomPortFinder
    {
        private const int TrialNumber = 100;
        private const int BeginPortRange = 20000;
        private const int EndPortRange = 45000;
        private static readonly Random Random = new Random();
        private static readonly ConcurrentBag<int> UsedPorts = new ConcurrentBag<int>();

        public static int GetRandomPort()
        {
            for (var i = 0; i < TrialNumber; i++)
            {
                var randomPort = Random.Next(BeginPortRange, EndPortRange);

                if (!PortInUse(randomPort))
                {
                    try
                    {
                        return UsePort(randomPort);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }

            throw new Exception("Cannot find available port to bind to.");
        }

        private static int UsePort(int randomPort)
        {
            UsedPorts.Add(randomPort);

            var ipe = new IPEndPoint(IPAddress.Loopback, randomPort);

            using (var socket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Bind(ipe);
                socket.Close();
                return randomPort;
            }
        }

        private static bool PortInUse(int randomPort)
        {
            return UsedPorts.Any(p => p == randomPort);
        }
    }
}

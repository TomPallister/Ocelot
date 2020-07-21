namespace Ocelot.AcceptanceTests
{
    using Ocelot.Configuration.File;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using System.Net.WebSockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using TestStack.BDDfy;
    using Xunit;

    public class WebSocketTests : IDisposable
    {
        private readonly List<string> _secondRecieved;
        private readonly List<string> _firstRecieved;
        private readonly Steps _steps;
        private readonly ServiceHandler _serviceHandler;

        public WebSocketTests()
        {
            _serviceHandler = new ServiceHandler();
            _steps = new Steps();
            _firstRecieved = new List<string>();
            _secondRecieved = new List<string>();
        }

        [Fact]
        public void should_proxy_websocket_input_to_downstream_service()
        {
            var downstreamPort = RandomPortFinder.GetRandomPort();
            var downstreamHost = "localhost";

            var config = new FileConfiguration
            {
                Routes = new List<FileRoute>
                {
                    new FileRoute
                    {
                        UpstreamPathTemplate = "/",
                        DownstreamPathTemplate = "/ws",
                        DownstreamScheme = "ws",
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = downstreamHost,
                                Port = downstreamPort
                            }
                        }
                    }
                }
            };

            this.Given(_ => _steps.GivenThereIsAConfiguration(config))
                .And(_ => _steps.StartFakeOcelotWithWebSockets())
                .And(_ => StartFakeDownstreamService($"http://{downstreamHost}:{downstreamPort}", "/ws"))
                .When(_ => StartClient("ws://localhost:5000/"))
                .Then(_ => _firstRecieved.Count.ShouldBe(10))
                .BDDfy();
        }

        [Fact]
        public void should_proxy_websocket_input_to_downstream_service_and_use_load_balancer()
        {
            var downstreamPort = RandomPortFinder.GetRandomPort();
            var downstreamHost = "localhost";
            var secondDownstreamPort = RandomPortFinder.GetRandomPort();
            var secondDownstreamHost = "localhost";

            var config = new FileConfiguration
            {
                Routes = new List<FileRoute>
                {
                    new FileRoute
                    {
                        UpstreamPathTemplate = "/",
                        DownstreamPathTemplate = "/ws",
                        DownstreamScheme = "ws",
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = downstreamHost,
                                Port = downstreamPort
                            },
                            new FileHostAndPort
                            {
                                Host = secondDownstreamHost,
                                Port = secondDownstreamPort
                            }
                        },
                        LoadBalancerOptions = new FileLoadBalancerOptions { Type = "RoundRobin" }
                    }
                }
            };

            this.Given(_ => _steps.GivenThereIsAConfiguration(config))
                .And(_ => _steps.StartFakeOcelotWithWebSockets())
                .And(_ => StartFakeDownstreamService($"http://{downstreamHost}:{downstreamPort}", "/ws"))
                .And(_ => StartSecondFakeDownstreamService($"http://{secondDownstreamHost}:{secondDownstreamPort}", "/ws"))
                .When(_ => WhenIStartTheClients())
                .Then(_ => ThenBothDownstreamServicesAreCalled())
                .BDDfy();
        }

        private void ThenBothDownstreamServicesAreCalled()
        {
            _firstRecieved.Count.ShouldBe(10);
            _firstRecieved.ForEach(x =>
            {
                x.ShouldBe("test");
            });

            _secondRecieved.Count.ShouldBe(10);
            _secondRecieved.ForEach(x =>
            {
                x.ShouldBe("chocolate");
            });
        }

        private async Task WhenIStartTheClients()
        {
            var firstClient = StartClient("ws://localhost:5000/");

            var secondClient = StartSecondClient("ws://localhost:5000/");

            await Task.WhenAll(firstClient, secondClient);
        }

        private async Task StartClient(string url)
        {
            var client = new ClientWebSocket();

            await client.ConnectAsync(new Uri(url), CancellationToken.None);

            var sending = Task.Run(async () =>
            {
                string line = "test";
                for (int i = 0; i < 10; i++)
                {
                    var bytes = Encoding.UTF8.GetBytes(line);

                    await client.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true,
                        CancellationToken.None);
                    await Task.Delay(10);
                }

                await client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            });

            var receiving = Task.Run(async () =>
            {
                var buffer = new byte[1024 * 4];

                while (true)
                {
                    var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        _firstRecieved.Add(Encoding.UTF8.GetString(buffer, 0, result.Count));
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        if (client.State != WebSocketState.Closed)
                        {
                            // Last version, the client state is CloseReceived
                            // Valid states are: Open, CloseReceived, CloseSent
                            await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                        }

                        break;
                    }
                }
            });

            await Task.WhenAll(sending, receiving);
        }

        private async Task StartSecondClient(string url)
        {
            await Task.Delay(500);

            var client = new ClientWebSocket();

            await client.ConnectAsync(new Uri(url), CancellationToken.None);

            var sending = Task.Run(async () =>
            {
                string line = "test";
                for (int i = 0; i < 10; i++)
                {
                    var bytes = Encoding.UTF8.GetBytes(line);

                    await client.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true,
                        CancellationToken.None);
                    await Task.Delay(10);
                }

                await client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            });

            var receiving = Task.Run(async () =>
            {
                var buffer = new byte[1024 * 4];

                while (true)
                {
                    var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        _secondRecieved.Add(Encoding.UTF8.GetString(buffer, 0, result.Count));
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        if (client.State != WebSocketState.Closed)
                        {
                            // Last version, the client state is CloseReceived
                            // Valid states are: Open, CloseReceived, CloseSent
                            await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                        }

                        break;
                    }
                }
            });

            await Task.WhenAll(sending, receiving);
        }

        private async Task StartFakeDownstreamService(string url, string path)
        {
            await _serviceHandler.StartFakeDownstreamService(url, path, async (context, next) =>
            {
                if (context.Request.Path == path)
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        await Echo(webSocket);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                }
                else
                {
                    await next();
                }
            });
        }

        private async Task StartSecondFakeDownstreamService(string url, string path)
        {
            await _serviceHandler.StartFakeDownstreamService(url, path, async (context, next) =>
            {
                if (context.Request.Path == path)
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        await Message(webSocket);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                }
                else
                {
                    await next();
                }
            });
        }

        private async Task Echo(WebSocket webSocket)
        {
            try
            {
                var buffer = new byte[1024 * 4];

                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                while (!result.CloseStatus.HasValue)                                
                {
                    await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);

                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }
                
                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async Task Message(WebSocket webSocket)
        {
            try
            {
                var buffer = new byte[1024 * 4];

                var bytes = Encoding.UTF8.GetBytes("chocolate");

                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                while (!result.CloseStatus.HasValue)
                {
                    await webSocket.SendAsync(new ArraySegment<byte>(bytes), result.MessageType, result.EndOfMessage, CancellationToken.None);

                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }

                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void Dispose()
        {
            _serviceHandler?.Dispose();
            _steps.Dispose();
        }
    }
}

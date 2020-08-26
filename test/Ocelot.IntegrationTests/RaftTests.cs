namespace Ocelot.IntegrationTests
{
    using Administration;
    using Configuration.File;
    using DependencyInjection;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Data.Sqlite;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Middleware;
    using Newtonsoft.Json;
    using Ocelot.Provider.Rafty;
    using Rafty.Infrastructure;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Abstractions;
    using Wait = Rafty.Infrastructure.Wait;

    public class RaftTests : IDisposable
    {
        private readonly List<IWebHost> _builders;
        private readonly List<IWebHostBuilder> _webHostBuilders;
        private readonly List<Thread> _threads;
        private FilePeers _peers;
        private HttpClient _httpClient;
        private readonly HttpClient _httpClientForAssertions;
        private BearerToken _token;
        private HttpResponseMessage _response;
        private static readonly object _lock = new object();
        private ITestOutputHelper _output;

        public RaftTests(ITestOutputHelper output)
        {
            _output = output;
            _httpClientForAssertions = new HttpClient();
            _webHostBuilders = new List<IWebHostBuilder>();
            _builders = new List<IWebHost>();
            _threads = new List<Thread>();
        }

        [Fact(Skip = "Still not stable, more work required in rafty..")]
        public async Task should_persist_command_to_five_servers()
        {
            var peers = new List<FilePeer>
            {
                new FilePeer {HostAndPort = "http://localhost:5000"},

                new FilePeer {HostAndPort = "http://localhost:5001"},

                new FilePeer {HostAndPort = "http://localhost:5002"},

                new FilePeer {HostAndPort = "http://localhost:5003"},

                new FilePeer {HostAndPort = "http://localhost:5004"}
            };

            var configuration = new FileConfiguration
            {
                GlobalConfiguration = new FileGlobalConfiguration
                {
                }
            };

            var updatedConfiguration = new FileConfiguration
            {
                GlobalConfiguration = new FileGlobalConfiguration
                {
                },
                Routes = new List<FileRoute>()
                {
                    new FileRoute()
                    {
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "127.0.0.1",
                                Port = 80,
                            }
                        },
                        DownstreamScheme = "http",
                        DownstreamPathTemplate = "/geoffrey",
                        UpstreamHttpMethod = new List<string> { "get" },
                        UpstreamPathTemplate = "/"
                    },
                    new FileRoute()
                    {
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "123.123.123",
                                Port = 443,
                            }
                        },
                        DownstreamScheme = "https",
                        DownstreamPathTemplate = "/blooper/{productId}",
                        UpstreamHttpMethod = new List<string> { "post" },
                        UpstreamPathTemplate = "/test"
                    }
                }
            };

            var command = new UpdateFileConfiguration(updatedConfiguration);
            GivenThePeersAre(peers);
            GivenThereIsAConfiguration(configuration);
            GivenFiveServersAreRunning();
            await GivenIHaveAnOcelotToken("/administration");
            await WhenISendACommandIntoTheCluster(command);
            Thread.Sleep(5000);
            await ThenTheCommandIsReplicatedToAllStateMachines(command);
        }

        [Fact(Skip = "Still not stable, more work required in rafty..")]
        public async Task should_persist_command_to_five_servers_when_using_administration_api()
        {
            var peers = new List<FilePeer>
            {
                new FilePeer {HostAndPort = "http://localhost:5005"},

                new FilePeer {HostAndPort = "http://localhost:5006"},

                new FilePeer {HostAndPort = "http://localhost:5007"},

                new FilePeer {HostAndPort = "http://localhost:5008"},

                new FilePeer {HostAndPort = "http://localhost:5009"}
            };

            var configuration = new FileConfiguration
            {
            };

            var updatedConfiguration = new FileConfiguration
            {
                Routes = new List<FileRoute>()
                {
                    new FileRoute()
                    {
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "127.0.0.1",
                                Port = 80,
                            }
                        },
                        DownstreamScheme = "http",
                        DownstreamPathTemplate = "/geoffrey",
                        UpstreamHttpMethod = new List<string> { "get" },
                        UpstreamPathTemplate = "/"
                    },
                    new FileRoute()
                    {
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "123.123.123",
                                Port = 443,
                            }
                        },
                        DownstreamScheme = "https",
                        DownstreamPathTemplate = "/blooper/{productId}",
                        UpstreamHttpMethod = new List<string> { "post" },
                        UpstreamPathTemplate = "/test"
                    }
                }
            };

            var command = new UpdateFileConfiguration(updatedConfiguration);
            GivenThePeersAre(peers);
            GivenThereIsAConfiguration(configuration);
            GivenFiveServersAreRunning();
            await GivenIHaveAnOcelotToken("/administration");
            GivenIHaveAddedATokenToMyRequest();
            await WhenIPostOnTheApiGateway("/administration/configuration", updatedConfiguration);
            await ThenTheCommandIsReplicatedToAllStateMachines(command);
        }

        private void GivenThePeersAre(List<FilePeer> peers)
        {
            FilePeers filePeers = new FilePeers();
            filePeers.Peers.AddRange(peers);
            var json = JsonConvert.SerializeObject(filePeers);
            File.WriteAllText("peers.json", json);
            _httpClient = new HttpClient();
            var ocelotBaseUrl = peers[0].HostAndPort;
            _httpClient.BaseAddress = new Uri(ocelotBaseUrl);
        }

        private async Task WhenISendACommandIntoTheCluster(UpdateFileConfiguration command)
        {
            async Task<bool> SendCommand()
            {
                try
                {
                    var p = _peers.Peers.First();
                    var json = JsonConvert.SerializeObject(command, new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.All
                    });
                    var httpContent = new StringContent(json);
                    httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token.AccessToken);
                        var response = await httpClient.PostAsync($"{p.HostAndPort}/administration/raft/command", httpContent);
                        response.EnsureSuccessStatusCode();
                        var content = await response.Content.ReadAsStringAsync();

                        var errorResult = JsonConvert.DeserializeObject<ErrorResponse<UpdateFileConfiguration>>(content);

                        if (!string.IsNullOrEmpty(errorResult.Error))
                        {
                            return false;
                        }

                        var okResult = JsonConvert.DeserializeObject<OkResponse<UpdateFileConfiguration>>(content);

                        if (okResult.Command.Configuration.Routes.Count == 2)
                        {
                            return true;
                        }
                    }

                    return false;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return false;
                }
            }

            var commandSent = await Wait.WaitFor(40000).Until(async () =>
            {
                var result = await SendCommand();
                Thread.Sleep(1000);
                return result;
            });

            commandSent.ShouldBeTrue();
        }

        private async Task ThenTheCommandIsReplicatedToAllStateMachines(UpdateFileConfiguration expecteds)
        {
            async Task<bool> CommandCalledOnAllStateMachines()
            {
                try
                {
                    var passed = 0;
                    foreach (var peer in _peers.Peers)
                    {
                        var path = $"{peer.HostAndPort.Replace("/", "").Replace(":", "")}.db";
                        using (var connection = new SqliteConnection($"Data Source={path};"))
                        {
                            connection.Open();
                            var sql = @"select count(id) from logs";
                            using (var command = new SqliteCommand(sql, connection))
                            {
                                var index = Convert.ToInt32(command.ExecuteScalar());
                                index.ShouldBe(1);
                            }
                        }

                        _httpClientForAssertions.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token.AccessToken);
                        var result = await _httpClientForAssertions.GetAsync($"{peer.HostAndPort}/administration/configuration");
                        var json = await result.Content.ReadAsStringAsync();
                        var response = JsonConvert.DeserializeObject<FileConfiguration>(json, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
                        response.GlobalConfiguration.RequestIdKey.ShouldBe(expecteds.Configuration.GlobalConfiguration.RequestIdKey);
                        response.GlobalConfiguration.ServiceDiscoveryProvider.Host.ShouldBe(expecteds.Configuration.GlobalConfiguration.ServiceDiscoveryProvider.Host);
                        response.GlobalConfiguration.ServiceDiscoveryProvider.Port.ShouldBe(expecteds.Configuration.GlobalConfiguration.ServiceDiscoveryProvider.Port);

                        for (var i = 0; i < response.Routes.Count; i++)
                        {
                            for (var j = 0; j < response.Routes[i].DownstreamHostAndPorts.Count; j++)
                            {
                                var res = response.Routes[i].DownstreamHostAndPorts[j];
                                var expected = expecteds.Configuration.Routes[i].DownstreamHostAndPorts[j];
                                res.Host.ShouldBe(expected.Host);
                                res.Port.ShouldBe(expected.Port);
                            }

                            response.Routes[i].DownstreamPathTemplate.ShouldBe(expecteds.Configuration.Routes[i].DownstreamPathTemplate);
                            response.Routes[i].DownstreamScheme.ShouldBe(expecteds.Configuration.Routes[i].DownstreamScheme);
                            response.Routes[i].UpstreamPathTemplate.ShouldBe(expecteds.Configuration.Routes[i].UpstreamPathTemplate);
                            response.Routes[i].UpstreamHttpMethod.ShouldBe(expecteds.Configuration.Routes[i].UpstreamHttpMethod);
                        }

                        passed++;
                    }

                    return passed == 5;
                }
                catch (Exception e)
                {
                    //_output.WriteLine($"{e.Message}, {e.StackTrace}");
                    Console.WriteLine(e);
                    return false;
                }
            }

            var commandOnAllStateMachines = await Wait.WaitFor(40000).Until(async () =>
            {
                var result = await CommandCalledOnAllStateMachines();
                Thread.Sleep(1000);
                return result;
            });

            commandOnAllStateMachines.ShouldBeTrue();
        }

        private async Task WhenIPostOnTheApiGateway(string url, FileConfiguration updatedConfiguration)
        {
            async Task<bool> SendCommand()
            {
                var json = JsonConvert.SerializeObject(updatedConfiguration);

                var content = new StringContent(json);

                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                _response = await _httpClient.PostAsync(url, content);

                var responseContent = await _response.Content.ReadAsStringAsync();

                if (responseContent == "There was a problem. This error message sucks raise an issue in GitHub.")
                {
                    return false;
                }

                if (string.IsNullOrEmpty(responseContent))
                {
                    return false;
                }

                return _response.IsSuccessStatusCode;
            }

            var commandSent = await Wait.WaitFor(40000).Until(async () =>
            {
                var result = await SendCommand();
                Thread.Sleep(1000);
                return result;
            });

            commandSent.ShouldBeTrue();
        }

        private void GivenIHaveAddedATokenToMyRequest()
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token.AccessToken);
        }

        private async Task GivenIHaveAnOcelotToken(string adminPath)
        {
            async Task<bool> AddToken()
            {
                try
                {
                    var tokenUrl = $"{adminPath}/connect/token";
                    var formData = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("client_id", "admin"),
                        new KeyValuePair<string, string>("client_secret", "secret"),
                        new KeyValuePair<string, string>("scope", "admin"),
                        new KeyValuePair<string, string>("grant_type", "client_credentials")
                    };
                    var content = new FormUrlEncodedContent(formData);

                    var response = await _httpClient.PostAsync(tokenUrl, content);
                    var responseContent = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        return false;
                    }

                    _token = JsonConvert.DeserializeObject<BearerToken>(responseContent);
                    var configPath = $"{adminPath}/.well-known/openid-configuration";
                    response = await _httpClient.GetAsync(configPath);
                    return response.IsSuccessStatusCode;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            var addToken = await Wait.WaitFor(40000).Until(async () =>
            {
                var result = await AddToken();
                Thread.Sleep(1000);
                return result;
            });

            addToken.ShouldBeTrue();
        }

        private void GivenThereIsAConfiguration(FileConfiguration fileConfiguration)
        {
            var configurationPath = $"{Directory.GetCurrentDirectory()}/ocelot.json";

            var jsonConfiguration = JsonConvert.SerializeObject(fileConfiguration);

            if (File.Exists(configurationPath))
            {
                File.Delete(configurationPath);
            }

            File.WriteAllText(configurationPath, jsonConfiguration);

            var text = File.ReadAllText(configurationPath);

            configurationPath = $"{AppContext.BaseDirectory}/ocelot.json";

            if (File.Exists(configurationPath))
            {
                File.Delete(configurationPath);
            }

            File.WriteAllText(configurationPath, jsonConfiguration);

            text = File.ReadAllText(configurationPath);
        }

        private void GivenAServerIsRunning(string url)
        {
            lock (_lock)
            {
                IWebHostBuilder webHostBuilder = new WebHostBuilder();
                webHostBuilder.UseUrls(url)
                    .UseKestrel()
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        config.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath);
                        var env = hostingContext.HostingEnvironment;
                        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: false);
                        config.AddJsonFile("ocelot.json", false, false);
                        config.AddJsonFile("peers.json", optional: true, reloadOnChange: false);
#pragma warning disable CS0618
                        config.AddOcelotBaseUrl(url);
#pragma warning restore CS0618
                        config.AddEnvironmentVariables();
                    })
                    .ConfigureServices(x =>
                    {
                        x.AddSingleton(new NodeId(url));
                        x
                            .AddOcelot()
                            .AddAdministration("/administration", "secret")
                            .AddRafty();
                    })
                    .Configure(app =>
                    {
                        app.UseOcelot().Wait();
                    });

                var builder = webHostBuilder.Build();
                builder.Start();

                _webHostBuilders.Add(webHostBuilder);
                _builders.Add(builder);
            }
        }

        private void GivenFiveServersAreRunning()
        {
            var bytes = File.ReadAllText("peers.json");
            _peers = JsonConvert.DeserializeObject<FilePeers>(bytes);

            foreach (var peer in _peers.Peers)
            {
                File.Delete(peer.HostAndPort.Replace("/", "").Replace(":", ""));
                File.Delete($"{peer.HostAndPort.Replace("/", "").Replace(":", "")}.db");
                var thread = new Thread(() => GivenAServerIsRunning(peer.HostAndPort));
                thread.Start();
                _threads.Add(thread);
            }
        }

        public void Dispose()
        {
            foreach (var builder in _builders)
            {
                builder?.Dispose();
            }

            foreach (var peer in _peers.Peers)
            {
                try
                {
                    File.Delete(peer.HostAndPort.Replace("/", "").Replace(":", ""));
                    File.Delete($"{peer.HostAndPort.Replace("/", "").Replace(":", "")}.db");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}

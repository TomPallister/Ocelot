using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Ocelot.Configuration.File;
using Ocelot.Raft;
using Rafty.Concensus;
using Rafty.FiniteStateMachine;
using Rafty.Infrastructure;
using Shouldly;
using Xunit;
using static Rafty.Infrastructure.Wait;
using Microsoft.Data.Sqlite;

namespace Ocelot.IntegrationTests
{
    public class Tests : IDisposable
    {
        private List<IWebHost> _builders;
        private List<IWebHostBuilder> _webHostBuilders;
        private List<Thread> _threads;
        private FilePeers _peers;
        private HttpClient _httpClient;
        private HttpClient _httpClientForAssertions;
        private string _ocelotBaseUrl;
        private BearerToken _token;
        private HttpResponseMessage _response;

        public Tests()
        {
            _httpClientForAssertions = new HttpClient();
            _httpClient = new HttpClient();
            _ocelotBaseUrl = "http://localhost:5000";
            _httpClient.BaseAddress = new Uri(_ocelotBaseUrl);
            _webHostBuilders = new List<IWebHostBuilder>();
            _builders = new List<IWebHost>();
            _threads = new List<Thread>();
        }
        public void Dispose()
        {
            foreach (var builder in _builders)
            {
                builder?.Dispose();
            }

            foreach (var peer in _peers.Peers)
            {
                File.Delete(peer.HostAndPort.Replace("/","").Replace(":",""));
                File.Delete($"{peer.HostAndPort.Replace("/","").Replace(":","")}.db");
            }
        }

        [Fact]
        public void should_persist_command_to_five_servers()
        {
             var configuration = new FileConfiguration
             { 
                 GlobalConfiguration = new FileGlobalConfiguration
                 {
                     AdministrationPath = "/administration"
                 }
             };

            var updatedConfiguration = new FileConfiguration
            {
                GlobalConfiguration = new FileGlobalConfiguration
                {
                    AdministrationPath = "/administration"
                },
                ReRoutes = new List<FileReRoute>()
                {
                    new FileReRoute()
                    {
                        DownstreamHost = "127.0.0.1",
                        DownstreamPort = 80,
                        DownstreamScheme = "http",
                        DownstreamPathTemplate = "/geoffrey",
                        UpstreamHttpMethod = new List<string> { "get" },
                        UpstreamPathTemplate = "/"
                    },
                    new FileReRoute()
                    {
                        DownstreamHost = "123.123.123",
                        DownstreamPort = 443,
                        DownstreamScheme = "https",
                        DownstreamPathTemplate = "/blooper/{productId}",
                        UpstreamHttpMethod = new List<string> { "post" },
                        UpstreamPathTemplate = "/test"
                    }
                }
            };
             
            var command = new UpdateFileConfiguration(updatedConfiguration);
            GivenThereIsAConfiguration(configuration);
            GivenFiveServersAreRunning();
            GivenALeaderIsElected();
            GivenIHaveAnOcelotToken("/administration");
            WhenISendACommandIntoTheCluster(command);
            ThenTheCommandIsReplicatedToAllStateMachines(command);
        }

        [Fact]
        public void should_persist_command_to_five_servers_when_using_administration_api()
        {
             var configuration = new FileConfiguration
             { 
                 GlobalConfiguration = new FileGlobalConfiguration
                 {
                     AdministrationPath = "/administration"
                 }
             };

            var updatedConfiguration = new FileConfiguration
            {
                GlobalConfiguration = new FileGlobalConfiguration
                {
                    AdministrationPath = "/administration"
                },
                ReRoutes = new List<FileReRoute>()
                {
                    new FileReRoute()
                    {
                        DownstreamHost = "127.0.0.1",
                        DownstreamPort = 80,
                        DownstreamScheme = "http",
                        DownstreamPathTemplate = "/geoffrey",
                        UpstreamHttpMethod = new List<string> { "get" },
                        UpstreamPathTemplate = "/"
                    },
                    new FileReRoute()
                    {
                        DownstreamHost = "123.123.123",
                        DownstreamPort = 443,
                        DownstreamScheme = "https",
                        DownstreamPathTemplate = "/blooper/{productId}",
                        UpstreamHttpMethod = new List<string> { "post" },
                        UpstreamPathTemplate = "/test"
                    }
                }
            };
             
            var command = new UpdateFileConfiguration(updatedConfiguration);
            GivenThereIsAConfiguration(configuration);
            GivenFiveServersAreRunning();
            GivenALeaderIsElected();
            GivenIHaveAnOcelotToken("/administration");
            GivenIHaveAddedATokenToMyRequest();
            WhenIPostOnTheApiGateway("/administration/configuration", updatedConfiguration);
            ThenTheCommandIsReplicatedToAllStateMachines(command);
        }

        private void WhenISendACommandIntoTheCluster(UpdateFileConfiguration command)
        {
            var p = _peers.Peers.First();
            var json = JsonConvert.SerializeObject(command,new JsonSerializerSettings() { 
                TypeNameHandling = TypeNameHandling.All
            });
            var httpContent = new StringContent(json);
            httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            using(var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token.AccessToken);
                var response = httpClient.PostAsync($"{p.HostAndPort}/administration/raft/command", httpContent).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonConvert.DeserializeObject<OkResponse<UpdateFileConfiguration>>(content);
                result.Command.Configuration.GlobalConfiguration.AdministrationPath.ShouldBe(command.Configuration.GlobalConfiguration.AdministrationPath);
            }

            //dirty sleep to make sure command replicated...
            var stopwatch = Stopwatch.StartNew();
            while(stopwatch.ElapsedMilliseconds < 10000)
            {

            }
        }

        private void ThenTheCommandIsReplicatedToAllStateMachines(UpdateFileConfiguration expected)
        {
            //dirty sleep to give a chance to replicate...
            var stopwatch = Stopwatch.StartNew();
            while(stopwatch.ElapsedMilliseconds < 2000)
            {

            }
            
             bool CommandCalledOnAllStateMachines()
            {
                try
                {
                    var passed = 0;
                    foreach (var peer in _peers.Peers)
                    {
                        var path = $"{peer.HostAndPort.Replace("/","").Replace(":","")}.db";
                        using(var connection = new SqliteConnection($"Data Source={path};"))
                        {
                            connection.Open();
                            var sql = @"select count(id) from logs";
                            using(var command = new SqliteCommand(sql, connection))
                            {
                                var index = Convert.ToInt32(command.ExecuteScalar());
                                index.ShouldBe(1);
                            }
                        }
                        _httpClientForAssertions.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token.AccessToken);
                        var result = _httpClientForAssertions.GetAsync($"{peer.HostAndPort}/administration/configuration").Result;
                        var json = result.Content.ReadAsStringAsync().Result;
                        var response = JsonConvert.DeserializeObject<FileConfiguration>(json, new JsonSerializerSettings{TypeNameHandling = TypeNameHandling.All});
                         response.GlobalConfiguration.AdministrationPath.ShouldBe(expected.Configuration.GlobalConfiguration.AdministrationPath);
                        response.GlobalConfiguration.RequestIdKey.ShouldBe(expected.Configuration.GlobalConfiguration.RequestIdKey);
                        response.GlobalConfiguration.ServiceDiscoveryProvider.Host.ShouldBe(expected.Configuration.GlobalConfiguration.ServiceDiscoveryProvider.Host);
                        response.GlobalConfiguration.ServiceDiscoveryProvider.Port.ShouldBe(expected.Configuration.GlobalConfiguration.ServiceDiscoveryProvider.Port);

                        for (var i = 0; i < response.ReRoutes.Count; i++)
                        {
                            response.ReRoutes[i].DownstreamHost.ShouldBe(expected.Configuration.ReRoutes[i].DownstreamHost);
                            response.ReRoutes[i].DownstreamPathTemplate.ShouldBe(expected.Configuration.ReRoutes[i].DownstreamPathTemplate);
                            response.ReRoutes[i].DownstreamPort.ShouldBe(expected.Configuration.ReRoutes[i].DownstreamPort);
                            response.ReRoutes[i].DownstreamScheme.ShouldBe(expected.Configuration.ReRoutes[i].DownstreamScheme);
                            response.ReRoutes[i].UpstreamPathTemplate.ShouldBe(expected.Configuration.ReRoutes[i].UpstreamPathTemplate);
                            response.ReRoutes[i].UpstreamHttpMethod.ShouldBe(expected.Configuration.ReRoutes[i].UpstreamHttpMethod);
                        }
                        passed++;
                    }

                    return passed == 5;
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                    return false;
                }
            }

            var commandOnAllStateMachines = WaitFor(20000).Until(() => CommandCalledOnAllStateMachines());
            commandOnAllStateMachines.ShouldBeTrue();   
        }

        private void ThenTheResponseShouldBe(FileConfiguration expected)
        {
            var response = JsonConvert.DeserializeObject<FileConfiguration>(_response.Content.ReadAsStringAsync().Result);

            response.GlobalConfiguration.AdministrationPath.ShouldBe(expected.GlobalConfiguration.AdministrationPath);
            response.GlobalConfiguration.RequestIdKey.ShouldBe(expected.GlobalConfiguration.RequestIdKey);
            response.GlobalConfiguration.ServiceDiscoveryProvider.Host.ShouldBe(expected.GlobalConfiguration.ServiceDiscoveryProvider.Host);
            response.GlobalConfiguration.ServiceDiscoveryProvider.Port.ShouldBe(expected.GlobalConfiguration.ServiceDiscoveryProvider.Port);

            for (var i = 0; i < response.ReRoutes.Count; i++)
            {
                response.ReRoutes[i].DownstreamHost.ShouldBe(expected.ReRoutes[i].DownstreamHost);
                response.ReRoutes[i].DownstreamPathTemplate.ShouldBe(expected.ReRoutes[i].DownstreamPathTemplate);
                response.ReRoutes[i].DownstreamPort.ShouldBe(expected.ReRoutes[i].DownstreamPort);
                response.ReRoutes[i].DownstreamScheme.ShouldBe(expected.ReRoutes[i].DownstreamScheme);
                response.ReRoutes[i].UpstreamPathTemplate.ShouldBe(expected.ReRoutes[i].UpstreamPathTemplate);
                response.ReRoutes[i].UpstreamHttpMethod.ShouldBe(expected.ReRoutes[i].UpstreamHttpMethod);
            }
        }

        private void WhenIGetUrlOnTheApiGateway(string url)
        {
            _response = _httpClient.GetAsync(url).Result;
        }

        private void WhenIPostOnTheApiGateway(string url, FileConfiguration updatedConfiguration)
        {
            var json = JsonConvert.SerializeObject(updatedConfiguration);
            var content = new StringContent(json);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            _response = _httpClient.PostAsync(url, content).Result;
        }

        private void GivenIHaveAddedATokenToMyRequest()
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token.AccessToken);
        }

        private void GivenIHaveAnOcelotToken(string adminPath)
        {
            var tokenUrl = $"{adminPath}/connect/token";
            var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id", "admin"),
                new KeyValuePair<string, string>("client_secret", "secret"),
                new KeyValuePair<string, string>("scope", "admin"),
                new KeyValuePair<string, string>("username", "admin"),
                new KeyValuePair<string, string>("password", "secret"),
                new KeyValuePair<string, string>("grant_type", "password")
            };
            var content = new FormUrlEncodedContent(formData);

            var response = _httpClient.PostAsync(tokenUrl, content).Result;
            var responseContent = response.Content.ReadAsStringAsync().Result;
            response.EnsureSuccessStatusCode();
            _token = JsonConvert.DeserializeObject<BearerToken>(responseContent);
            var configPath = $"{adminPath}/.well-known/openid-configuration";
            response = _httpClient.GetAsync(configPath).Result;
            response.EnsureSuccessStatusCode();
        }

        private void GivenThereIsAConfiguration(FileConfiguration fileConfiguration)
        {
            var configurationPath = $"{Directory.GetCurrentDirectory()}/configuration.json";

            var jsonConfiguration = JsonConvert.SerializeObject(fileConfiguration);

            if (File.Exists(configurationPath))
            {
                File.Delete(configurationPath);
            }

            File.WriteAllText(configurationPath, jsonConfiguration);

            var text = File.ReadAllText(configurationPath);

            configurationPath = $"{AppContext.BaseDirectory}/configuration.json";

            if (File.Exists(configurationPath))
            {
                File.Delete(configurationPath);
            }

            File.WriteAllText(configurationPath, jsonConfiguration);

            text = File.ReadAllText(configurationPath);
        }

        private void GivenAServerIsRunning(string url)
        {
            IWebHostBuilder webHostBuilder = new WebHostBuilder();
            webHostBuilder.UseUrls(url)
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureServices(x =>
                {
                    x.AddSingleton(webHostBuilder);
                    x.AddSingleton(new NodeId(url));
                })
                .UseStartup<RaftStartup>();

            var builder = webHostBuilder.Build();
            builder.Start();

            _webHostBuilders.Add(webHostBuilder);
            _builders.Add(builder);
        }

        private void GivenFiveServersAreRunning()
        {
            var bytes = File.ReadAllText("peers.json");
            _peers = JsonConvert.DeserializeObject<FilePeers>(bytes);

            foreach (var peer in _peers.Peers)
            {
                var thread = new Thread(() => GivenAServerIsRunning(peer.HostAndPort));
                thread.Start();
                _threads.Add(thread);
            }
        }

        private void GivenALeaderIsElected()
        {
            //dirty sleep to make sure we have a leader
            var stopwatch = Stopwatch.StartNew();
            while(stopwatch.ElapsedMilliseconds < 20000)
            {

            }
        }

        private void WhenISendACommandIntoTheCluster(FakeCommand command)
        {
            var p = _peers.Peers.First();
            var json = JsonConvert.SerializeObject(command,new JsonSerializerSettings() { 
                TypeNameHandling = TypeNameHandling.All
            });
            var httpContent = new StringContent(json);
            httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            using(var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token.AccessToken);
                var response = httpClient.PostAsync($"{p.HostAndPort}/administration/raft/command", httpContent).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonConvert.DeserializeObject<OkResponse<FakeCommand>>(content);
                result.Command.Value.ShouldBe(command.Value);
            }

            //dirty sleep to make sure command replicated...
            var stopwatch = Stopwatch.StartNew();
            while(stopwatch.ElapsedMilliseconds < 10000)
            {

            }
        }

        private void ThenTheCommandIsReplicatedToAllStateMachines(FakeCommand command)
        {
            //dirty sleep to give a chance to replicate...
            var stopwatch = Stopwatch.StartNew();
            while(stopwatch.ElapsedMilliseconds < 2000)
            {

            }
            
             bool CommandCalledOnAllStateMachines()
            {
                try
                {
                    var passed = 0;
                    foreach (var peer in _peers.Peers)
                    {
                        string fsmData;
                        fsmData = File.ReadAllText(peer.HostAndPort.Replace("/","").Replace(":",""));
                        fsmData.ShouldNotBeNullOrEmpty();
                        var fakeCommand = JsonConvert.DeserializeObject<FakeCommand>(fsmData);
                        fakeCommand.Value.ShouldBe(command.Value);
                        passed++;
                    }

                    return passed == 5;
                }
                catch(Exception e)
                {
                    return false;
                }
            }

            var commandOnAllStateMachines = WaitFor(20000).Until(() => CommandCalledOnAllStateMachines());
            commandOnAllStateMachines.ShouldBeTrue();   
        }
    }
}

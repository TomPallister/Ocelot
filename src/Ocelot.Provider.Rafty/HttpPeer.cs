namespace Ocelot.Provider.Rafty
{
    using Administration;
    using Configuration;
    using global::Rafty.Concensus.Messages;
    using global::Rafty.Concensus.Peers;
    using global::Rafty.FiniteStateMachine;
    using global::Rafty.Infrastructure;
    using Middleware;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class HttpPeer : IPeer
    {
        private readonly string _hostAndPort;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly string _baseSchemeUrlAndPort;
        private BearerToken _token;
        private readonly IInternalConfiguration _config;
        private readonly IIdentityServerConfiguration _identityServerConfiguration;

        public HttpPeer(string hostAndPort, HttpClient httpClient, IBaseUrlFinder finder, IInternalConfiguration config, IIdentityServerConfiguration identityServerConfiguration)
        {
            _identityServerConfiguration = identityServerConfiguration;
            _config = config;
            Id = hostAndPort;
            _hostAndPort = hostAndPort;
            _httpClient = httpClient;
            _jsonSerializerSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };
            _baseSchemeUrlAndPort = finder.Find();
        }

        public string Id { get; }

        public async Task<RequestVoteResponse> Request(RequestVote requestVote)
        {
            if (_token == null)
            {
                await SetToken();
            }

            var json = JsonConvert.SerializeObject(requestVote, _jsonSerializerSettings);
            var content = new StringContent(json);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var response = await _httpClient.PostAsync($"{_hostAndPort}/administration/raft/requestvote", content);
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<RequestVoteResponse>(await response.Content.ReadAsStringAsync(), _jsonSerializerSettings);
            }

            return new RequestVoteResponse(false, requestVote.Term);
        }

        public async Task<AppendEntriesResponse> Request(AppendEntries appendEntries)
        {
            try
            {
                if (_token == null)
                {
                    await SetToken();
                }

                var json = JsonConvert.SerializeObject(appendEntries, _jsonSerializerSettings);
                var content = new StringContent(json);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                var response = await _httpClient.PostAsync($"{_hostAndPort}/administration/raft/appendEntries", content);
                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<AppendEntriesResponse>(await response.Content.ReadAsStringAsync(), _jsonSerializerSettings);
                }

                return new AppendEntriesResponse(appendEntries.Term, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new AppendEntriesResponse(appendEntries.Term, false);
            }
        }

        public async Task<Response<T>> Request<T>(T command)
            where T : ICommand
        {
            Console.WriteLine("SENDING REQUEST....");
            if (_token == null)
            {
                await SetToken();
            }

            var json = JsonConvert.SerializeObject(command, _jsonSerializerSettings);
            var content = new StringContent(json);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var response = await _httpClient.PostAsync($"{_hostAndPort}/administration/raft/command", content);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("REQUEST OK....");
                var okResponse = JsonConvert.DeserializeObject<OkResponse<ICommand>>(await response.Content.ReadAsStringAsync(), _jsonSerializerSettings);
                return new OkResponse<T>((T)okResponse.Command);
            }

            Console.WriteLine("REQUEST NOT OK....");
            return new ErrorResponse<T>(await response.Content.ReadAsStringAsync(), command);
        }

        private async Task SetToken()
        {
            var tokenUrl = $"{_baseSchemeUrlAndPort}{_config.AdministrationPath}/connect/token";
            var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id", _identityServerConfiguration.ApiName),
                new KeyValuePair<string, string>("client_secret", _identityServerConfiguration.ApiSecret),
                new KeyValuePair<string, string>("scope", _identityServerConfiguration.ApiName),
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            };
            var content = new FormUrlEncodedContent(formData);
            var response = await _httpClient.PostAsync(tokenUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
            _token = JsonConvert.DeserializeObject<BearerToken>(responseContent);
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(_token.TokenType, _token.AccessToken);
        }
    }
}

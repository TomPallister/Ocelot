namespace Ocelot.Requester
{
    using Microsoft.AspNetCore.Http;
    using Ocelot.Configuration;
    using Ocelot.Logging;
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;

    public class HttpClientBuilder : IHttpClientBuilder
    {
        private readonly IDelegatingHandlerHandlerFactory _factory;
        private readonly IHttpClientCache _cacheHandlers;
        private readonly IOcelotLogger _logger;
        private DownstreamRoute _cacheKey;
        private HttpClient _httpClient;
        private IHttpClient _client;
        private readonly TimeSpan _defaultTimeout;

        public HttpClientBuilder(
            IDelegatingHandlerHandlerFactory factory,
            IHttpClientCache cacheHandlers,
            IOcelotLogger logger)
        {
            _factory = factory;
            _cacheHandlers = cacheHandlers;
            _logger = logger;

            // This is hardcoded at the moment but can easily be added to configuration
            // if required by a user request.
            _defaultTimeout = TimeSpan.FromSeconds(90);
        }

        public IHttpClient Create(DownstreamRoute downstreamRoute, HttpContext httpContext)
        {
            _cacheKey = downstreamRoute;

            var httpClient = _cacheHandlers.Get(_cacheKey);

            if (httpClient != null)
            {
                _client = httpClient;

                var clientHandler = _client.ClientMainHandler;
                while (clientHandler != null)
                {
                    if (clientHandler is IDelegatingHandlerWithHttpContext contextClientHandler)
                    {
                        contextClientHandler.HttpContext = httpContext;
                    }

                    clientHandler = clientHandler.InnerHandler as DelegatingHandler;
                }

                return httpClient;
            }

            var handler = CreateHandler(downstreamRoute);

            if (downstreamRoute.DangerousAcceptAnyServerCertificateValidator)
            {
                handler.ServerCertificateCustomValidationCallback = (request, certificate, chain, errors) => true;

                _logger
                    .LogWarning($"You have ignored all SSL warnings by using DangerousAcceptAnyServerCertificateValidator for this DownstreamRoute, UpstreamPathTemplate: {downstreamRoute.UpstreamPathTemplate}, DownstreamPathTemplate: {downstreamRoute.DownstreamPathTemplate}");
            }

            var timeout = downstreamRoute.QosOptions.TimeoutValue == 0
                ? _defaultTimeout
                : TimeSpan.FromMilliseconds(downstreamRoute.QosOptions.TimeoutValue);

            var clientMainHandler = CreateHttpMessageHandler(handler, downstreamRoute, httpContext);
            _httpClient = new HttpClient(clientMainHandler)
            {
                Timeout = timeout
            };

            _client = new HttpClientWrapper(_httpClient, clientMainHandler as DelegatingHandler);

            return _client;
        }

        private HttpClientHandler CreateHandler(DownstreamRoute downstreamRoute)
        {
            // Dont' create the CookieContainer if UseCookies is not set or the HttpClient will complain
            // under .Net Full Framework
            var useCookies = downstreamRoute.HttpHandlerOptions.UseCookieContainer;

            return useCookies ? UseCookiesHandler(downstreamRoute) : UseNonCookiesHandler(downstreamRoute);
        }

        private HttpClientHandler UseNonCookiesHandler(DownstreamRoute downstreamRoute)
        {
            return new HttpClientHandler
            {
                AllowAutoRedirect = downstreamRoute.HttpHandlerOptions.AllowAutoRedirect,
                UseCookies = downstreamRoute.HttpHandlerOptions.UseCookieContainer,
                UseProxy = downstreamRoute.HttpHandlerOptions.UseProxy,
                MaxConnectionsPerServer = downstreamRoute.HttpHandlerOptions.MaxConnectionsPerServer,
            };
        }

        private HttpClientHandler UseCookiesHandler(DownstreamRoute downstreamRoute)
        {
            return new HttpClientHandler
            {
                AllowAutoRedirect = downstreamRoute.HttpHandlerOptions.AllowAutoRedirect,
                UseCookies = downstreamRoute.HttpHandlerOptions.UseCookieContainer,
                UseProxy = downstreamRoute.HttpHandlerOptions.UseProxy,
                MaxConnectionsPerServer = downstreamRoute.HttpHandlerOptions.MaxConnectionsPerServer,
                CookieContainer = new CookieContainer(),
            };
        }

        public void Save()
        {
            _cacheHandlers.Set(_cacheKey, _client, TimeSpan.FromHours(24));
        }

        private HttpMessageHandler CreateHttpMessageHandler(HttpMessageHandler httpMessageHandler, 
            DownstreamRoute request, HttpContext httpContext)
        {
            //todo handle error
            var handlers = _factory.Get(request, httpContext).Data;

            handlers
                .Select(handler => handler)
                .Reverse()
                .ToList()
                .ForEach(handler =>
                {
                    var delegatingHandler = handler();
                    delegatingHandler.InnerHandler = httpMessageHandler;
                    httpMessageHandler = delegatingHandler;
                });
            return httpMessageHandler;
        }
    }
}

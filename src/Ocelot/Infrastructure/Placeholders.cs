namespace Ocelot.Infrastructure
{
    using Microsoft.AspNetCore.Http;
    using Ocelot.Infrastructure.RequestData;
    using Ocelot.Middleware;
    using Ocelot.Request.Middleware;
    using Ocelot.Responses;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Placeholders : IPlaceholders
    {
        private readonly Dictionary<string, Func<Response<string>>> _placeholders;
        private readonly Dictionary<string, Func<DownstreamRequest, string>> _requestPlaceholders;
        private readonly IBaseUrlFinder _finder;
        private readonly IRequestScopedDataRepository _repo;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public Placeholders(IBaseUrlFinder finder, IRequestScopedDataRepository repo, IHttpContextAccessor httpContextAccessor)
        {
            _repo = repo;
            _httpContextAccessor = httpContextAccessor;
            _finder = finder;
            _placeholders = new Dictionary<string, Func<Response<string>>>
            {
                { "{BaseUrl}", GetBaseUrl() },
                { "{TraceId}", GetTraceId() },
                { "{RemoteIpAddress}", GetRemoteIpAddress() },
                { "{UpstreamHost}", GetUpstreamHost() },
            };

            _requestPlaceholders = new Dictionary<string, Func<DownstreamRequest, string>>
            {
                { "{DownstreamBaseUrl}", GetDownstreamBaseUrl() },
            };
        }

        public Response<string> Get(string key)
        {
            if (_placeholders.ContainsKey(key))
            {
                var response = _placeholders[key].Invoke();
                if (!response.IsError)
                {
                    return new OkResponse<string>(response.Data);
                }
            }

            return new ErrorResponse<string>(new CouldNotFindPlaceholderError(key));
        }

        public Response<string> Get(string key, DownstreamRequest request)
        {
            if (_requestPlaceholders.ContainsKey(key))
            {
                return new OkResponse<string>(_requestPlaceholders[key].Invoke(request));
            }

            return new ErrorResponse<string>(new CouldNotFindPlaceholderError(key));
        }

        public Response Add(string key, Func<Response<string>> func)
        {
            if (_placeholders.ContainsKey(key))
            {
                return new ErrorResponse(new CannotAddPlaceholderError($"Unable to add placeholder: {key}, placeholder already exists"));
            }

            _placeholders.Add(key, func);
            return new OkResponse();
        }

        public Response Remove(string key)
        {
            if (!_placeholders.ContainsKey(key))
            {
                return new ErrorResponse(new CannotRemovePlaceholderError($"Unable to remove placeholder: {key}, placeholder does not exists"));
            }

            _placeholders.Remove(key);
            return new OkResponse();
        }

        private Func<Response<string>> GetRemoteIpAddress()
        {
            return () =>
            {
                // this can blow up so adding try catch and return error
                try
                {
                    var remoteIdAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
                    return new OkResponse<string>(remoteIdAddress);
                }
                catch
                {
                    return new ErrorResponse<string>(new CouldNotFindPlaceholderError("{RemoteIpAddress}"));
                }
            };
        }

        private Func<DownstreamRequest, string> GetDownstreamBaseUrl()
        {
            return x =>
            {
                var downstreamUrl = $"{x.Scheme}://{x.Host}";

                if (x.Port != 80 && x.Port != 443)
                {
                    downstreamUrl = $"{downstreamUrl}:{x.Port}";
                }

                return $"{downstreamUrl}/";
            };
        }

        private Func<Response<string>> GetTraceId()
        {
            return () =>
            {
                var traceId = _repo.Get<string>("TraceId");
                if (traceId.IsError)
                {
                    return new ErrorResponse<string>(traceId.Errors);
                }

                return new OkResponse<string>(traceId.Data);
            };
        }

        private Func<Response<string>> GetBaseUrl()
        {
            return () => new OkResponse<string>(_finder.Find());
        }

        private Func<Response<string>> GetUpstreamHost()
        {
            return () =>
            {
                try
                {
                    if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue("Host", out var upstreamHost))
                    {
                        return new OkResponse<string>(upstreamHost.First());
                    }

                    return new ErrorResponse<string>(new CouldNotFindPlaceholderError("{UpstreamHost}"));
                }
                catch
                {
                    return new ErrorResponse<string>(new CouldNotFindPlaceholderError("{UpstreamHost}"));
                }
            };
        }
    }
}

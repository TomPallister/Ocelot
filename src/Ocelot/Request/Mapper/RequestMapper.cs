namespace Ocelot.Request.Mapper
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Extensions;
    using Microsoft.Extensions.Primitives;
    using Ocelot.Configuration;
    using Ocelot.Responses;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;

    public class RequestMapper : IRequestMapper
    {
        private readonly string[] _unsupportedHeaders = { "host" };

        public Response<HttpRequestMessage> Map(HttpRequest request, DownstreamRoute downstreamRoute)
        {
            try
            {
                var httpMethod = MapMethod(request, downstreamRoute);
                var requestMessage = new HttpRequestMessage()
                {
                    Method = httpMethod,
                    Content = MapContent(httpMethod.Method, request),
                    RequestUri = MapUri(request),
                    Version = downstreamRoute.DownstreamHttpVersion,
                };

                MapHeaders(request, requestMessage);

                return new OkResponse<HttpRequestMessage>(requestMessage);
            }
            catch (Exception ex)
            {
                return new ErrorResponse<HttpRequestMessage>(new UnmappableRequestError(ex));
            }
        }

        private static bool BodyIsSupported(string method)
        {
            return method != HttpMethod.Get.Method
                && method != HttpMethod.Head.Method
                && method != HttpMethod.Trace.Method;
        }

        private static HttpContent MapContent(string downstreamMethod, HttpRequest request)
        {
            if (!BodyIsSupported(downstreamMethod)
                || !BodyIsSupported(request.Method))
            {
                return null;
            }

            if (request.Body == null || (request.Body.CanSeek && request.Body.Length <= 0))
            {
                return null;
            }

            var content = new StreamContent(request.Body);
            AddHeaderIfExistsOnRequest("Content-Encoding", content, request);
            AddHeaderIfExistsOnRequest("Content-Disposition", content, request);
            AddHeaderIfExistsOnRequest("Content-Language", content, request);
            AddHeaderIfExistsOnRequest("Content-Length", content, request);
            AddHeaderIfExistsOnRequest("Content-Location", content, request);
            AddHeaderIfExistsOnRequest("Content-MD5", content, request);
            AddHeaderIfExistsOnRequest("Content-Range", content, request);
            AddHeaderIfExistsOnRequest("Content-Type", content, request);
            return content;
        }

        private static void AddHeaderIfExistsOnRequest(string key, HttpContent content, HttpRequest request)
        {
            if (request.Headers.ContainsKey(key))
            {
                content.Headers
                    .TryAddWithoutValidation(key, request.Headers[key].ToList());
            }
        }

        private static HttpMethod MapMethod(HttpRequest request, DownstreamRoute downstreamRoute)
        {
            if (!string.IsNullOrEmpty(downstreamRoute?.DownstreamHttpMethod))
            {
                return new HttpMethod(downstreamRoute.DownstreamHttpMethod);
            }

            return new HttpMethod(request.Method);
        }

        private static Uri MapUri(HttpRequest request)
        {
            return new Uri(request.GetEncodedUrl());
        }

        private void MapHeaders(HttpRequest request, HttpRequestMessage requestMessage)
        {
            foreach (var header in request.Headers)
            {
                if (IsSupportedHeader(header))
                {
                    requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }
        }

        private bool IsSupportedHeader(KeyValuePair<string, StringValues> header)
        {
            return !_unsupportedHeaders.Contains(header.Key.ToLower());
        }
    }
}

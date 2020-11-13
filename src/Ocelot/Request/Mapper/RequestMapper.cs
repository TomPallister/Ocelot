namespace Ocelot.Request.Mapper
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Extensions;
    using Microsoft.Extensions.Primitives;
    using Ocelot.Configuration;
    using Ocelot.Responses;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class RequestMapper : IRequestMapper
    {
        private readonly string[] _unsupportedHeaders = { "host" };

        public async Task<Response<HttpRequestMessage>> Map(HttpRequest request, DownstreamRoute downstreamRoute)
        {
            try
            {
                var requestMessage = new HttpRequestMessage()
                {
                    Content = await MapContent(request),
                    Method = MapMethod(request, downstreamRoute),
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

        private async Task<HttpContent> MapContent(HttpRequest request)
        {
            if (request.Body == null || (request.Body.CanSeek && request.Body.Length <= 0))
            {
                return null;
            }

            // Never change this to StreamContent again, I forgot it doesnt work in #464.
            var content = new ByteArrayContent(await ToByteArray(request.Body));

            if (!string.IsNullOrEmpty(request.ContentType))
            {
                content.Headers
                    .TryAddWithoutValidation("Content-Type", new[] { request.ContentType });
            }

            AddHeaderIfExistsOnRequest("Content-Language", content, request);
            AddHeaderIfExistsOnRequest("Content-Location", content, request);
            AddHeaderIfExistsOnRequest("Content-Range", content, request);
            AddHeaderIfExistsOnRequest("Content-MD5", content, request);
            AddHeaderIfExistsOnRequest("Content-Disposition", content, request);
            AddHeaderIfExistsOnRequest("Content-Encoding", content, request);

            return content;
        }

        private void AddHeaderIfExistsOnRequest(string key, HttpContent content, HttpRequest request)
        {
            if (request.Headers.ContainsKey(key))
            {
                content.Headers
                    .TryAddWithoutValidation(key, request.Headers[key].ToList());
            }
        }

        private HttpMethod MapMethod(HttpRequest request, DownstreamRoute downstreamRoute)
        {
            if (!string.IsNullOrEmpty(downstreamRoute?.DownstreamHttpMethod))
            {
                return new HttpMethod(downstreamRoute.DownstreamHttpMethod);
            }

            return new HttpMethod(request.Method);
        }

        private Uri MapUri(HttpRequest request)
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

        private async Task<byte[]> ToByteArray(Stream stream)
        {
            using (stream)
            {
                using (var memStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memStream);
                    return memStream.ToArray();
                }
            }
        }
    }
}

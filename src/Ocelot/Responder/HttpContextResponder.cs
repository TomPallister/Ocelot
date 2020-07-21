using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;
using Ocelot.Headers;
using Ocelot.Middleware;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ocelot.Responder
{
    /// <summary>
    /// Cannot unit test things in this class due to methods not being implemented
    /// on .net concretes used for testing
    /// </summary>
    public class HttpContextResponder : IHttpResponder
    {
        private readonly IRemoveOutputHeaders _removeOutputHeaders;

        public HttpContextResponder(IRemoveOutputHeaders removeOutputHeaders)
        {
            _removeOutputHeaders = removeOutputHeaders;
        }

        public async Task SetResponseOnHttpContext(HttpContext context, DownstreamResponse response)
        {
            _removeOutputHeaders.Remove(response.Headers);

            foreach (var httpResponseHeader in response.Headers)
            {
                AddHeaderIfDoesntExist(context, httpResponseHeader);
            }

            SetStatusCode(context, (int)response.StatusCode);

            context.Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = response.ReasonPhrase;

            if (response.Content is null)
            {
                return;
            }

            foreach (var httpResponseHeader in response.Content.Headers)
            {
                AddHeaderIfDoesntExist(context, new Header(httpResponseHeader.Key, httpResponseHeader.Value));
            }

            var content = await response.Content.ReadAsStreamAsync();

            if (response.Content.Headers.ContentLength != null)
            {
                AddHeaderIfDoesntExist(context, new Header("Content-Length", new[] { response.Content.Headers.ContentLength.ToString() }));
            }

            using (content)
            {
                if (response.StatusCode != HttpStatusCode.NotModified && context.Response.ContentLength != 0)
                {
                    await content.CopyToAsync(context.Response.Body);
                }
            }
        }

        public void SetErrorResponseOnContext(HttpContext context, int statusCode)
        {
            SetStatusCode(context, statusCode);
        }

        public async Task SetErrorResponseOnContext(HttpContext context, DownstreamResponse response)
        {
            var content = await response.Content.ReadAsStreamAsync();

            if (response.Content.Headers.ContentLength != null)
            {
                AddHeaderIfDoesntExist(context, new Header("Content-Length", new[] { response.Content.Headers.ContentLength.ToString() }));
            }

            using (content)
            {
                if (context.Response.ContentLength != 0)
                {
                    await content.CopyToAsync(context.Response.Body);
                }
            }
        }

        private void SetStatusCode(HttpContext context, int statusCode)
        {
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = statusCode;
            }
        }

        private static void AddHeaderIfDoesntExist(HttpContext context, Header httpResponseHeader)
        {
            if (!context.Response.Headers.ContainsKey(httpResponseHeader.Key))
            {
                context.Response.Headers.Add(httpResponseHeader.Key, new StringValues(httpResponseHeader.Values.ToArray()));
            }
        }
    }
}

namespace Ocelot.Request.Middleware
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;

    public class DownstreamRequest
    {
        private readonly HttpRequestMessage _request;

        public DownstreamRequest(HttpRequestMessage request)
        {
            _request = request;
            Method = _request.Method.Method;
            OriginalString = _request.RequestUri.OriginalString;
            Scheme = _request.RequestUri.Scheme;
            Host = _request.RequestUri.Host;
            Port = _request.RequestUri.Port;
            Headers = _request.Headers;
            AbsolutePath = _request.RequestUri.AbsolutePath;
            Query = _request.RequestUri.Query;
            Content = _request.Content;
        }

        public HttpRequestHeaders Headers { get; }

        public string Method { get; }

        public string OriginalString { get; }

        public string Scheme { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public string AbsolutePath { get; set; }

        public string Query { get; set; }

        public HttpContent Content { get; set; }

        public HttpRequestMessage ToHttpRequestMessage()
        {
            var uriBuilder = new UriBuilder
            {
                Port = Port,
                Host = Host,
                Path = AbsolutePath,
                Query = RemoveLeadingQuestionMark(Query),
                Scheme = Scheme
            };

            _request.RequestUri = uriBuilder.Uri;
            _request.Method = new HttpMethod(Method);
            return _request;
        }

        public string ToUri()
        {
            var uriBuilder = new UriBuilder
            {
                Port = Port,
                Host = Host,
                Path = AbsolutePath,
                Query = RemoveLeadingQuestionMark(Query),
                Scheme = Scheme
            };

            return uriBuilder.Uri.AbsoluteUri;
        }

        public override string ToString()
        {
            return ToUri();
        }

        private string RemoveLeadingQuestionMark(string query)
        {
            if (!string.IsNullOrEmpty(query) && query.StartsWith("?"))
            {
                return query.Substring(1);
            }

            return query;
        }
    }
}

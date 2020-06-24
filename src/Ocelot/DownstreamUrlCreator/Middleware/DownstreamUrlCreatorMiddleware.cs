namespace Ocelot.DownstreamUrlCreator.Middleware
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Ocelot.Configuration;
    using Ocelot.DownstreamRouteFinder.UrlMatcher;
    using Microsoft.AspNetCore.Http;
    using Ocelot.Request.Middleware;
    using Ocelot.DownstreamUrlCreator.UrlTemplateReplacer;
    using Ocelot.Logging;
    using Ocelot.Middleware;
    using Ocelot.Responses;
    using Ocelot.Values;
    using System;
    using System.Threading.Tasks;
    using Ocelot.DownstreamRouteFinder.Middleware;

    public class DownstreamUrlCreatorMiddleware : OcelotMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDownstreamPathPlaceholderReplacer _replacer;

        public DownstreamUrlCreatorMiddleware(RequestDelegate next,
            IOcelotLoggerFactory loggerFactory,
            IDownstreamPathPlaceholderReplacer replacer
            )
                : base(loggerFactory.CreateLogger<DownstreamUrlCreatorMiddleware>())
        {
            _next = next;
            _replacer = replacer;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var downstreamRoute = httpContext.Items.DownstreamRoute();

            var templatePlaceholderNameAndValues = httpContext.Items.TemplatePlaceholderNameAndValues();

            var response = _replacer
                .Replace(downstreamRoute.DownstreamPathTemplate.Value, templatePlaceholderNameAndValues);

            var downstreamRequest = httpContext.Items.DownstreamRequest();

            if (response.IsError)
            {
                Logger.LogDebug("IDownstreamPathPlaceholderReplacer returned an error, setting pipeline error");

                httpContext.Items.UpsertErrors(response.Errors);
                return;
            }

            if (!string.IsNullOrEmpty(downstreamRoute.DownstreamScheme))
            {
                //todo make sure this works, hopefully there is a test ;E
                httpContext.Items.DownstreamRequest().Scheme = downstreamRoute.DownstreamScheme;
            }

            var internalConfiguration = httpContext.Items.IInternalConfiguration();

            if (ServiceFabricRequest(internalConfiguration, downstreamRoute))
            {
                var pathAndQuery = CreateServiceFabricUri(downstreamRequest, downstreamRoute, templatePlaceholderNameAndValues, response);

                //todo check this works again hope there is a test..
                downstreamRequest.AbsolutePath = pathAndQuery.path;
                downstreamRequest.Query = pathAndQuery.query;
            }
            else
            {
                var dsPath = response.Data;

                if (ContainsQueryString(dsPath))
                {
                    downstreamRequest.AbsolutePath = GetPath(dsPath);

                    if (string.IsNullOrEmpty(downstreamRequest.Query))
                    {
                        downstreamRequest.Query = GetQueryString(dsPath);
                    }
                    else
                    {
                        downstreamRequest.Query += GetQueryString(dsPath).Replace('?', '&');
                    }
                }
                else
                {
                    RemoveQueryStringParametersThatHaveBeenUsedInTemplate(downstreamRequest, templatePlaceholderNameAndValues);

                    downstreamRequest.AbsolutePath = dsPath.Value;
                }
            }

            Logger.LogDebug($"Downstream url is {downstreamRequest}");

            await _next.Invoke(httpContext);
        }

        private static void RemoveQueryStringParametersThatHaveBeenUsedInTemplate(DownstreamRequest downstreamRequest, List<PlaceholderNameAndValue> templatePlaceholderNameAndValues)
        {
            foreach (var nAndV in templatePlaceholderNameAndValues)
            {
                var name = nAndV.Name.Replace("{", "").Replace("}", "");

                if (downstreamRequest.Query.Contains(name) &&
                    downstreamRequest.Query.Contains(nAndV.Value))
                {
                    var questionMarkOrAmpersand = downstreamRequest.Query.IndexOf(name, StringComparison.Ordinal);
                    downstreamRequest.Query = downstreamRequest.Query.Remove(questionMarkOrAmpersand - 1, 1);

                    var rgx = new Regex($@"\b{name}={nAndV.Value}\b");
                    downstreamRequest.Query = rgx.Replace(downstreamRequest.Query, "");

                    if (!string.IsNullOrEmpty(downstreamRequest.Query))
                    {
                        downstreamRequest.Query = '?' + downstreamRequest.Query.Substring(1);
                    }
                }
            }
        }

        private string GetPath(DownstreamPath dsPath)
        {
            return dsPath.Value.Substring(0, dsPath.Value.IndexOf("?", StringComparison.Ordinal));
        }

        private string GetQueryString(DownstreamPath dsPath)
        {
            return dsPath.Value.Substring(dsPath.Value.IndexOf("?", StringComparison.Ordinal));
        }

        private bool ContainsQueryString(DownstreamPath dsPath)
        {
            return dsPath.Value.Contains("?");
        }

        private (string path, string query) CreateServiceFabricUri(DownstreamRequest downstreamRequest, DownstreamRoute downstreamRoute, List<PlaceholderNameAndValue> templatePlaceholderNameAndValues, Response<DownstreamPath> dsPath)
        {
            var query = downstreamRequest.Query;
            var serviceName = _replacer.Replace(downstreamRoute.ServiceName, templatePlaceholderNameAndValues);
            var pathTemplate = $"/{serviceName.Data.Value}{dsPath.Data.Value}";
            return (pathTemplate, query);
        }

        private static bool ServiceFabricRequest(IInternalConfiguration config, DownstreamRoute downstreamRoute)
        {
            return config.ServiceProviderConfiguration.Type?.ToLower() == "servicefabric" && downstreamRoute.UseServiceDiscovery;
        }
    }
}

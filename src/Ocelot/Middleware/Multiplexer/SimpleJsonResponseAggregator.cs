﻿using Microsoft.AspNetCore.Http;
using Ocelot.Configuration;
using Ocelot.DownstreamRouteFinder.Middleware;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Ocelot.Middleware.Multiplexer
{
    public class SimpleJsonResponseAggregator : IResponseAggregator
    {
        public async Task Aggregate(ReRoute reRoute, HttpContext originalContext, List<HttpContext> downstreamContexts)
        {
            await MapAggregateContent(originalContext, downstreamContexts);
        }

        private static async Task MapAggregateContent(HttpContext originalContext, List<HttpContext> downstreamContexts)
        {
            var contentBuilder = new StringBuilder();

            contentBuilder.Append("{");

            var responseKeys = downstreamContexts.Select(s => s.Items.DownstreamReRoute().Key).Distinct().ToList();

            for (var k = 0; k < responseKeys.Count; k++)
            {
                var contexts = downstreamContexts.Where(w => w.Items.DownstreamReRoute().Key == responseKeys[k]).ToList();
                if (contexts.Count == 1)
                {
                    if (contexts[0].Items.Errors().Count > 0)
                    {
                        MapAggregateError(originalContext, contexts[0]);
                        return;
                    }

                    var content = await contexts[0].Items.DownstreamResponse().Content.ReadAsStringAsync();
                    contentBuilder.Append($"\"{responseKeys[k]}\":{content}");
                }
                else
                {
                    contentBuilder.Append($"\"{responseKeys[k]}\":");
                    contentBuilder.Append("[");

                    for (var i = 0; i < contexts.Count; i++)
                    {
                        if (contexts[i].Items.Errors().Count > 0)
                        {
                            MapAggregateError(originalContext, contexts[i]);
                            return;
                        }

                        var content = await contexts[i].Items.DownstreamResponse().Content.ReadAsStringAsync();
                        if (string.IsNullOrWhiteSpace(content))
                        {
                            continue;
                        }

                        contentBuilder.Append($"{content}");

                        if (i + 1 < contexts.Count)
                        {
                            contentBuilder.Append(",");
                        }
                    }

                    contentBuilder.Append("]");
                }

                if (k + 1 < responseKeys.Count)
                {
                    contentBuilder.Append(",");
                }
            }

            contentBuilder.Append("}");

            var stringContent = new StringContent(contentBuilder.ToString())
            {
                Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
            };

            originalContext.Items.SetDownstreamResponse(new DownstreamResponse(stringContent, HttpStatusCode.OK, new List<KeyValuePair<string, IEnumerable<string>>>(), "cannot return from aggregate..which reason phrase would you use?"));
        }

        private static void MapAggregateError(HttpContext originalContext, HttpContext downstreamContext)
        {
            originalContext.Items.SetErrors(downstreamContext.Items.Errors());
            originalContext.Items.SetDownstreamResponse(downstreamContext.Items.DownstreamResponse());
        }
    }
}

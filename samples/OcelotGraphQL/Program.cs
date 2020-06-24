using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocelot.Middleware;
using Ocelot.DependencyInjection;
using GraphQL.Types;
using GraphQL;
using Ocelot.Requester;
using Ocelot.Responses;
using System.Net.Http;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;

namespace OcelotGraphQL
{
    public class Hero
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Query
    {
        private readonly List<Hero> _heroes = new List<Hero>
        {
            new Hero { Id = 1, Name = "R2-D2" },
            new Hero { Id = 2, Name = "Batman" },
            new Hero { Id = 3, Name = "Wonder Woman" },
            new Hero { Id = 4, Name = "Tom Pallister" }
        };

        [GraphQLMetadata("hero")]
        public Hero GetHero(int id)
        {
            return _heroes.FirstOrDefault(x => x.Id == id);
        }
    }

    public class GraphQlDelegatingHandler : DelegatingHandler
    {
        private readonly ISchema _schema;

        public GraphQlDelegatingHandler(ISchema schema)
        {
            _schema = schema;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //try get query from body, could check http method :)
            var query = await request.Content.ReadAsStringAsync();

            //if not body try query string, dont hack like this in real world..
            if (query.Length == 0)
            {
                var decoded = WebUtility.UrlDecode(request.RequestUri.Query);
                query = decoded.Replace("?query=", "");
            }

            var result = _schema.Execute(_ =>
            {
                _.Query = query;
            });

            //maybe check for errors and headers etc in real world?
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(result)
            };

            //ocelot will treat this like any other http request...
            return response;
        }
    }

    public class Program
    {
        public static void Main()
        {
            var schema = Schema.For(@"
                type Hero {
                    id: Int
                    name: String
                }

                type Query {
                    hero(id: Int): Hero
                }
            ", _ =>
            {
                _.Types.Include<Query>();
            });

            new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config
                        .SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
                        .AddJsonFile("appsettings.json", true, true)
                        .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", true, true)
                        .AddJsonFile("ocelot.json", false, false)
                        .AddEnvironmentVariables();
                })
                .ConfigureServices(s =>
                {
                    s.AddSingleton<ISchema>(schema);
                    s.AddOcelot()
                        .AddDelegatingHandler<GraphQlDelegatingHandler>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                })
                .UseIISIntegration()
                .Configure(app =>
                {
                    app.UseOcelot().Wait();
                })
                .Build()
                .Run();
        }
    }
}

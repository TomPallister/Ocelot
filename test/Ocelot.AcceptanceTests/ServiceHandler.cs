namespace Ocelot.AcceptanceTests
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Server.Kestrel.Core;

    public class ServiceHandler : IDisposable
    {
        private IWebHost _builder;

        public void GivenThereIsAServiceRunningOn(string baseUrl, RequestDelegate del)
        {
            _builder = new WebHostBuilder()
                .UseUrls(baseUrl)
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .Configure(app =>
                {
                    app.Run(del);
                })
                .Build();

            _builder.Start();
        }

        public void GivenThereIsAServiceRunningOn(string baseUrl, string basePath, RequestDelegate del)
        {
            _builder = new WebHostBuilder()
                .UseUrls(baseUrl)
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .Configure(app =>
                {
                    app.UsePathBase(basePath);
                    app.Run(del);
                })
                .Build();

            _builder.Start();
        }

        public void GivenThereIsAServiceRunningOn(string baseUrl, string basePath, RequestDelegate del, int port, HttpProtocols protocols)
        {
            _builder = new WebHostBuilder()
                .UseUrls(baseUrl)
                .UseKestrel()
                .ConfigureKestrel(serverOptions =>
                {
                    serverOptions.Listen(IPAddress.Loopback, port, listenOptions =>
                        {
                            listenOptions.UseHttps("idsrv3test.pfx", "idsrv3test");
                            listenOptions.Protocols = protocols;
                        });
                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .Configure(app =>
                {
                    app.UsePathBase(basePath);
                    app.Run(del);
                })
                .Build();

            _builder.Start();
        }

        public void GivenThereIsAServiceRunningOn(string baseUrl, string basePath, string fileName, string password, int port, RequestDelegate del)
        {
            _builder = new WebHostBuilder()
                .UseUrls(baseUrl)
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Loopback, port, listenOptions =>
                    {
                        listenOptions.UseHttps(fileName, password);
                    });
                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .Configure(app =>
                {
                    app.UsePathBase(basePath);
                    app.Run(del);
                })
                .Build();

            _builder.Start();
        }

        public async Task StartFakeDownstreamService(string url, string path, Func<HttpContext, Func<Task>, Task> middleware)
        {
            _builder = new WebHostBuilder()
                .ConfigureServices(s => { }).UseKestrel()
                .UseUrls(url)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath);
                    var env = hostingContext.HostingEnvironment;
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: false);
                    config.AddEnvironmentVariables();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                })
                .Configure(app =>
                {
                    app.UseWebSockets();
                    app.Use(middleware);
                })
                .UseIISIntegration()
                .Build();

            await _builder.StartAsync();
        }

        public void Dispose()
        {
            _builder?.Dispose();
        }
    }
}

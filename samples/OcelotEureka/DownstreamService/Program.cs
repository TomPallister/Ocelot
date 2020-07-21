using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace DownstreamService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseUrls($"http://{Environment.MachineName}:5001")
                .UseStartup<Startup>()
                .Build();
    }
}

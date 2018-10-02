using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("ASPNETCORE_HOSTINGSTARTUPASSEMBLIES: " +
                Environment.GetEnvironmentVariable("ASPNETCORE_HOSTINGSTARTUPASSEMBLIES"));
            Console.WriteLine("DOTNET_ADDITIONAL_DEPS: " +
                Environment.GetEnvironmentVariable("DOTNET_ADDITIONAL_DEPS"));

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseSetting(WebHostDefaults.ApplicationKey, "My Custom WebApp") //ASPNETCORE_APPLICATIONKEY
                .ConfigureAppConfiguration((cfg) => cfg.AddEnvironmentVariables("TOREPLACE_"))
                // to jest juz obsluzone przez CreateDefaultBuilder
                //.ConfigureLogging((ctx, log) =>
                //{
                //    log.AddConfiguration(ctx.Configuration.GetSection("Logging"));
                //    log.AddConsole();
                //    log.AddDebug();
                //})
                .UseStartup(typeof(StartupDevelopment).Assembly.FullName);
    }
}

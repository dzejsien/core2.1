using Microsoft.Extensions.Configuration;
using System;
using System.IO;


namespace Code2_1.Configuration
{
    class Program
    {
        public static IConfiguration Configuration;

        static void Main(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.{environmentName}.json")
                .AddEnvironmentVariables("TOREPLACE_");

            Configuration = builder.Build();

            Console.WriteLine($"O_FIRST: {Configuration["O_FIRST"]}");

            var myConfig = new MyConfig();
            Configuration.GetSection("App").Bind(myConfig);

            Console.WriteLine($"O_FIRST: {myConfig.O_First}");

            Console.WriteLine($"UserDAta.Opt1: {myConfig.UserData.Opt1}");

            Console.ReadKey();
        }
    }
}

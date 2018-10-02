using Code2_1.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace WebApp
{
    public class StartupProduction
    {
        private readonly IConfiguration _configuration;

        public StartupProduction(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var appSection = _configuration.GetSection("App");

            services.Configure<MyConfig>(appSection);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.Run(async (context) =>
            {
                var myConfig = app.ApplicationServices.GetService<IOptions<MyConfig>>();
                await context.Response.WriteAsync("PRODUCTION!!: " + myConfig.Value.UserData.Opt1);
            });
        }
    }
}

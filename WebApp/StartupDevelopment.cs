using Code2_1.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WebApp
{
    public class StartupDevelopment
    {
        private readonly IConfiguration _configuration;

        public StartupDevelopment(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var appSection = _configuration.GetSection("App");

            services.Configure<MyConfig>(appSection);

            services.Configure<MyConfig>(opt => opt.OFirst = opt.OFirst ?? "Cfg by delegate");

            services.Configure<MyConfig>("NamedOptions", opt =>
            {
                opt.OFirst = "Named 1";
                opt.Opt1 = "Named 2";
                opt.Opt2 = "Named 3";
            });

            // dla wszystkich named istniejacych zmienia opcje
            // uwaga, wszystkie cfg sa named, nawet pierwsze ust. jest named jako null, zobacz ze nie ma "Cfg by delegate"
            services.ConfigureAll<MyConfig>(opt => opt.OFirst = "Named ALL 1");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDeveloperExceptionPage();

            app.Run(async (context) =>
            {
                var myConfig = app.ApplicationServices.GetService<IOptions<MyConfig>>();
                var logger = app.ApplicationServices.GetService<ILogger<StartupDevelopment>>();
                var loggerFromFac = app.ApplicationServices.GetService<ILoggerFactory>().CreateLogger("Logger.From.FAC");
                logger.LogInformation(">>>>>>>>>>>>>>>>>>>>>>>>>> TEst logger");
                loggerFromFac.LogInformation(">>>>>>>>>>>>>>>>>>>>>>>>>> TEst logger from fac");
                // named sa def jako scope, trzeba brac z context.ReqSer
                var myNamedConfig = context.RequestServices.GetService<IOptionsSnapshot<MyConfig>>().Get("NamedOptions");
                await context.Response.WriteAsync("DEVELOPMENT!!: " + myConfig.Value.UserData.Opt1 + " " + myConfig.Value.OFirst + " " + myNamedConfig.OFirst);
            });
        }
    }
}

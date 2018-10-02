using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace WebApp.FileProviders
{
    public class Startup
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public Startup(IHostingEnvironment env)
        {
            _hostingEnvironment = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var physicalProvider = _hostingEnvironment.ContentRootFileProvider;
            var embeddedProvider = new EmbeddedFileProvider(Assembly.GetEntryAssembly());
            var compositeProvider = new CompositeFileProvider(physicalProvider, embeddedProvider);

            // choose one provider to use for the app and register it
            //services.AddSingleton<IFileProvider>(physicalProvider);
            //services.AddSingleton<IFileProvider>(embeddedProvider);
            services.AddSingleton<IFileProvider>(compositeProvider);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFac)
        {
            var logger = loggerFac.CreateLogger("test.logger");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                var _fileProvider = app.ApplicationServices.GetService<IFileProvider>();
                var contents = _fileProvider.GetDirectoryContents("");

                foreach (var item in contents)
                {
                    if (item.IsDirectory)
                    {
                        logger.LogInformation(item.Name);
                        continue;
                    }

                    logger.LogInformation($"{item.Name} - {item.Length} bytes");

                    if (item.Name.Equals("EmbededExample.sql"))
                    {
                        var token = _fileProvider.Watch(item.Name);

                        token.RegisterChangeCallback(state =>
                        {
                            logger.LogInformation("!!!!!!!!!!!!!!!!!! file was changed");
                        }, null);
                    }
                }



                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}

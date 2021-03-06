﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * trzeba jeszcze sporo zmeinic, dodac zeby to lapalo
 * zmienic runtime na odp. sciezke, wprowdzic dllke i deps do odpowiedniego miejsca
 * ustawic zmienne srodowiskowe
 */


// Use a Hosting Startup Attribute to identify the IHostingStartup implementation.
[assembly: HostingStartup(typeof(Cfg.HostingStartup.StartupDiagnosticsHostingStartup))]

namespace Cfg.HostingStartup
{
    public class StartupDiagnosticsHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            Console.WriteLine("StartupDiagnostics.StartupDiagnosticsHostingStartup");

            builder.ConfigureServices(services =>
            {
                // Create a factory with a GetServices method that can
                // be called in middleware to obtain a list of the app's
                // services.
                Func<IServiceProvider, IServiceDescriptorsService> factory =
                provider => new ServiceDescriptorsService(services);

                // Register the factory in the service container.
                services.AddSingleton(factory);

                // Implement a startup filter that is used to register 
                // two middleware components.
                services.AddSingleton<IStartupFilter, DiagnosticMiddlewareStartupFilter>();
            });
        }
    }

    public class DiagnosticMiddlewareStartupFilter : IStartupFilter
    {
        private readonly IHostingEnvironment _env;

        public DiagnosticMiddlewareStartupFilter(IHostingEnvironment env)
        {
            _env = env;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                // Use a terminal middleware that branches on a request for
                // /services. The middleware uses a factory to obtain the services
                // registered for the app and outputs them in a webpage.
                app.Map("/services", builder => builder.Run(async ctx =>
            {
                    // Make sure this only runs in the Development environment.
                    if (!_env.IsDevelopment())
                {
                    return;
                }

                var sb = new StringBuilder(@"
                        <!DOCTYPE html><html lang=""en""><head><title>All Services</title>
                        <style>body{font-family:Verdana,Geneva,sans-serif;font-size:.8em}
                        li{padding-bottom:10px}</style></head><body>
                        <h1>All Services</h1>
                        <ul>");

                var serviceDescriptorService =
                    ctx.RequestServices.GetService<IServiceDescriptorsService>();

                foreach (var service in serviceDescriptorService.GetServices())
                {
                    sb.Append($"<li><b>{service.FullName}</b> ({service.Lifetime})");
                    if (!string.IsNullOrEmpty(service.ImplementationType))
                    {
                        sb.Append($"<br>{service.ImplementationType}</li>");

                    }
                    else
                    {
                        sb.Append($"</li>");
                    }

                }

                sb.Append("</ul></body></html>");

                await ctx.Response.WriteAsync(sb.ToString());
            }));

                app.UseMiddleware<DiagnosticMiddleware>();

                next(app);
            };
        }
    }

    // Use a middleware to write out diagnostic information from the app.
    public class DiagnosticMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IHostingEnvironment _env;
        private string cr = Environment.NewLine;

        public DiagnosticMiddleware(RequestDelegate next,
                                    ILoggerFactory loggerFactory,
                                    IHostingEnvironment env)
        {
            _next = next;
            _loggerFactory = loggerFactory;
            _env = env;
        }

        public async Task Invoke(HttpContext ctx)
        {
            var path = ctx.Request.Path;

            // Make sure this only runs at the /diag endpoint in the Development environment.
            if (path != "/diag" || !_env.IsDevelopment())
            {
                await _next(ctx);
            }
            else
            {
                var logger = _loggerFactory.CreateLogger("Requests");

                logger.LogDebug("Received request: {METHOD} {PATH}",
                    ctx.Request.Method, ctx.Request.Path);

                ctx.Response.ContentType = "text/plain";

                var sb = new StringBuilder();
                sb.Append($"{DateTimeOffset.Now}{cr}{cr}");
                sb.Append($"Address:{cr}{cr}");
                sb.Append($"Scheme: {ctx.Request.Scheme}{cr}");
                sb.Append($"PathBase: {ctx.Request.PathBase.Value}{cr}");
                sb.Append($"Path: {ctx.Request.Path.Value}{cr}");
                sb.Append($"Query: {ctx.Request.QueryString.Value}{cr}{cr}");
                sb.Append($"Connection:{cr}{cr}");
                sb.Append($"RemoteIp: {ctx.Connection.RemoteIpAddress}{cr}");
                sb.Append($"RemotePort: {ctx.Connection.RemotePort}{cr}");
                sb.Append($"LocalIp: {ctx.Connection.LocalIpAddress}{cr}");
                sb.Append($"LocalPort: {ctx.Connection.LocalPort}{cr}");
                sb.Append($"ClientCert: {ctx.Connection.ClientCertificate}{cr}{cr}");
                sb.Append($"Headers:{cr}{cr}");

                sb.Append($"{cr}Environment Variables:{cr}{cr}");

                var vars = Environment.GetEnvironmentVariables();
                foreach (var key in vars.Keys.Cast<string>()
                    .OrderBy(key => key, StringComparer.OrdinalIgnoreCase))
                {
                    sb.Append($"{key}: {vars[key]}{cr}");
                }

                await ctx.Response.WriteAsync(sb.ToString());
            }
        }
    }
}

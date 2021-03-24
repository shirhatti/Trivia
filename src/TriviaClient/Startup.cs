using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TriviaGame;

namespace TriviaClient
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(logging =>
            {
                logging.AddSeq($"http://{Configuration.GetConnectionString("seq")}")
                       .AddSimpleConsole(o => o.IncludeScopes = true);
            });
            services.AddOpenTelemetryTracing(tracing =>
            {
                tracing
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(typeof(Startup).Assembly.GetName().Name))
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddGrpcClientInstrumentation()
                    .SetSampler(new AlwaysOnSampler())
                    .AddZipkinExporter(options =>
                    {
                        options.Endpoint = new Uri($"http://{Configuration.GetConnectionString("zipkin")}/api/v2/spans");
                    });
            });

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddGrpcClient<Trivia.TriviaClient>(options =>
            {
                options.Address = GetUri(Configuration, "TriviaServer");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }

        private Uri GetUri(IConfiguration configuration, string name)
        {
            return new Uri($"https://{configuration[$"service:{name}:https:host"]}:{configuration[$"service:{name}:https:port"]}");
        }
    }
}

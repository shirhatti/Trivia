using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;
using TriviaServer.Hubs;

namespace TriviaServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

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
                    .SetSampler(new AlwaysOnSampler())
                    .AddZipkinExporter(options =>
                    {
                        options.Endpoint = new Uri($"http://{Configuration.GetConnectionString("zipkin")}/api/v2/spans");
                    });
            });

            services.AddRazorPages();
            services.AddSignalR(options =>
            {
                options.MaximumParallelInvocationsPerClient = 5;
            });
            services.AddGrpc();
            services.AddSingleton<TriviaLobby>();
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
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization(); ;

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapHub<ScoreHub>("scoreHub", options =>
                {
                });
                endpoints.MapGrpcService<TriviaService>();
            });
        }
    }
}

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using Codle.Api.Settings;
using Codle.Api.Middleware;

namespace Codle.Api
{
    public class Startup
    {
        #region -- Builder --

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        #endregion

        #region -- Methods --

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSignalR();

            SwaggerConfigureServices(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();       

            app.UseApiVersioning();

            SwaggerConfigure(app, provider);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<Streaming>("streaming");
            });
        }

        #endregion

        #region -- Swagger --

        private static void SwaggerConfigureServices(IServiceCollection services)
        {
            services.AddApiVersioning(api =>
            {
                api.DefaultApiVersion = new ApiVersion(1, 0);
                api.ReportApiVersions = true;
                api.AssumeDefaultVersionWhenUnspecified = true;
            });

            services.AddVersionedApiExplorer(api =>
            {
                api.GroupNameFormat = "'v'VVV";
                api.SubstituteApiVersionInUrl = true;
            });

            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

            services.AddSwaggerGen();
        }
                
        private static void SwaggerConfigure(IApplicationBuilder app, IApiVersionDescriptionProvider provider)
        {
            app.UseSwagger();

            app.UseSwaggerUI(options =>
            {
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint(
                    $"/swagger/{description.GroupName}/swagger.json",
                    description.GroupName.ToUpperInvariant());
                }

                options.DocExpansion(DocExpansion.List);
            });
        }

        #endregion
    }
}

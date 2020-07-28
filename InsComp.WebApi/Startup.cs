using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;

namespace InsComp.WebApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddControllers();

            //services.AddMvcCore().AddApiExplorer().SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
            //    .AddAuthorization()
            //    .AddJsonFormatters();

            var jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            services.AddMvcCore(mvcOptions =>
                {
                    mvcOptions.EnableEndpointRouting = false;
                    //mvcOptions.OutputFormatters.Add(new SystemTextJsonOutputFormatter(jsonSerializerOptions));
                })
                .AddAuthorization();

            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = "https://localhost:5001";
                    options.RequireHttpsMetadata = false;
                    options.LegacyAudienceValidation = false;
                    options.ApiName = "api1";
                });
            //.AddJwtBearer("Bearer", options =>
            //{
            //    options.Authority = "https://localhost:5001";
            //    options.RequireHttpsMetadata = false;

            //    options.Audience = "api1";
            //});

            services.AddCors(options =>
            {
                // this defines a CORS policy called "default"
                options.AddPolicy("default", policy =>
                {
                    policy.WithOrigins("https://localhost:5003")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseCors("default");

            app.UseAuthentication();

            //app.UseAuthorization();

            app.UseMvc();
        }
    }
}

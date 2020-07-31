// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using IdentityServer4.Quickstart.UI.Data;
using IdentityServer4.Quickstart.UI.Models;
using IdentityServer4.Quickstart.UI.Settings;
using IdentityServerHost.Quickstart.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;

namespace IdentityServer4.Quickstart.UI
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Environment { get; }

        private readonly string _allowSpecificOrigins = "DevelopmentAllowSpecificOrigins";
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(_allowSpecificOrigins, builder1 =>
                {
                    builder1.WithOrigins("http://localhost:3000/")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowAnyOrigin();
                });
            });

            IIdentityServerBuilder builder = IntegrateAspNetIdentity(services);
            ConfigureIdentityStores(services, builder);
            ConfigureSigningCredential(services, builder);




            // uncomment, if you want to add an MVC-based UI
            services.AddControllersWithViews();

            //var builder = services.AddIdentityServer(options =>
            //{
            //    // see https://identityserver4.readthedocs.io/en/latest/topics/resources.html
            //    options.EmitStaticAudienceClaim = true;
            //})
            //.AddInMemoryIdentityResources(Config.IdentityResources)
            //.AddInMemoryApiResources(Config.GetApiResources())
            //.AddInMemoryApiScopes(Config.ApiScopes)
            //.AddInMemoryClients(Config.Clients)
            //.AddTestUsers(TestUsers.Users);

            // not recommended for production - you need to store your key material somewhere secure
            //builder.AddDeveloperSigningCredential();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = $"IdentityServer4.Quickstart.UI", Version = "v1" });
                c.DescribeAllEnumsAsStrings();
            });
        }

        private IIdentityServerBuilder IntegrateAspNetIdentity(IServiceCollection services)
        {
            services.AddIdentity<AuthUser, IdentityRole<int>>(options =>
                {
                    options.User.RequireUniqueEmail = true;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireDigit = false;
                    options.Password.RequiredLength = 6;


                    options.Password.RequireNonAlphanumeric = false;
                    options.SignIn.RequireConfirmedEmail = true;
                })
                .AddEntityFrameworkStores<AuthDbContext>()
                .AddDefaultTokenProviders();

            return services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            }).AddAspNetIdentity<AuthUser>();
        }

        private void ConfigureIdentityStores(IServiceCollection services, IIdentityServerBuilder identityBuilder)
        {
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            string connectionString = Configuration.GetConnectionString("InsCompAuthConnection");

            services.AddDbContext<AuthDbContext>(options => options.UseSqlServer(connectionString,
                sql => sql.MigrationsAssembly(migrationsAssembly)));

            identityBuilder.AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
                    sql => sql.MigrationsAssembly(migrationsAssembly));
            });

            identityBuilder.AddOperationalStore(options =>
            {
                options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
                    sql => sql.MigrationsAssembly(migrationsAssembly));
                options.EnableTokenCleanup = true;
                //options.TokenCleanupInterval = 3000; // frequency in seconds to cleanup stale grants. 15 is useful during debugging
            });
        }

        private void ConfigureSigningCredential(IServiceCollection services, IIdentityServerBuilder identityBuilder)
        {
            SigningCredentialConfig signingCredentialConfig = Configuration
                .GetSection("SigningCredentialConfig")
                .Get<SigningCredentialConfig>();

            identityBuilder 
            //.AddSigningCredential(new X509Certificate2(signingCredentialConfig.CertName, signingCredentialConfig.CertPassword));
            .AddDeveloperSigningCredential();
        }

        public void Configure(IApplicationBuilder app)
        {
            InitializeDatabase(app);

            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // uncomment if you want to add MVC
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors(_allowSpecificOrigins);
            app.UseIdentityServer();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", $"IdentityServer4.Quickstart.UI");
            });

            // uncomment, if you want to add MVC
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }

        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var serviceProvider = serviceScope.ServiceProvider;

                var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
                var authDbContext = serviceScope.ServiceProvider.GetRequiredService<AuthDbContext>();
                var configurationDbContext = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                var persistedGrantDbContext = serviceProvider.GetRequiredService<PersistedGrantDbContext>();

                authDbContext.Database.Migrate();
                configurationDbContext.Database.Migrate();
                persistedGrantDbContext.Database.Migrate();

                if (!roleManager.Roles.Any())
                {
                    foreach (var role in Config.GetRoles())
                    {
                        roleManager.CreateAsync(role).Wait();
                    }
                }

                if (!configurationDbContext.Clients.Any())
                {
                    configurationDbContext.Clients.AddRange(Config.Clients.Select(c => c.ToEntity()));
                }

                if (!configurationDbContext.IdentityResources.Any())
                {
                    configurationDbContext.IdentityResources.AddRange(Config.IdentityResources.Select(ir => ir.ToEntity()));
                }

                if (!configurationDbContext.ApiResources.Any())
                {
                    configurationDbContext.ApiResources.AddRange(Config.GetApiResources().Select(ar => ar.ToEntity()));
                }

                if (!configurationDbContext.ApiScopes.Any())
                {
                    configurationDbContext.ApiScopes.AddRange(Config.ApiScopes.Select(asc => asc.ToEntity()));
                }

                configurationDbContext.SaveChanges();
            }
        }
    }
}

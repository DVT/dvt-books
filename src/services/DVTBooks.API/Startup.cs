using System;
using System.IO;
using System.Reflection;
using DVTBooks.API.Entities;
using DVTBooks.API.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace DVTBooks.API
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
            services.AddControllers();

            string connectionString = Configuration.GetConnectionString("DVTBooksDb");
            string migrationsAssembly = typeof(BooksDbContext).GetTypeInfo().Assembly.GetName().Name;

            services.AddDbContext<BooksDbContext>(options =>
            {
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(migrationsAssembly);
                    sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                });
            });

            services.AddCors();

            services.AddResponseCaching();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(jwtBearerOptions =>
                {
                    jwtBearerOptions.Audience = Configuration["Auth0:Audience"];
                    jwtBearerOptions.Authority = Configuration["Auth0:Domain"];
                });
            
            services
                .AddMvc(options =>
                {
                    options.CacheProfiles.Add("Static", new CacheProfile { Location = ResponseCacheLocation.Any, Duration = 86400/*1 day*/ });
                })
                .AddNewtonsoftJson(options => options.SerializerSettings.Configure());

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Books API", Version = "v1" });

                c.IncludeXmlComments(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DVTBooks.API.xml"));
            });

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseResponseCaching();
            app.UseSwagger(c =>
            {
                c.RouteTemplate = "api-docs/{documentName}/swagger.json";
            });

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("api-docs/v1/swagger.json", "DVT Books API");
                c.OAuthAppName("DVT Books API");
                c.OAuthClientId("dvt-books-api");
            });

            app.UseReDoc();

            app.UseRouting();

            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().WithExposedHeaders("Location"));

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}

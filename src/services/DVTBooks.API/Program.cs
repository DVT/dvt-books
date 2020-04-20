using System;
using DVTBooks.API.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;

namespace DVTBooks.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IHost host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                BooksDbContext context = scope.ServiceProvider.GetService<BooksDbContext>();

                Policy
                    .Handle<SqlException>()
                    .WaitAndRetry(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)))
                    .Execute(() =>
                    {
                        context.Database.Migrate();
                        new BooksDbContextSeed().SeedAsync(context).Wait();
                    });
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}

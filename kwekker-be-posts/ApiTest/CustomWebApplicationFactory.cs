using System;
using System.Linq;
using Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ApiTest;

public class CustomWebApplicationFactory<Program> : WebApplicationFactory<Program> where Program : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                     typeof(DbContextOptions<KwekkerContext>));

            services.Remove(descriptor);

            services.AddDbContext<KwekkerContext>(options => { options.UseInMemoryDatabase("InMemoryDbForTesting"); });
            services.AddScoped<KwekkerContextSeed>();
            var sp = services.BuildServiceProvider();

            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<KwekkerContext>();
            var logger = scopedServices
                .GetRequiredService<ILogger<CustomWebApplicationFactory<Program>>>();
            var seeder = scopedServices.GetRequiredService<KwekkerContextSeed>();

            db.Database.EnsureCreated();

            try
            {
                seeder.Seed();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred seeding the " +
                                    "database with test messages. Error: {Message}", ex.Message);
            }
        });
    }
}
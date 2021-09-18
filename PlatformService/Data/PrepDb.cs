using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PlatformService.Models;

namespace PlatformService.Data
{
    public static class PrepDb
    {
        public static void PrepPopulation(IApplicationBuilder app, bool isProduction)
        {
            using(var serviceScope = app.ApplicationServices.CreateScope())
            {
                SeedData(serviceScope.ServiceProvider.GetService<AppDbContext>(), isProduction);
            }
        }

        private static void SeedData(AppDbContext context, bool isProduction)
        {
            if(isProduction)
            {
                System.Console.WriteLine("--> Attemting to apply database migrations...");
                try
                {
                    context.Database.Migrate();
                    System.Console.WriteLine("--> Database migrations succeeded.");
                }
                catch(Exception ex)
                {
                    System.Console.WriteLine($"Database migration error: {ex.Message}");
                }
            }

            if(!context.Platforms.Any())
            {
                System.Console.WriteLine("--> Seeding data...");
                context.Platforms.AddRange(
                    new Platform() { Name="Dot Net", Publisher="Microsoft", Cost = "Free" },
                    new Platform() { Name="SQL Server Express", Publisher="Microsoft", Cost = "Free" },
                    new Platform() { Name="Kubernetes", Publisher="Cloud Native Computing Foundation", Cost = "Free" }
                );

                context.SaveChanges();
            }
        }
    }
}
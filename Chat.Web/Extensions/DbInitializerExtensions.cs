using Chat.Web.Data;
using Chat.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Chat.Web.Extensions
{
    public static class DbInitializerExtensions
    {
        public static IHost SeedData(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                try
                {
                    var services = scope.ServiceProvider;
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

                    DbInitializer.Initialize(context, userManager).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error initializing the database" + ex.Message);
                }
            }
            return host;
        }
    }
}

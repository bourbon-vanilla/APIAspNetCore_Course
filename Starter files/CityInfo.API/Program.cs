using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CityInfo.API.Contexts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace CityInfo.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = NLogBuilder
                .ConfigureNLog("nlog.config")
                .GetCurrentClassLogger();
            try
            {
                logger.Info("Initializing applicatiion...");
                var host = CreateHostBuilder(args).Build();

                using(var scope = host.Services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<CityInfoContext>();

                    // if for demo purposes - delete existing & create fresh database on sql-server
                    dbContext.Database.EnsureDeleted();
                    // You
                    var createdNow = dbContext.Database.EnsureCreated();
                    // or if you dont delete you migrate the existing database, if the db is not the latest
                    if (!createdNow)
                        dbContext.Database.Migrate(); // The migration will be done only if not latest db schema
                }
                    
                host.Run();
                logger.Info("Initializing application done.");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Application stopped because of exception");
                throw;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseNLog();
                });
    }
}

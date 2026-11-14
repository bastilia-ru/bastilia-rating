using Bastilia.Rating.Database;
using JoinRpg.Common.WebInfrastructure;
using JoinRpg.Common.WebInfrastructure.EfCoreMigration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Bastilia.Rating.Migrator;

internal class Program
{
    private static void Main(string[] args)
    {
        IHostBuilder builder = Host.CreateDefaultBuilder(args);
        builder.UseJoinSerilog("Bastilia.Rating.Migrator");
        builder.ConfigureServices((context, services) =>
        {
            services.AddMigrationsLauncher();
            services.RegisterMigrator<AppDbContext>(context.Configuration, context.HostingEnvironment, "BastiliaRating", options => options.UseOpenIddict());
        });

        builder.Build().Run();
    }
}

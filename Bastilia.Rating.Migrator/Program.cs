using Bastilia.Rating.Database;
using JoinRpg.Common.WebInfrastructure.EfCoreMigration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Bastilia.Rating.Migrator;

internal class Program
{
    private static void Main(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddMigrationsLauncher();
        builder.Services.RegisterMigrator<AppDbContext>(builder.Configuration, builder.Environment, "BastiliaRating", options => options.UseOpenIddict());

        builder.Build().Run();
    }


}
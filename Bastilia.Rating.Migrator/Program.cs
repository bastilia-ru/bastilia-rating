using Bastilia.Rating.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Bastilia.Rating.Migrator;

internal class Program
{
    private static void Main(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddRatingDal(builder.Configuration, builder.Environment);
        builder.Services.AddHostedService<MigrationsLauncher>();
        builder.Services.AddScoped<IMigratorService, MigrateEfCoreHostService<AppDbContext>>();

        builder.Build().Run();
    }


}
using Bastilia.Rating.Database.DbServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Bastilia.Rating.Database
{
    public static class Registration
    {
        public static void RegisterRatingDal(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            services.AddJoinEfCoreDbContext<AppDbContext>(configuration, environment, "BastiliaRating");
            services.AddTransient<IBastiliaMemberRepository, BastiliaMemberRepository>();
            services.AddTransient<IBastiliaProjectRepository, BastiliaProjectRepository>();
            services.AddTransient<IBastiliaTemplateRepository, BastiliaTemplateRepository>();
            services.AddTransient<IBastiliaKograIgraRepository, BastiliaKograIgraRepository>();

            services.AddTransient<IProjectService, ProjectService>();
            services.AddTransient<IAchievementService, AchievementService>();
            services.AddTransient<IUserDbService, UserDbService>();
        }
    }
}

using Bastilia.Rating.Domain;
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
        }
    }
}

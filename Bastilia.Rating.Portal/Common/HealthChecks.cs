using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Bastilia.Rating.Portal.Common;

public static class HealthChecks
{
    /// <summary>
    /// Добавляет чек базы данных в /health/ready. Сам чек "BastiliaRating-db" регистрируется
    /// в <c>AddRatingDal()</c> (пакет JoinRpg.Common.EntityFrameworkCore) — здесь мы ставим
    /// ему тег "ready", а не заводим второй чек с тем же SQL.
    /// </summary>
    public static IServiceCollection AddDatabaseHealthCheck(this IServiceCollection services, IConfiguration configuration)
    {
        if (configuration.GetConnectionString("BastiliaRating") is null)
        {
            return services;
        }

        return services.PostConfigure<HealthCheckServiceOptions>(options =>
        {
            foreach (var registration in options.Registrations)
            {
                if (registration.Name == "BastiliaRating-db")
                {
                    registration.Tags.Add("ready");
                }
            }
        });
    }
}

using JoinRpg.Common.WebInfrastructure.Job;

namespace Bastilia.Rating.Portal.AppServices.Jobs;

internal static class JobsRegistration
{
    public static void AddBastiliaJobs(this IServiceCollection services)
    {
        services.AddMidnightJobs();
        services.AddJob<TestDailyJob>();
        services.AddTransient<IJobImpersonator, RobotJobImpersonator>();
    }
}

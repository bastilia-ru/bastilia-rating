using JoinRpg.Common.WebInfrastructure.Job;

namespace Bastilia.Rating.Portal.AppServices.Jobs;

/// <summary>
/// Джоба-заглушка для проверки инфраструктуры ежедневных джоб. Удалить перед первой реальной джобой.
/// Джобы обязаны быть идемпотентными: если вчера не отработали — сегодня должны доделать за вчера.
/// </summary>
internal sealed class TestDailyJob(ILogger<TestDailyJob> logger) : IJob
{
    public Task RunOnce(CancellationToken cancellationToken)
    {
        logger.LogInformation("Test daily job executed");
        return Task.CompletedTask;
    }
}

using Bastilia.Rating.Database.Entities;
using EntityFramework.Exceptions.Common;
using JoinRpg.Common.WebInfrastructure.Job;

namespace Bastilia.Rating.Database;

/// <summary>
/// Журнал запусков джоб: «один запуск в сутки» через уникальный индекс (JobName, DayOfRun).
/// Порт JoinRpg.Dal.JobService.JobRepository.
/// </summary>
internal class DailyJobRepository(IDbContextFactory<AppDbContext> contextFactory) : IJobRepository
{
    async Task<bool> IJobRepository.TryInsertJobRecord(JobId jobId)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync();

        if (await dbContext.DailyJobRuns.ByJobId(jobId).AnyAsync())
        {
            // Проверим, нет ли такой записи уже, чтобы не засирать лог ошибками
            return false;
        }

        _ = dbContext.DailyJobRuns.Add(new DailyJobRunEntity()
        {
            DayOfRun = jobId.DayOfRun,
            JobName = jobId.JobName,
            MachineName = Environment.MachineName,
        });

        try
        {
            _ = await dbContext.SaveChangesAsync();
        }
        catch (UniqueConstraintException)
        {
            return false; // Проиграли гонку
        }

        return true; // Выиграли, запускайте джобу
    }

    async Task<bool> IJobRepository.TrySetJobCompleted(JobId jobId) => await TrySetJobStatus(jobId, DailyJobStatus.Completed);

    async Task<bool> IJobRepository.TrySetJobFailed(JobId jobId) => await TrySetJobStatus(jobId, DailyJobStatus.Error);

    private async Task<bool> TrySetJobStatus(JobId jobId, DailyJobStatus targetStatus)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync();

        var totalRows = await dbContext.DailyJobRuns
            .ByJobId(jobId)
            // Всегда трогаем только джобы, запущенные нами и в состоянии Started
            .Where(d => d.MachineName == Environment.MachineName && d.JobStatus == DailyJobStatus.Started)
            .ExecuteUpdateAsync(setters => setters.SetProperty(d => d.JobStatus, targetStatus));

        return totalRows switch
        {
            0 => false,
            1 => true,
            _ => throw new InvalidOperationException("Unexpected — too many rows updated"),
        };
    }
}

file static class DailyJobRunQueries
{
    public static IQueryable<DailyJobRunEntity> ByJobId(this IQueryable<DailyJobRunEntity> dailyJobRuns, JobId jobId)
        => dailyJobRuns.Where(d => d.JobName == jobId.JobName && d.DayOfRun == jobId.DayOfRun);
}

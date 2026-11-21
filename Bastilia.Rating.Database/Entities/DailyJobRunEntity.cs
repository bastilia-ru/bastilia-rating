using System.ComponentModel.DataAnnotations;

namespace Bastilia.Rating.Database.Entities;

[Index(nameof(JobName), nameof(DayOfRun), IsUnique = true)]
public class DailyJobRunEntity
{
    public int DailyJobRunId { get; set; }

    [MaxLength(1024)]
    public required string JobName { get; set; }
    public required DateOnly DayOfRun { get; set; }

    public DailyJobStatus JobStatus { get; set; } = DailyJobStatus.Started;

    /// <summary>
    /// Имя машины (=podname), где была запущена джоба
    /// </summary>
    [MaxLength(1024)]
    public required string MachineName { get; set; }
}

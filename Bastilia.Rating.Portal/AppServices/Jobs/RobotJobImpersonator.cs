using JoinRpg.Common.WebInfrastructure.Job;

namespace Bastilia.Rating.Portal.AppServices.Jobs;

/// <summary>
/// No-op имперсонация джоб: в проекте нет системы имперсонации и робота-пользователя
/// (в joinrpg это RobotJobImpersonator через IImpersonateAccessor). Заменить реализацией, если появится робот.
/// </summary>
internal sealed class RobotJobImpersonator : IJobImpersonator
{
    public void StartAsRobot()
    {
    }

    public void Stop()
    {
    }
}

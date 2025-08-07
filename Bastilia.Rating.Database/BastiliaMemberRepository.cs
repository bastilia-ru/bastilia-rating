using Bastilia.Rating.Domain;
using Microsoft.EntityFrameworkCore;

namespace Bastilia.Rating.Database;

public class BastiliaMemberRepository(AppDbContext context) : IBastiliaMemberRepository
{
    public async Task<BastiliaMember?> GetByIdAsync(int userId)
    {
        var user = await context.Users
            .Include(u => u.BastiliaStatuses)
            .Include(u => u.ProjectAdmins)
                .ThenInclude(pa => pa.Project)
            .Include(u => u.Achievements)
                .ThenInclude(a => a.Template)
            .Include(u => u.Achievements)
                .ThenInclude(a => a.GrantedByUser)
            .Include(u => u.Achievements)
                .ThenInclude(a => a.RemovedByUser)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.JoinrpgUserId == userId);

        if (user == null) return null;

        return new BastiliaMember(
            JoinrpgUserId: user.JoinrpgUserId,
            Username: user.Username,
            AvatarUrl: user.AvatarUrl,
            ParticipateInRating: user.ParticipateInRating,
            StatusHistory: user.BastiliaStatuses
                .Select(s => new BastiliaStatusHistory(
                    s.BeginDate,
                    s.StatusType,
                    s.EndDate))
                .ToList()
                .AsReadOnly(),
            HisProjects: user.ProjectAdmins
                .Select(pa => new ProjectAdminInfo(
                    pa.ProjectId,
                    pa.Project.ProjectName,
                    pa.AddDate,
                    pa.RemoveDate))
                .ToList()
                .AsReadOnly(),
            Achievements: user.Achievements
                .Where(a => a.RemovedDate == null)
                .Select(a => new MemberAchievement(
                    a.Template.AchievementName,
                    a.Template.AchievementDescription,
                    new Uri(a.Template.AchievementImageUrl),
                    a.Template.AchievementRatingValue,
                    a.GrantedDate,
                    a.GrantedByUser.Username,
                    a.RemovedDate,
                    a.RemovedByUser.Username,
                    a.ExpirationDate
                    ))
                .ToList()
                .AsReadOnly());
    }
}
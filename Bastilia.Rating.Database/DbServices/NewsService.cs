using Bastilia.Rating.Database.Entities;
using Bastilia.Rating.Domain;

namespace Bastilia.Rating.Database.DbServices
{
    internal class NewsService(AppDbContext appDbContext, ITelegramNewsSender telegramSender) : IProjectNewsService
    {
        public async Task<ProjectNews> Publish(int projectId, int authorUserId, string newsText)
        {
            var project = await appDbContext.BastiliaProjects.FirstOrDefaultAsync(x => x.BastiliaProjectId == projectId)
                ?? throw new InvalidOperationException($"Project {projectId} not found");

            var author = await appDbContext.Set<User>().FindAsync(authorUserId)
                ?? throw new InvalidOperationException($"User {authorUserId} not found");

            var newsEntity = new ProjectNewsEntity
            {
                Project = project,
                AuthorUserId = authorUserId,
                Author = author,
                CreatedAt = DateTimeOffset.UtcNow,
                Text = newsText,
                SendStatus = NewsSendStatus.NotSent,
            };

            await appDbContext.Set<ProjectNewsEntity>().AddAsync(newsEntity);
            await appDbContext.SaveChangesAsync();

            var result = await telegramSender.Send(BastiliaNewsRepository.ToNews(newsEntity), project.ProjectName, project.Slug);
            ApplySendResult(newsEntity, result);
            await appDbContext.SaveChangesAsync();

            return BastiliaNewsRepository.ToNews(newsEntity);
        }

        public async Task<ProjectNews> RetrySend(int newsId)
        {
            var newsEntity = await appDbContext.Set<ProjectNewsEntity>()
                .Include(x => x.Project)
                .Include(x => x.Author)
                .FirstOrDefaultAsync(x => x.ProjectNewsId == newsId)
                ?? throw new InvalidOperationException($"News {newsId} not found");

            var result = await telegramSender.Send(
                BastiliaNewsRepository.ToNews(newsEntity),
                newsEntity.Project.ProjectName,
                newsEntity.Project.Slug);
            ApplySendResult(newsEntity, result);
            await appDbContext.SaveChangesAsync();

            return BastiliaNewsRepository.ToNews(newsEntity);
        }

        private static void ApplySendResult(ProjectNewsEntity newsEntity, NewsSendResult result)
        {
            if (result.Sent)
            {
                newsEntity.SendStatus = NewsSendStatus.Sent;
                newsEntity.TelegramError = null;
            }
            else
            {
                newsEntity.SendStatus = NewsSendStatus.NotSent;
                newsEntity.TelegramError = result.ErrorMessage;
            }
        }
    }
}

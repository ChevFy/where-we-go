
using Microsoft.EntityFrameworkCore;

using where_we_go.Database;
using where_we_go.Models.Enums;
using where_we_go.DTO;

namespace where_we_go.Service
{
    public class CronService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CronService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

        public CronService(IServiceScopeFactory scopeFactory, ILogger<CronService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("-----CronService started------");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateExpiredPostsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error : while updating expired posts.");
                }

                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("------CronService stopped-----");
        }

        private async Task UpdateExpiredPostsAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var now = DateTime.Now;

            var expiredPosts = await dbContext.Posts
                .Include(p => p.Participants)
                .Where(p => (p.DateDeadline <= now
                         && p.Status != PostStatus.Closed
                         && p.Status != PostStatus.Cancelled)
                        || (p.EventDate <= now
                         && p.Status != PostStatus.Cancelled
                         && p.Status != PostStatus.Completed))
                .ToListAsync();

            if (expiredPosts.Count == 0) return;

            foreach (var post in expiredPosts)
            {
                // if participants count meets minimum required, mark as Closed, else mark as Cancelled
                var participantCount = await dbContext.Participants.CountAsync(p => p.PostId == post.PostId && p.Status == ParticipantStatus.Approved);
                if (post.EventDate <= now)
                {
                    post.Status = PostStatus.Completed;
                    await notificationService.CreateNotificationAsync(new NotificationCreateDto
                    {
                        UserId = post.UserId,
                        PostId = post.PostId,
                        Content = $"Your activity '{post.Title}' is starting now.",
                        Link = $"/Post/PostDetail/{post.PostId}",
                        Type = NotificationType.PostCompleted,
                    });
                    foreach (var participant in post.Participants.Where(p => p.Status == ParticipantStatus.Approved))
                    {
                        await notificationService.CreateNotificationAsync(new NotificationCreateDto
                        {
                            UserId = participant.UserId,
                            PostId = post.PostId,
                            Content = $"The activity '{post.Title}' you joined is starting now.",
                            Link = $"/Post/PostDetail/{post.PostId}",
                            Type = NotificationType.PostCompleted,
                        });
                    }
                }
                else if (participantCount >= post.MinParticipants)
                {
                    post.Status = PostStatus.Closed;
                    await notificationService.CreateNotificationAsync(new NotificationCreateDto
                    {
                        UserId = post.UserId,
                        PostId = post.PostId,
                        Content = $"Your activity '{post.Title}' has expired and is now closed.",
                        Link = $"/Post/PostDetail/{post.PostId}",
                        Type = NotificationType.PostExpiredFull,
                    });
                    foreach (var participant in post.Participants.Where(p => p.Status == ParticipantStatus.Approved))
                    {
                        await notificationService.CreateNotificationAsync(new NotificationCreateDto
                        {
                            UserId = participant.UserId,
                            PostId = post.PostId,
                            Content = $"The activity '{post.Title}' you joined has expired and is now closed.",
                            Link = $"/Post/PostDetail/{post.PostId}",
                            Type = NotificationType.PostExpiredFull,
                        });
                    }

                }
                else
                {
                    post.Status = PostStatus.Cancelled;
                    await notificationService.CreateNotificationAsync(new NotificationCreateDto
                    {
                        UserId = post.UserId,
                        PostId = post.PostId,
                        Content = $"Your activity '{post.Title}' has expired and is now cancelled.",
                        Link = $"/Post/PostDetail/{post.PostId}",
                        Type = NotificationType.PostExpiredNotFull,
                    });
                    foreach (var participant in post.Participants.Where(p => p.Status == ParticipantStatus.Approved))
                    {
                        await notificationService.CreateNotificationAsync(new NotificationCreateDto
                        {
                            UserId = participant.UserId,
                            PostId = post.PostId,
                            Content = $"The activity '{post.Title}' you joined has expired and is now cancelled.",
                            Link = $"/Post/PostDetail/{post.PostId}",
                            Type = NotificationType.PostExpiredNotFull,
                        });
                    }
                }
            }

            await dbContext.SaveChangesAsync();

            _logger.LogInformation("Updated {Count} expired post(s) to Closed status.", expiredPosts.Count);
        }
    }
}
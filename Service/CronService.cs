
using Microsoft.EntityFrameworkCore;

using where_we_go.Database;
using where_we_go.Models.Enums;

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

            var now = DateTime.UtcNow;

            var expiredPosts = await dbContext.Posts
                .Where(p => p.DateDeadline <= now
                         && p.Status != PostStatus.Ended
                         && p.Status != PostStatus.Delete)
                .ToListAsync();

            if (expiredPosts.Count == 0) return;

            foreach (var post in expiredPosts)
            {
                post.Status = PostStatus.Ended;
            }

            await dbContext.SaveChangesAsync();

            _logger.LogInformation("Updated {Count} expired post(s) to Ended status.", expiredPosts.Count);
        }
    }
}
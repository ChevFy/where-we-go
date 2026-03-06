using Microsoft.EntityFrameworkCore;

using where_we_go.Database;
using where_we_go.DTO;
using where_we_go.Models;

namespace where_we_go.Service
{
    public class NotificationService : BaseService, INotificationService
    {
        private readonly AppDbContext _dbContext;

        public NotificationService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Guid> CreateNotificationAsync(NotificationCreateDto dto)
        {
            var notification = new Notification
            {
                NotificationId = Guid.NewGuid(),
                UserId = dto.UserId,
                PostId = dto.PostId,
                Content = dto.Content,
                Type = dto.Type,
                DateCreated = DateTime.UtcNow,
                IsRead = false
            };

            _dbContext.Notifications.Add(notification);
            await _dbContext.SaveChangesAsync();

            return notification.NotificationId;
        }

        public async Task<PaginatedResponseDto<NotificationDto>> GetNotificationsByUserIdAsync(string userId, NotificationQueryDto query)
        {
            var notifications = _dbContext.Notifications
                .Where(n => n.UserId == userId)
                .AsNoTracking();

            // Filter by IsRead status
            if (query.IsReadFilter.HasValue)
            {
                notifications = notifications.Where(n => n.IsRead == query.IsReadFilter.Value);
            }

            // Filter by PostId
            if (query.PostIdFilter.HasValue)
            {
                notifications = notifications.Where(n => n.PostId == query.PostIdFilter.Value);
            }

            // Sort by
            notifications = (query.SortBy ?? "").ToLower() switch
            {
                "latest" => notifications.OrderByDescending(n => n.DateCreated),
                "oldest" => notifications.OrderBy(n => n.DateCreated),
                _ => notifications.OrderByDescending(n => n.NotificationId)
            };

            // Map to NotificationDto and paginate
            var result = await ToPaginatedResponseAsync(notifications, query, n => new NotificationDto
            {
                NotificationId = n.NotificationId,
                UserId = n.UserId,
                PostId = n.PostId,
                Content = n.Content,
                DateCreated = n.DateCreated,
                IsRead = n.IsRead,
                Type = n.Type
            });

            return result;
        }

        public async Task<bool> UpdateNotificationReadStatusAsync(Guid notificationId, bool isRead)
        {
            var notification = await _dbContext.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId);

            if (notification == null)
            {
                return false;
            }

            notification.IsRead = isRead;
            _dbContext.Notifications.Update(notification);
            await _dbContext.SaveChangesAsync();

            return true;
        }
    }
}
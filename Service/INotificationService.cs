using where_we_go.DTO;

namespace where_we_go.Service
{
    public interface INotificationService
    {
        Task<Guid> CreateNotificationAsync(NotificationCreateDto dto);
        Task<PaginatedResponseDto<NotificationDto>> GetNotificationsByUserIdAsync(string userId, NotificationQueryDto query);
        Task<bool> UpdateNotificationReadStatusAsync(Guid notificationId, bool isRead);
    }
}

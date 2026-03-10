using where_we_go.DTO;

namespace where_we_go.Service
{
    public interface IAdminUserService
    {
        Task<PaginatedResponseDto<AdminUserDto>> GetUsersAsync(UserQueryDto query);
        Task<(bool Success, string? Error)> BanUserAsync(string userId, string reason, int durationDays, string bannedBy);
        Task<(bool Success, string? Error)> UnbanUserAsync(string userId);
    }
}

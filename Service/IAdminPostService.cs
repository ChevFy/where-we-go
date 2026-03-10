using where_we_go.DTO;

namespace where_we_go.Service
{
    public interface IAdminPostService
    {
        Task<PaginatedResponseDto<AdminPostDto>> GetPostsAsync(PostQueryDto query);
        Task<AdminPostDetailDto?> GetPostDetailAsync(Guid id);
        Task<(bool Success, string? Error)> DeletePostAsync(Guid id);
        Task<(bool Success, string? Error)> RestorePostAsync(Guid id);
        Task<(bool Success, string? Error)> UpdatePostAsync(Guid id, AdminPostUpdateDto dto);
        Task<(bool Success, string? Error)> RemoveParticipantAsync(Guid postId, Guid participantId);
    }
}

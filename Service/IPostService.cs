using where_we_go.DTO;
using where_we_go.Models;

namespace where_we_go.Service
{
    public interface IPostService
    {
        Task<PaginatedResponseDto<PostDto>> GetAllPostsAsync(PostQueryDto query, string? userId = null);
        Task<PostDetailDto?> GetPostDetailAsync(Guid id, string? currentUserId = null);
        Task CreatePostAsync(PostCreateDto dto, string userId);
        Task<bool> UpdatePostAsync(Guid postId, PostUpdateDto dto, string currentUserId);
        Task<bool> DeletePostAsync(Guid id, string userId);
        Task<string> JoinPostAsync(Guid postId, string userId);
        Task<string> LeavePostAsync(Guid postId, string userId);
        Task<string> ApproveJoinAsync(Guid postId, string[] participantUserIds, string currentUserId);
        Task<string> RejectJoinAsync(Guid postId, string[] participantUserIds, string currentUserId);
        Task<string> RemoveParticipantAsync(Guid postId, string participantUserId, string currentUserId);
        Task<List<ApplicantDto>> GetPostApplicantsAsync(Guid postId, string currentUserId, string? statusFilter = null);
        Task<PaginatedResponseDto<PostDto>> GetPostsByUserIdAsync(string userId, PostQueryDto query);
        Task<PaginatedResponseDto<PostDto>> GetPostsJoinedByUserIdAsync(string userId, PostQueryDto query);
    }
}
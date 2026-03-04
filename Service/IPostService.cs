using where_we_go.DTO;
using where_we_go.Models;

namespace where_we_go.Service
{
    public interface IPostService
    {
        Task<PaginatedResponseDto<PostDto>> GetAllPostsAsync(PostQueryDto query);
        Task<PostDetailDto?> GetPostDetailAsync(Guid id, string? currentUserId = null);
        Task CreatePostAsync(PostCreateDto dto, string userId);
        Task<bool> DeletePostAsync(Guid id, string userId);
        Task<string> JoinPostAsync(Guid postId, string userId);
        Task<string> LeavePostAsync(Guid postId, string userId);
    }
}
using where_we_go.DTO;
using where_we_go.Models;

namespace where_we_go.Service
{
    public interface IPostService
    {
        Task<List<PostDto>> GetAllPostsAsync();
        Task<PostDetailDto?> GetPostDetailAsync(Guid id);
        Task CreatePostAsync(PostCreateDto dto, string userId);
        Task<bool> DeletePostAsync(Guid id, string userId);
    }
}
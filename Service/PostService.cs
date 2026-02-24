using Microsoft.EntityFrameworkCore;
using where_we_go.Database;
using where_we_go.DTO;
using where_we_go.Models;

namespace where_we_go.Service
{
    public class PostService : IPostService
    {
        private readonly AppDbContext _dbContext;

        public PostService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<PostDto>> GetAllPostsAsync()
        {
            return await _dbContext.Posts
                .Select(p => new PostDto
                {
                    PostId = p.PostId,
                    Title = p.Title,
                    Description = p.Description,
                    LocationName = p.LocationName,
                    DateDeadline = p.DateDeadline,
                    CategoryName = "Mock Category"
                })
                .ToListAsync();
        }

        public async Task<PostDetailDto?> GetPostDetailAsync(Guid id)
        {
            return await _dbContext.Posts
                .Where(p => p.PostId == id)
                .Select(p => new PostDetailDto
                {
                    PostId = p.PostId,
                    Title = p.Title,
                    Description = p.Description,
                    LocationName = p.LocationName,
                    DateDeadline = p.DateDeadline,
                    CurrentParticipants = p.CurrentParticipants,
                    MaxParticipants = p.MaxParticipants,
                    CategoryName = "Mock Category"
                })
                .FirstOrDefaultAsync();
        }

        public async Task CreatePostAsync(PostCreateDto dto, string userId)
        {
            var post = new Post
            {
                PostId = Guid.NewGuid(),
                UserId = userId,
                Title = dto.Title,
                Description = dto.Description,
                LocationName = dto.LocationName,

                DateDeadline = dto.DateDeadline.ToUniversalTime(),

                MinParticipants = dto.MinParticipants,
                MaxParticipants = dto.MaxParticipants,

                DateCreated = DateTime.UtcNow,

                Status = "Active",
                CurrentParticipants = 0,
                InviteCode = Guid.NewGuid().ToString().Substring(0, 8).ToUpper()
            };

            _dbContext.Posts.Add(post);
            await _dbContext.SaveChangesAsync();
        }
    }
}
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
                    CurrentParticipants = _dbContext.Participants.Count(part => part.PostId == p.PostId && part.status == ParticipantStatus.Approved),
                    MaxParticipants = p.MaxParticipants,
                    CategoryName = "Mock Category",
                    UserId = p.UserId
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
                InviteCode = Guid.NewGuid().ToString().Substring(0, 8).ToUpper()
            };

            _dbContext.Posts.Add(post);
            await _dbContext.SaveChangesAsync();
        }
        public async Task<bool> DeletePostAsync(Guid id, string userId)
        {
            var post = await _dbContext.Posts.FirstOrDefaultAsync(p => p.PostId == id && p.UserId == userId);
            if (post == null)
            {
                return false;
            }

            _dbContext.Posts.Remove(post);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        public async Task<string> JoinPostAsync(Guid postId, string userId)
        {
            var post = await _dbContext.Posts.FindAsync(postId);
            if (post == null) return "Activity not found.";

            var existing = await _dbContext.Participants
                .AnyAsync(p => p.PostId == postId && p.UserId == userId);
            if (existing) return "You have already joined this activity.";

            var currentCount = await _dbContext.Participants.CountAsync(p => p.PostId == postId && p.status == ParticipantStatus.Approved);

            if (currentCount >= post.MaxParticipants) return "This activity is full.";

            var participant = new Participant
            {
                ParticipantId = Guid.NewGuid(),
                PostId = postId,
                UserId = userId,
                DateJoin = DateTime.UtcNow,
                status = ParticipantStatus.Approved
            };

            _dbContext.Participants.Add(participant);
            await _dbContext.SaveChangesAsync();

            return "Success";
        }
    }
}
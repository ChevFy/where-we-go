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

        public async Task<PostDetailDto?> GetPostDetailAsync(Guid id, string? currentUserId = null)
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
                    UserId = p.UserId,
                    // Check if the current user is an approved participant
                    IsJoined = currentUserId != null && _dbContext.Participants.Any(part => part.PostId == p.PostId && part.UserId == currentUserId && part.status == ParticipantStatus.Approved)
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

            var existingParticipant = await _dbContext.Participants
                .FirstOrDefaultAsync(p => p.PostId == postId && p.UserId == userId);

            var currentCount = await _dbContext.Participants
                .CountAsync(p => p.PostId == postId && p.status == ParticipantStatus.Approved);

            // Determine if they get in, or go to the waitlist
            string assignedStatus = currentCount >= post.MaxParticipants
                ? ParticipantStatus.Pending
                : ParticipantStatus.Approved;

            if (existingParticipant != null)
            {
                if (existingParticipant.status == ParticipantStatus.Approved)
                    return "You have already joined this activity.";

                if (existingParticipant.status == ParticipantStatus.Pending)
                    return "Pending"; // They are already on the waitlist

                if (existingParticipant.status == ParticipantStatus.Left)
                {
                    // Reactivate their old record
                    existingParticipant.status = assignedStatus;
                    existingParticipant.DateJoin = DateTime.UtcNow;
                    await _dbContext.SaveChangesAsync();
                    return assignedStatus == ParticipantStatus.Pending ? "Pending" : "Success";
                }
            }

            // Completely new participant
            var participant = new Participant
            {
                ParticipantId = Guid.NewGuid(),
                PostId = postId,
                UserId = userId,
                DateJoin = DateTime.UtcNow,
                status = assignedStatus
            };

            _dbContext.Participants.Add(participant);
            await _dbContext.SaveChangesAsync();

            return assignedStatus == ParticipantStatus.Pending ? "Pending" : "Success";
        }
        public async Task<string> LeavePostAsync(Guid postId, string userId)
        {
            var participant = await _dbContext.Participants
                .FirstOrDefaultAsync(p => p.PostId == postId &&
                                          p.UserId == userId &&
                                          p.status == ParticipantStatus.Approved);

            if (participant == null) return "You are not a member of this activity.";

            participant.status = ParticipantStatus.Left;

            await _dbContext.SaveChangesAsync();
            return "Success";
        }
    }
}
using Microsoft.EntityFrameworkCore;

using where_we_go.Database;
using where_we_go.DTO;
using where_we_go.Models.Enums;
using where_we_go.Models;

namespace where_we_go.Service
{
    public class PostService : IPostService
    {
        private readonly AppDbContext _dbContext;

        private readonly IFileService _fileService;

        public PostService(AppDbContext dbContext, IFileService fileService)
        {
            _dbContext = dbContext;
            _fileService = fileService;
        }



        public async Task<List<PostDto>> GetAllPostsAsync()
        {
            var posts = await _dbContext.Posts
                .Include(p => p.Categories)
                .ToListAsync();
            var result = new List<PostDto>();
            foreach (var p in posts)
            {
                result.Add(new PostDto
                {
                    PostId = p.PostId,
                    Title = p.Title,
                    Description = p.Description,
                    LocationName = p.LocationName,
                    DateDeadline = p.DateDeadline,
                    PostImgURL = await _fileService.GeneratePresignedPostUrlAsync(p.PostImageKey),
                    Categories = p.Categories.Select(c => new CategorySimpleDto
                    {
                        CategoryId = c.CategoryId,
                        Name = c.Name
                    }).ToList()
                });
            }
            return result;
        }

        public async Task<PostDetailDto?> GetPostDetailAsync(Guid id, string? currentUserId = null)
        {
            var post = await _dbContext.Posts
                .Include(p => p.Categories)
                .Where(p => p.PostId == id)
                .FirstOrDefaultAsync();

            if (post == null)
                return null;

            return new PostDetailDto
            {
                PostId = post.PostId,
                Title = post.Title,
                Description = post.Description,
                LocationName = post.LocationName,
                DateDeadline = post.DateDeadline,
                CurrentParticipants = _dbContext.Participants.Count(part => part.PostId == post.PostId && part.Status == ParticipantStatus.Approved),
                MaxParticipants = post.MaxParticipants,
                Categories = post.Categories.Select(c => new CategoryDetailDto
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Description = c.Description
                }).ToList(),
                PostImgURL = await _fileService.GeneratePresignedPostUrlAsync(post.PostImageKey),
                UserId = post.UserId,
                IsJoined = currentUserId != null && _dbContext.Participants.Any(part => part.PostId == post.PostId && part.UserId == currentUserId && part.Status == ParticipantStatus.Approved)
            };
        }

        public async Task CreatePostAsync(PostCreateDto dto, string userId, Guid postId)
        {
            var post = new Post
            {
                PostId = postId,
                UserId = userId,
                Title = dto.Title,
                Description = dto.Description,
                LocationName = dto.LocationName,
                PostImageKey = string.IsNullOrWhiteSpace(dto.PostImgkey) ? null : dto.PostImgkey,

                DateDeadline = dto.DateDeadline.ToUniversalTime(),

                MinParticipants = dto.MinParticipants,
                MaxParticipants = dto.MaxParticipants,

                DateCreated = DateTime.UtcNow,

                Status = "Active",
                InviteCode = Guid.NewGuid().ToString().Substring(0, 8).ToUpper()
            };

            _dbContext.Posts.Add(post);
            await _dbContext.SaveChangesAsync();

            // Associate categories if provided
            if (dto.CategoryIds?.Count > 0)
            {
                var categories = await _dbContext.Categories
                    .Where(c => dto.CategoryIds.Contains(c.CategoryId))
                    .ToListAsync();

                foreach (var category in categories)
                {
                    post.Categories.Add(category);
                }

                await _dbContext.SaveChangesAsync();
            }
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
                .CountAsync(p => p.PostId == postId && p.Status == ParticipantStatus.Approved);

            // Determine if they get in, or go to the waitlist
            var assignedStatus = currentCount >= post.MaxParticipants
                ? ParticipantStatus.Pending
                : ParticipantStatus.Approved;

            if (existingParticipant != null)
            {
                if (existingParticipant.Status == ParticipantStatus.Approved)
                    return "You have already joined this activity.";

                if (existingParticipant.Status == ParticipantStatus.Pending)
                    return "Pending"; // They are already on the waitlist

                if (existingParticipant.Status == ParticipantStatus.Left)
                {
                    // Reactivate their old record
                    existingParticipant.Status = assignedStatus;
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
                Status = assignedStatus
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
                                          p.Status == ParticipantStatus.Approved);

            if (participant == null) return "You are not a member of this activity.";

            participant.Status = ParticipantStatus.Left;

            await _dbContext.SaveChangesAsync();
            return "Success";
        }


    }
}
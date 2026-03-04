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



        private PostStatus GetPostStatus(Post post)
        {
            // Check if post is marked as deleted
            if (post.Status == PostStatus.Delete)
                return PostStatus.Delete;

            // Check if deadline has passed
            if (DateTime.UtcNow > post.DateDeadline)
                return PostStatus.Ended;

            // Check if post is full
            var participantCount = _dbContext.Participants.Count(part => part.PostId == post.PostId && part.Status == ParticipantStatus.Approved);
            if (participantCount >= post.MaxParticipants)
                return PostStatus.Full;

            // Otherwise active
            return PostStatus.Active;
        }

        public async Task<List<PostDto>> GetAllPostsAsync()
        {
            var posts = await _dbContext.Posts
                .Where(p => p.Status != PostStatus.Delete)
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
                    Status = GetPostStatus(p).ToString(),
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
                Status = GetPostStatus(post).ToString(),
                Locationlat = post.LocationLat ?? 0f,
                Locationlon = post.LocationLon ?? 0f,
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

        public async Task CreatePostAsync(PostCreateDto dto, string userId)
        {
            // Combine date and time into a single DateTime
            var combinedDateTime = dto.DateDeadline.Add(dto.TimeDeadline.ToTimeSpan());

            var post = new Post
            {
                PostId = Guid.NewGuid(),
                UserId = userId,
                Title = dto.Title,
                Description = dto.Description,
                LocationName = dto.LocationName,
                LocationLat = !string.IsNullOrEmpty(dto.LocationLat) ? float.Parse(dto.LocationLat) : null,
                LocationLon = !string.IsNullOrEmpty(dto.LocationLon) ? float.Parse(dto.LocationLon) : null,
                PostImageKey = string.IsNullOrWhiteSpace(dto.PostImgkey) ? null : dto.PostImgkey,

                DateDeadline = combinedDateTime.ToUniversalTime(),

                MinParticipants = dto.MinParticipants,
                MaxParticipants = dto.MaxParticipants,

                DateCreated = DateTime.UtcNow,

                Status = PostStatus.Active,
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

            post.Status = PostStatus.Delete;
            _dbContext.Posts.Update(post);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        public async Task<string> JoinPostAsync(Guid postId, string userId)
        {
            var post = await _dbContext.Posts.FindAsync(postId);
            if (post == null) return "Activity not found.";

            // Check if post is deleted, ended, or full
            if (post.Status == PostStatus.Delete)
                return "This activity has been deleted.";

            if (DateTime.UtcNow > post.DateDeadline)
                return "This activity has ended.";

            var approvedCount = await _dbContext.Participants
                .CountAsync(p => p.PostId == postId && p.Status == ParticipantStatus.Approved);

            if (approvedCount >= post.MaxParticipants)
                return "This activity is full. You will be added to the waitlist.";

            // Determine if they get in, or go to the waitlist
            var assignedStatus = approvedCount >= post.MaxParticipants
                ? ParticipantStatus.Pending
                : ParticipantStatus.Approved;

            var existingParticipant = await _dbContext.Participants
                .FirstOrDefaultAsync(p => p.PostId == postId && p.UserId == userId);

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
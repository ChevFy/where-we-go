using Microsoft.EntityFrameworkCore;

using where_we_go.Database;
using where_we_go.DTO;
using where_we_go.Models;
using where_we_go.Models.Enums;

namespace where_we_go.Service
{
    public class AdminPostService : BaseService, IAdminPostService
    {
        private readonly AppDbContext _dbContext;

        public AdminPostService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private PostStatus GetComputedPostStatus(Post post)
        {
            var now = DateTime.UtcNow;

            // Check for explicit Cancelled or Closed
            if (post.Status == PostStatus.Cancelled)
                return PostStatus.Cancelled;
            if (post.Status == PostStatus.Closed)
                return PostStatus.Closed;

            // Check EventDate - if passed, mark as Completed
            if (now > post.EventDate)
                return PostStatus.Completed;

            // Check DateDeadline logic
            if (now > post.DateDeadline)
            {
                // If Open: closed immediately, then cancelled after 1 hour
                if (post.Status == PostStatus.Open)
                {
                    var timeSinceDeadline = now - post.DateDeadline;
                    if (timeSinceDeadline.TotalHours >= 1)
                        return PostStatus.Cancelled;
                    else
                        return PostStatus.Closed;
                }

                // If already Closed, stay Closed
                if (post.Status == PostStatus.Closed)
                    return PostStatus.Closed;

                // Full stays Full
                if (post.Status == PostStatus.Full)
                    return PostStatus.Full;
            }

            // Check capacity-based status (before deadline)
            if (now <= post.DateDeadline)
            {
                var participantCount = _dbContext.Participants
                    .Count(part => part.PostId == post.PostId && part.Status == ParticipantStatus.Approved);
                if (participantCount >= post.MaxParticipants)
                    return PostStatus.Full;
            }

            // Default state
            return PostStatus.Open;
        }

        public async Task<PaginatedResponseDto<AdminPostDto>> GetPostsAsync(PostQueryDto query)
        {
            var postsQuery = _dbContext.Posts
                .Include(p => p.User)
                .AsNoTracking();

            // Apply name filter
            if (!string.IsNullOrWhiteSpace(query.NameFilter))
            {
                var keyword = query.NameFilter.Trim().ToLower();
                postsQuery = postsQuery.Where(p =>
                    EF.Functions.ILike(p.Title, $"%{keyword}%"));
            }

            // Parse status filter (for computed status filtering)
            PostStatus? targetStatus = null;
            if (!string.IsNullOrWhiteSpace(query.StatusFilter))
            {
                targetStatus = query.StatusFilter.ToLower() switch
                {
                    "open" => PostStatus.Open,
                    "full" => PostStatus.Full,
                    "closed" => PostStatus.Closed,
                    "completed" => PostStatus.Completed,
                    "cancelled" => PostStatus.Cancelled,
                    _ => PostStatus.Open
                };
            }

            // Load posts with required data for computed status
            var allPosts = await postsQuery
                .Include(p => p.Participants)
                .Select(p => new
                {
                    Post = p,
                    ApprovedParticipantCount = p.Participants.Count(part => part.Status == ParticipantStatus.Approved)
                })
                .ToListAsync();

            if (targetStatus.HasValue)
            {
                allPosts = allPosts.Where(x => GetComputedPostStatus(x.Post) == targetStatus.Value).ToList();
            }

            var totalCount = allPosts.Count;

            // Sorting
            allPosts = (query.SortBy ?? "").ToLower() switch
            {
                "title" => allPosts.OrderBy(x => x.Post.Title).ToList(),
                "title_desc" => allPosts.OrderByDescending(x => x.Post.Title).ToList(),
                "latest" => allPosts.OrderByDescending(x => x.Post.DateCreated).ToList(),
                "oldest" => allPosts.OrderBy(x => x.Post.DateCreated).ToList(),
                _ => allPosts.OrderByDescending(x => x.Post.DateCreated).ToList()
            };

            var pagedPosts = allPosts
                .Skip((query.PageSave - 1) * query.PageSizeSave)
                .Take(query.PageSizeSave)
                .Select(x => new AdminPostDto
                {
                    PostId = x.Post.PostId,
                    Title = x.Post.Title,
                    Description = x.Post.Description,
                    OwnerEmail = x.Post.User.Email ?? string.Empty,
                    OwnerName = x.Post.User.Name ?? string.Empty,
                    Status = GetComputedPostStatus(x.Post).ToString(),
                    CurrentParticipants = x.ApprovedParticipantCount,
                    MaxParticipants = x.Post.MaxParticipants,
                    DateDeadline = x.Post.DateDeadline,
                    EventDate = x.Post.EventDate,
                    DateCreated = x.Post.DateCreated,
                    LocationName = x.Post.LocationName
                })
                .ToList();

            return new PaginatedResponseDto<AdminPostDto>(pagedPosts, query.PageSizeSave, query.PageSave, totalCount);
        }

        public async Task<AdminPostDetailDto?> GetPostDetailAsync(Guid id)
        {
            var post = await _dbContext.Posts
                .Include(p => p.User)
                .Include(p => p.Categories)
                .Include(p => p.Participants)
                    .ThenInclude(part => part.User)
                .FirstOrDefaultAsync(p => p.PostId == id);

            if (post == null) return null;

            return new AdminPostDetailDto
            {
                PostId = post.PostId,
                Title = post.Title,
                Description = post.Description,
                OwnerId = post.UserId,
                OwnerEmail = post.User.Email ?? string.Empty,
                OwnerName = post.User.Name ?? string.Empty,
                Status = GetComputedPostStatus(post).ToString(),
                CurrentParticipants = post.Participants.Count(p => p.Status == ParticipantStatus.Approved),
                MaxParticipants = post.MaxParticipants,
                MinParticipants = post.MinParticipants,
                DateDeadline = post.DateDeadline,
                EventDate = post.EventDate,
                DateCreated = post.DateCreated,
                LocationName = post.LocationName,
                LocationLat = post.LocationLat,
                LocationLon = post.LocationLon,
                PostImageKey = post.PostImageKey,
                InviteCode = post.InviteCode,
                Categories = post.Categories.Select(c => new CategorySimpleDto
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name
                }).ToList(),
                Participants = post.Participants.Select(p => new ParticipantInfoDto
                {
                    ParticipantId = p.ParticipantId,
                    UserId = p.UserId,
                    UserName = p.User.Name ?? string.Empty,
                    UserEmail = p.User.Email ?? string.Empty,
                    Status = p.Status.ToString(),
                    DateJoin = p.DateJoin
                }).ToList()
            };
        }

        public async Task<(bool Success, string? Error)> CancelPostAsync(Guid id)
        {
            var post = await _dbContext.Posts.FindAsync(id);
            if (post == null)
            {
                return (false, "Post not found");
            }

            // Prevent cancelling Completed posts
            var computedStatus = GetComputedPostStatus(post);
            if (computedStatus == PostStatus.Completed)
            {
                return (false, "Cannot cancel a completed post");
            }

            post.Status = PostStatus.Cancelled;
            await _dbContext.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? Error)> RestorePostAsync(Guid id)
        {
            var post = await _dbContext.Posts.FindAsync(id);
            if (post == null)
            {
                return (false, "Post not found");
            }

            // Prevent restoring Completed posts
            var computedStatus = GetComputedPostStatus(post);
            if (computedStatus == PostStatus.Completed)
            {
                return (false, "Cannot restore a completed post");
            }

            // Prevent restoring if deadline has passed
            if (DateTime.UtcNow > post.DateDeadline)
            {
                return (false, "Cannot restore: deadline has passed");
            }

            post.Status = PostStatus.Open;
            await _dbContext.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? Error)> UpdatePostAsync(Guid id, AdminPostUpdateDto dto)
        {
            var post = await _dbContext.Posts
                .Include(p => p.Categories)
                .FirstOrDefaultAsync(p => p.PostId == id);

            if (post == null)
            {
                return (false, "Post not found");
            }

            // Prevent updating Completed posts
            var computedStatus = GetComputedPostStatus(post);
            if (computedStatus == PostStatus.Completed)
            {
                return (false, "Cannot update a completed post");
            }

            // Update editable fields: Participants, Categories
            post.MinParticipants = dto.MinParticipants;
            post.MaxParticipants = dto.MaxParticipants;

            // Update categories if provided
            if (dto.CategoryIds != null && dto.CategoryIds.Count > 0)
            {
                var categories = await _dbContext.Categories
                    .Where(c => dto.CategoryIds.Contains(c.CategoryId))
                    .ToListAsync();

                post.Categories.Clear();
                foreach (var category in categories)
                {
                    post.Categories.Add(category);
                }
            }

            await _dbContext.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? Error)> RemoveParticipantAsync(Guid postId, Guid participantId)
        {
            var post = await _dbContext.Posts.FindAsync(postId);
            if (post == null)
            {
                return (false, "Post not found");
            }

            // Prevent removing participants from Completed posts
            var computedStatus = GetComputedPostStatus(post);
            if (computedStatus == PostStatus.Completed)
            {
                return (false, "Cannot remove participants from a completed post");
            }

            var participant = await _dbContext.Participants
                .FirstOrDefaultAsync(p => p.ParticipantId == participantId && p.PostId == postId);

            if (participant == null)
            {
                return (false, "Participant not found");
            }

            // Set status to Rejected (user was removed by admin, not voluntary withdrawal)
            participant.Status = ParticipantStatus.Rejected;
            await _dbContext.SaveChangesAsync();
            return (true, null);
        }
    }
}

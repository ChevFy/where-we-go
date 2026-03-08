using Microsoft.EntityFrameworkCore;

using where_we_go.Database;
using where_we_go.DTO;
using where_we_go.Models.Enums;
using where_we_go.Models;

namespace where_we_go.Service
{
    public class PostService : BaseService, IPostService
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
            // 1. Check for manual/explicit states first
            if (post.Status == PostStatus.Cancelled)
                return PostStatus.Cancelled;

            if (post.Status == PostStatus.Closed)
                return PostStatus.Closed; // Owner closed it early

            var now = DateTime.UtcNow;

            // 2. Check time-based states
            if (now > post.EventDate)
                return PostStatus.Completed;

            if (now > post.DateDeadline)
                return PostStatus.Closed;

            // 3. Check capacity-based states
            var participantCount = _dbContext.Participants.Count(part => part.PostId == post.PostId && part.Status == ParticipantStatus.Approved);
            if (participantCount >= post.MaxParticipants)
                return PostStatus.Full;

            // 4. Default state
            return PostStatus.Open;
        }

        private async Task<PaginatedResponseDto<PostDto>> ApplyFiltersAndGetPaginatedPostsAsync(IQueryable<Post> posts, PostQueryDto query)
        {
            // Filter by name
            if (!string.IsNullOrWhiteSpace(query.NameFilter))
            {
                var keyword = query.NameFilter.Trim();
                posts = posts.Where(p => EF.Functions.Like(p.Title.ToLower(), $"%{keyword}%"));
            }

            // Filter by categories
            if (query.Categories != null && query.Categories.Count > 0)
            {
                posts = posts.Where(p => query.Categories.Any(catId => p.Categories.Any(c => c.CategoryId == catId)));
            }

            // Filter by status
            var now = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(query.StatusFilter))
            {
                posts = query.StatusFilter.ToLower() switch
                {
                    "cancelled" => posts.Where(p => p.Status == PostStatus.Cancelled),
                    "completed" => posts.Where(p => p.Status != PostStatus.Cancelled && now > p.EventDate),
                    "closed" => posts.Where(p => p.Status != PostStatus.Cancelled && now > p.DateDeadline && now <= p.EventDate),
                    "full" => posts.Where(p => p.Status != PostStatus.Cancelled &&
                                              now <= p.DateDeadline &&
                                              _dbContext.Participants.Count(part => part.PostId == p.PostId && part.Status == ParticipantStatus.Approved) >= p.MaxParticipants),
                    "open" => posts.Where(p => p.Status != PostStatus.Cancelled &&
                                                now <= p.DateDeadline &&
                                                _dbContext.Participants.Count(part => part.PostId == p.PostId && part.Status == ParticipantStatus.Approved) < p.MaxParticipants),
                    _ => posts.Where(p => p.Status != PostStatus.Cancelled)
                };
            }
            else
            {
                // Exclude cancelled posts by default
                posts = posts.Where(p => p.Status != PostStatus.Cancelled);
            }

            // Sort by
            posts = (query.SortBy ?? "").ToLower() switch
            {
                "title" => posts.OrderBy(p => p.Title),
                "title_desc" => posts.OrderByDescending(p => p.Title),
                "latest" => posts.OrderByDescending(p => p.DateCreated),
                "oldest" => posts.OrderBy(p => p.DateCreated),
                _ => posts.OrderBy(p => p.PostId)
            };

            // Map to PostDto and paginate
            var result = await ToPaginatedResponseAsync(posts, query, p => new PostDto
            {
                PostId = p.PostId,
                Title = p.Title,
                Description = p.Description,
                LocationName = p.LocationName,
                DateDeadline = p.DateDeadline,
                EventDate = p.EventDate,
                PostImgURL = p.PostImageKey,
                Status = GetPostStatus(p).ToString(),
                MaxParticipants = p.MaxParticipants,
                CurrentParticipants = _dbContext.Participants.Count(part => part.PostId == p.PostId && part.Status == ParticipantStatus.Approved),
                Categories = [.. p.Categories.Select(c => new CategorySimpleDto
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name
                })]
            });

            // Generate presigned URLs for post images
            foreach (var p in result.Data)
            {
                p.PostImgURL = await _fileService.GeneratePresignedPostUrlAsync(p.PostImgURL);
            }

            return result;
        }

        public async Task<PaginatedResponseDto<PostDto>> GetAllPostsAsync(PostQueryDto query)
        {
            var posts = _dbContext.Posts
                .Include(p => p.Categories)
                .AsNoTracking();

            return await ApplyFiltersAndGetPaginatedPostsAsync(posts, query);
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
                EventDate = post.EventDate,
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
                IsJoined = currentUserId != null && _dbContext.Participants.Any(part => part.PostId == post.PostId && part.UserId == currentUserId && part.Status == ParticipantStatus.Approved),
                IsPending = currentUserId != null && _dbContext.Participants.Any(part => part.PostId == post.PostId && part.UserId == currentUserId && part.Status == ParticipantStatus.Pending)
            };
        }

        public async Task CreatePostAsync(PostCreateDto dto, string userId)
        {
            // Combine date and time into a single DateTime
            var combinedDateTime = dto.DateDeadline.Add(dto.TimeDeadline.ToTimeSpan());
            var combinedEventDateTime = dto.EventDate.Add(dto.EventTime.ToTimeSpan());

            var dateDeadline = combinedDateTime.ToUniversalTime();
            var eventDate = combinedEventDateTime.ToUniversalTime();

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

                DateDeadline = dateDeadline,
                EventDate = eventDate,

                MinParticipants = dto.MinParticipants,
                MaxParticipants = dto.MaxParticipants,

                DateCreated = DateTime.UtcNow,

                Status = PostStatus.Open, // Changed from Active to Open
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

            post.Status = PostStatus.Cancelled; // Changed from Delete to Cancelled
            _dbContext.Posts.Update(post);

            // TODO: Notify every participant (except status == reject, withdrawn)
            /* var participantsToNotify = await _dbContext.Participants
                .Where(p => p.PostId == id && 
                            p.Status != ParticipantStatus.Rejected && 
                            p.Status != ParticipantStatus.Withdrawn)
                .ToListAsync();

            foreach(var participant in participantsToNotify) {
                // notificationService.SendNotification(participant.UserId, "The activity has been cancelled.");
            }
            */

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<string> JoinPostAsync(Guid postId, string userId)
        {
            var post = await _dbContext.Posts.FindAsync(postId);
            if (post == null) return "Activity not found.";

            var currentStatus = GetPostStatus(post);
            if (currentStatus == PostStatus.Cancelled) return "This activity has been cancelled.";
            if (currentStatus == PostStatus.Closed) return "This activity is closed for new participants.";
            if (currentStatus == PostStatus.Completed) return "This activity is already completed.";
            if (currentStatus == PostStatus.Full) return "This activity is currently full.";

            var existingParticipant = await _dbContext.Participants
                .FirstOrDefaultAsync(p => p.PostId == postId && p.UserId == userId);

            if (existingParticipant != null)
            {
                if (existingParticipant.Status == ParticipantStatus.Approved) return "You have already joined this activity.";
                if (existingParticipant.Status == ParticipantStatus.Pending) return "Your request is already pending.";
                if (existingParticipant.Status == ParticipantStatus.Rejected) return "Your previous request was rejected.";

                if (existingParticipant.Status == ParticipantStatus.Withdrawn) // Changed from Left to Withdrawn
                {
                    existingParticipant.Status = ParticipantStatus.Pending;
                    existingParticipant.DateJoin = DateTime.UtcNow;
                    await _dbContext.SaveChangesAsync();

                    // TODO: Notify owner here
                    // notificationService.NotifyOwner(post.UserId, "Someone requested to join!");

                    return "Pending";
                }
            }

            // New participant ALWAYS goes to pending
            var participant = new Participant
            {
                ParticipantId = Guid.NewGuid(),
                PostId = postId,
                UserId = userId,
                DateJoin = DateTime.UtcNow,
                Status = ParticipantStatus.Pending
            };

            _dbContext.Participants.Add(participant);
            await _dbContext.SaveChangesAsync();

            // TODO: Notify owner here
            // notificationService.NotifyOwner(post.UserId, "Someone requested to join!");

            return "Pending";
        }

        public async Task<string> LeavePostAsync(Guid postId, string userId)
        {
            var participant = await _dbContext.Participants
                .FirstOrDefaultAsync(p => p.PostId == postId &&
                                          p.UserId == userId &&
                                          p.Status == ParticipantStatus.Approved);

            if (participant == null) return "You are not a member of this activity.";

            participant.Status = ParticipantStatus.Withdrawn; // Changed from Left to Withdrawn

            await _dbContext.SaveChangesAsync();
            return "Success";
        }

        public async Task<string> ApproveJoinAsync(Guid postId, string participantUserId, string currentUserId)
        {
            var post = await _dbContext.Posts.FindAsync(postId);
            if (post == null || post.UserId != currentUserId) return "Unauthorized or Post Not Found.";

            var participant = await _dbContext.Participants
                .FirstOrDefaultAsync(p => p.PostId == postId && p.UserId == participantUserId && p.Status == ParticipantStatus.Pending);

            if (participant == null) return "Participant request not found.";

            // Double check if post is full before approving
            var approvedCount = await _dbContext.Participants.CountAsync(p => p.PostId == postId && p.Status == ParticipantStatus.Approved);
            if (approvedCount >= post.MaxParticipants) return "Cannot approve: Activity is already full.";

            participant.Status = ParticipantStatus.Approved;
            await _dbContext.SaveChangesAsync();

            // TODO: Notify participant
            // notificationService.NotifyUser(participantUserId, "Your request to join was approved!");

            return "Success";
        }

        public async Task<string> RejectJoinAsync(Guid postId, string participantUserId, string currentUserId)
        {
            var post = await _dbContext.Posts.FindAsync(postId);
            if (post == null || post.UserId != currentUserId) return "Unauthorized or Post Not Found.";

            var participant = await _dbContext.Participants
                .FirstOrDefaultAsync(p => p.PostId == postId && p.UserId == participantUserId && p.Status == ParticipantStatus.Pending);

            if (participant == null) return "Participant request not found.";

            participant.Status = ParticipantStatus.Rejected;
            await _dbContext.SaveChangesAsync();

            // TODO: Notify participant
            // notificationService.NotifyUser(participantUserId, "Your request to join was declined.");

            return "Success";
        }

        public async Task<PaginatedResponseDto<PostDto>> GetPostsByUserIdAsync(string userId, PostQueryDto query)
        {
            var posts = _dbContext.Posts
                .Include(p => p.Categories)
                .Where(p => p.UserId == userId)
                .AsNoTracking();

            return await ApplyFiltersAndGetPaginatedPostsAsync(posts, query);
        }

        public async Task<PaginatedResponseDto<PostDto>> GetPostsJoinedByUserIdAsync(string userId, PostQueryDto query)
        {
            var posts = _dbContext.Posts
                .Include(p => p.Categories)
                .Where(p => _dbContext.Participants.Any(part => part.PostId == p.PostId &&
                                                               part.UserId == userId &&
                                                               part.Status == ParticipantStatus.Approved))
                .AsNoTracking();

            return await ApplyFiltersAndGetPaginatedPostsAsync(posts, query);
        }
    }
}
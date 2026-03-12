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
                    EF.Functions.Like(p.Title.ToLower(), $"%{keyword}%"));
            }

            // Filter by status
            if (!string.IsNullOrWhiteSpace(query.StatusFilter))
            {
                var status = query.StatusFilter.ToLower() switch
                {
                    "open" => PostStatus.Open,
                    "full" => PostStatus.Full,
                    "completed" => PostStatus.Completed,
                    "cancelled" => PostStatus.Cancelled,
                    _ => PostStatus.Open
                };
                postsQuery = postsQuery.Where(p => p.Status == status);
            }

            var totalCount = await postsQuery.CountAsync();

            // Sorting
            postsQuery = (query.SortBy ?? "").ToLower() switch
            {
                "title" => postsQuery.OrderBy(p => p.Title),
                "title_desc" => postsQuery.OrderByDescending(p => p.Title),
                "latest" => postsQuery.OrderByDescending(p => p.DateCreated),
                "oldest" => postsQuery.OrderBy(p => p.DateCreated),
                _ => postsQuery.OrderByDescending(p => p.DateCreated)
            };

            // Pagination
            var posts = await postsQuery
                .Skip((query.PageSave - 1) * query.PageSizeSave)
                .Take(query.PageSizeSave)
                .Select(p => new AdminPostDto
                {
                    PostId = p.PostId,
                    Title = p.Title,
                    Description = p.Description,
                    OwnerEmail = p.User.Email ?? string.Empty,
                    OwnerName = p.User.Name ?? string.Empty,
                    Status = p.Status.ToString(),
                    CurrentParticipants = _dbContext.Participants
                        .Count(part => part.PostId == p.PostId && part.Status == ParticipantStatus.Approved),
                    MaxParticipants = p.MaxParticipants,
                    DateDeadline = p.DateDeadline,
                    EventDate = p.EventDate,
                    DateCreated = p.DateCreated,
                    LocationName = p.LocationName
                })
                .ToListAsync();

            return new PaginatedResponseDto<AdminPostDto>(posts, query.PageSizeSave, query.PageSave, totalCount);
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
                Status = post.Status.ToString(),
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

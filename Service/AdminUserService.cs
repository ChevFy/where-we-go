using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

using where_we_go.Database;
using where_we_go.DTO;
using where_we_go.Models;
using where_we_go.Models.Enums;

namespace where_we_go.Service
{
    public class AdminUserService : BaseService, IAdminUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _dbContext;
        private readonly IMemoryCache _cache;

        public AdminUserService(UserManager<User> userManager, AppDbContext dbContext, IMemoryCache cache)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _cache = cache;
        }

        public async Task<PaginatedResponseDto<AdminUserDto>> GetUsersAsync(UserQueryDto query)
        {
            var usersQuery = _userManager.Users.AsNoTracking();

            // Apply name filter if provided
            if (!string.IsNullOrWhiteSpace(query.NameFilter))
            {
                var keyword = query.NameFilter.Trim();
                usersQuery = usersQuery.Where(u =>
                    (u.Email != null && u.Email.Contains(keyword)) ||
                    (u.Name != null && u.Name.Contains(keyword)));
            }

            // Get total count
            var totalCount = await usersQuery.CountAsync();

            // Apply sorting
            usersQuery = (query.SortBy ?? "").ToLower() switch
            {
                "name" => usersQuery.OrderBy(u => u.Name),
                "name_desc" => usersQuery.OrderByDescending(u => u.Name),
                "email" => usersQuery.OrderBy(u => u.Email),
                "email_desc" => usersQuery.OrderByDescending(u => u.Email),
                _ => usersQuery.OrderBy(u => u.Id)
            };

            // Apply pagination
            var users = await usersQuery
                .Skip((query.PageSave - 1) * query.PageSizeSave)
                .Take(query.PageSizeSave)
                .Select(u => new AdminUserDto
                {
                    Id = u.Id,
                    Email = u.Email ?? string.Empty,
                    Name = u.Name,
                    IsBanned = u.IsBanned,
                    BanReason = u.BanReason,
                    BanExpiresAt = u.BanExpiresAt,
                    IsAdmin = _dbContext.UserRoles
                        .Where(ur => ur.UserId == u.Id)
                        .Join(_dbContext.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                        .Contains("Admin")
                })
                .ToListAsync();

            return new PaginatedResponseDto<AdminUserDto>(users, query.PageSizeSave, query.PageSave, totalCount);
        }

        public async Task<(bool Success, string? Error)> BanUserAsync(string userId, string reason, int durationDays, string bannedBy)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (false, "User not found");
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles.Contains("Admin"))
            {
                return (false, "Cannot ban an admin user");
            }

            user.IsBanned = true;
            user.BanReason = reason;
            user.BanExpiresAt = DateTime.UtcNow.AddDays(durationDays);
            user.BannedBy = bannedBy;

            await _userManager.UpdateAsync(user);

            // Cancel all posts created by the banned user
            var userPosts = await _dbContext.Posts
                .Where(p => p.UserId == userId && p.Status != PostStatus.Cancelled)
                .ToListAsync();

            foreach (var post in userPosts)
            {
                post.Status = PostStatus.Cancelled;
            }

            if (userPosts.Any())
            {
                await _dbContext.SaveChangesAsync();
            }

            _cache.Remove($"user_ban_status_{userId}");

            return (true, null);
        }

        public async Task<(bool Success, string? Error)> UnbanUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (false, "User not found");
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles.Contains("Admin"))
            {
                return (false, "Cannot modify ban status for admin users");
            }

            user.IsBanned = false;
            user.BanReason = null;
            user.BanExpiresAt = null;

            await _userManager.UpdateAsync(user);

            _cache.Remove($"user_ban_status_{userId}");

            return (true, null);
        }
    }
}

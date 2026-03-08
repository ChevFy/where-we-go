using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

using where_we_go.Database;
using where_we_go.DTO;
using where_we_go.Models;
using where_we_go.Models.Enums;
using where_we_go.Service;


[Authorize(Roles = "Admin")]
[Route("admin")]
public class AdminController(UserManager<User> userManager, IMemoryCache cache, AppDbContext dbContext, ICategoryService categoryService) : Controller
{
    private IMemoryCache _cache { get; init; } = cache;
    private AppDbContext _dbContext { get; init; } = dbContext;
    private ICategoryService _categoryService { get; init; } = categoryService;

    [HttpGet("index")]
    public IActionResult Index() => View();

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] UserQueryDto query)
    {
        var usersQuery = userManager.Users.AsNoTracking();

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

        var meta = new PaginatedMetaDto(query.PageSizeSave, query.PageSave, totalCount);
        var response = new PaginatedResponseDto<AdminUserDto>(users, query.PageSizeSave, query.PageSave, totalCount);

        return Json(response);
    }

    [HttpPost("users/ban")]
    public async Task<IActionResult> BanUser([FromBody] BanUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { details = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        var user = await userManager.FindByIdAsync(dto.UserId);
        if (user == null) return NotFound(new { details = "User not found" });
        var userRoles = await userManager.GetRolesAsync(user);
        if (userRoles.Contains("Admin"))
        {
            return BadRequest(new { details = "Cannot ban an admin user" });
        }
        user.IsBanned = true;
        user.BanReason = dto.Reason;
        user.BanExpiresAt = DateTime.UtcNow.AddDays(dto.DurationDays);
        user.BannedBy = User.Identity?.Name ?? "System";

        await userManager.UpdateAsync(user);

        _cache.Remove($"user_ban_status_{dto.UserId}");

        return Ok();
    }

    [HttpPost("users/unban")]
    public async Task<IActionResult> UnbanUser([FromBody] UnbanUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { details = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        var user = await userManager.FindByIdAsync(dto.UserId);
        if (user == null) return NotFound(new { details = "User not found" });
        var userRoles = await userManager.GetRolesAsync(user);
        if (userRoles.Contains("Admin"))
        {
            return BadRequest(new { details = "Cannot modify ban status for admin users" });
        }
        user.IsBanned = false;
        user.BanReason = null;
        user.BanExpiresAt = null;

        await userManager.UpdateAsync(user);

        _cache.Remove($"user_ban_status_{dto.UserId}");

        return Ok();
    }

    [HttpGet("users/search")]
    public async Task<IActionResult> SearchUsers([FromQuery] UserQueryDto query)
    {
        if (string.IsNullOrWhiteSpace(query.NameFilter))
        {
            query.NameFilter = null;
        }

        return await GetUsers(query);
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _categoryService.GetAllAsync();
        return Json(categories);
    }

    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { details = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        try
        {
            var category = await _categoryService.CreateAsync(dto);
            return Ok(category);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { details = ex.Message });
        }
    }

    [HttpPut("categories/{id}")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] CreateCategoryDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { details = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        try
        {
            var category = await _categoryService.UpdateAsync(id, dto);
            if (category == null)
            {
                return NotFound(new { details = "Category not found" });
            }
            return Ok(category);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { details = ex.Message });
        }
    }

    [HttpDelete("categories/{id:guid}")]
    public async Task<IActionResult> DeleteCategory([FromRoute] Guid id)
    {
        try
        {
            var result = await _categoryService.DeleteAsync(id);
            if (!result)
            {
                return NotFound(new { details = "Category not found" });
            }
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { details = "Cannot delete category: " + ex.Message });
        }
    }

    [HttpGet("posts")]
    public async Task<IActionResult> GetPosts([FromQuery] PostQueryDto query)
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
                "active" => PostStatus.Active,
                "full" => PostStatus.Full,
                "ended" => PostStatus.Ended,
                "deleted" => PostStatus.Delete,
                _ => PostStatus.Active
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

        var response = new PaginatedResponseDto<AdminPostDto>(posts, query.PageSizeSave, query.PageSave, totalCount);
        return Json(response);
    }

    [HttpGet("posts/{id:guid}")]
    public async Task<IActionResult> GetPostDetail([FromRoute] Guid id)
    {
        var post = await _dbContext.Posts
            .Include(p => p.User)
            .Include(p => p.Categories)
            .Include(p => p.Participants)
                .ThenInclude(part => part.User)
            .FirstOrDefaultAsync(p => p.PostId == id);

        if (post == null) return NotFound(new { details = "Post not found" });

        var result = new AdminPostDetailDto
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

        return Json(result);
    }

    [HttpPost("posts/{id:guid}/delete")]
    public async Task<IActionResult> DeletePost([FromRoute] Guid id)
    {
        var post = await _dbContext.Posts.FindAsync(id);
        if (post == null) return NotFound(new { details = "Post not found" });

        post.Status = PostStatus.Delete;
        await _dbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpPut("posts/{id:guid}")]
    public async Task<IActionResult> UpdatePost([FromRoute] Guid id, [FromBody] AdminPostUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { details = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        var post = await _dbContext.Posts
            .Include(p => p.Categories)
            .FirstOrDefaultAsync(p => p.PostId == id);

        if (post == null) return NotFound(new { details = "Post not found" });

        // Update editable fields: Status, Participants, Categories
        post.MinParticipants = dto.MinParticipants;
        post.MaxParticipants = dto.MaxParticipants;

        // Parse and update status
        if (Enum.TryParse<PostStatus>(dto.Status, true, out var status))
        {
            post.Status = status;
        }

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

        return Ok(new { message = "Post updated successfully" });
    }

    [HttpPost("posts/{id:guid}/restore")]
    public async Task<IActionResult> RestorePost([FromRoute] Guid id)
    {
        var post = await _dbContext.Posts.FindAsync(id);
        if (post == null) return NotFound(new { details = "Post not found" });

        post.Status = PostStatus.Active;
        await _dbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("posts/{postId:guid}/participants/{participantId:guid}")]
    public async Task<IActionResult> RemoveParticipant([FromRoute] Guid postId, [FromRoute] Guid participantId)
    {
        var participant = await _dbContext.Participants
            .FirstOrDefaultAsync(p => p.ParticipantId == participantId && p.PostId == postId);

        if (participant == null)
            return NotFound(new { details = "Participant not found" });

        _dbContext.Participants.Remove(participant);
        await _dbContext.SaveChangesAsync();

        return Ok(new { message = "Participant removed successfully" });
    }

}

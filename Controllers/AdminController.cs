using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

using where_we_go.Database;
using where_we_go.DTO;
using where_we_go.Models;


[Authorize(Roles = "Admin")]
[Route("admin")]
public class AdminController(UserManager<User> userManager, IMemoryCache cache, AppDbContext dbContext) : Controller
{
    private IMemoryCache _cache { get; init; } = cache;
    private AppDbContext _dbContext { get; init; } = dbContext;

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
                BanExpiresAt = u.BanExpiresAt
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
        var categories = await _dbContext.Categories
            .AsNoTracking()
            .Select(c => new CategoryDto
            {
                CategoryId = c.CategoryId,
                Name = c.Name,
                Description = c.Description,
                PostCount = c.Posts.Count
            })
            .ToListAsync();

        return Json(categories);
    }

    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { details = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        // Check if category name already exists
        var existingCategory = await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.Name.ToLower() == dto.Name.ToLower());
        
        if (existingCategory != null)
        {
            return BadRequest(new { details = "A category with this name already exists" });
        }

        var category = new Category
        {
            CategoryId = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description
        };

        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync();

        return Ok(new CategoryDto
        {
            CategoryId = category.CategoryId,
            Name = category.Name,
            Description = category.Description,
            PostCount = 0
        });
    }

}

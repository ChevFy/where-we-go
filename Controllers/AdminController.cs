using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using where_we_go.DTO;
using where_we_go.Models;


[Authorize(Roles = "Admin")]
[Route("admin")]
public class AdminController(UserManager<User> userManager) : Controller
{
    [HttpGet("index")]
    public IActionResult Index() => View();
    
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await userManager.Users
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
        
        return Json(users);
    }
    
    [HttpPost("users/ban")]
    public async Task<IActionResult> BanUser([FromBody] BanUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var user = await userManager.FindByIdAsync(dto.UserId);
        if (user == null) return NotFound();
        
        user.IsBanned = true;
        user.BanReason = dto.Reason;
        user.BanExpiresAt = DateTime.UtcNow.AddDays(dto.DurationDays);
        user.BannedBy = User.Identity?.Name ?? "System";
        
        await userManager.UpdateAsync(user);
        return Ok();
    }
    
    [HttpPost("users/unban")]
    public async Task<IActionResult> UnbanUser([FromBody] UnbanUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var user = await userManager.FindByIdAsync(dto.UserId);
        if (user == null) return NotFound();
        
        user.IsBanned = false;
        user.BanReason = null;
        user.BanExpiresAt = null;
        
        await userManager.UpdateAsync(user);
        return Ok();
    }
    
    [HttpGet("users/search")]
    public async Task<IActionResult> SearchUsers(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return await GetUsers();
        }
        
        var searchTerm = query.ToLower();
        var users = await userManager.Users
            .Where(u => (u.Email != null && u.Email.ToLower().Contains(searchTerm)) ||
                        (u.Name != null && u.Name.ToLower().Contains(searchTerm)))
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
        
        return Json(users);
    }
}

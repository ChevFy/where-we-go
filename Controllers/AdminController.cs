using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using where_we_go.Models;


[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<User> _userManager;
    
    public AdminController(UserManager<User> userManager)
    {
        _userManager = userManager;
    }
    
    public IActionResult Index() => View();
    
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userManager.Users
            .Select(u => new {
                u.Id,
                u.Email,
                u.Name,
                u.IsBanned,
                u.BanReason,
                u.BanExpiresAt
            })
            .ToListAsync();
        
        return Json(users);
    }
    
    [HttpPost]
    public async Task<IActionResult> BanUser(string userId, string reason, int durationDays)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();
        
        user.IsBanned = true;
        user.BanReason = reason;
        user.BanExpiresAt = DateTime.UtcNow.AddDays(durationDays);
        user.BannedBy = User.Identity.Name;
        
        await _userManager.UpdateAsync(user);
        return Ok();
    }
    
    [HttpPost]
    public async Task<IActionResult> UnbanUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();
        
        user.IsBanned = false;
        user.BanReason = null;
        user.BanExpiresAt = null;
        
        await _userManager.UpdateAsync(user);
        return Ok();
    }
    
    [HttpGet]
    public async Task<IActionResult> SearchUsers(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return await GetUsers();
        }
        
        var searchTerm = query.ToLower();
        var users = await _userManager.Users
            .Where(u => u.Email.ToLower().Contains(searchTerm) ||
                        (u.Name != null && u.Name.ToLower().Contains(searchTerm)))
            .Select(u => new {
                u.Id,
                u.Email,
                u.Name,
                u.IsBanned,
                u.BanReason,
                u.BanExpiresAt
            })
            .ToListAsync();
        
        return Json(users);
    }
}

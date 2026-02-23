using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using where_we_go.Models;
using where_we_go.Database;

namespace where_we_go.Controllers;

public class HomeController(UserManager<User> userManager, AppDbContext dbContext) : Controller
{
    private UserManager<User> _userManager { get; init; } = userManager;
    private AppDbContext _dbContext { get; init; } = dbContext;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        bool IsAuth = User.Identity?.IsAuthenticated ?? false;
        ViewBag.IsAuth = IsAuth;

        var posts = await _dbContext.Posts
        .Select(p => new PostDto
        {
            PostId = p.PostId,
            Title = p.Title,
            Description = p.Description,
            LocationName = p.LocationName,
            DateDeadlineFormatted = p.DateDeadline.ToString("dd/MM/yyyy"),
            CategoryName = "Mock Category"
        })
        .ToListAsync();

        return View(posts);

    }

    [Authorize]
    public async Task<IActionResult> Privacy()
    {
        bool IsAuth = User.Identity?.IsAuthenticated ?? false;
        return View();
    }

    [Authorize(Roles = "Admin")]
    public string Admin()
    {
        return "THIS IS ADMIN PAGE";
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

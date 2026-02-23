using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using where_we_go.Models;
using where_we_go.Database;
using where_we_go.Service;

namespace where_we_go.Controllers;

public class HomeController(UserManager<User> userManager, AppDbContext dbContext, IPostService postService) : Controller
{
    private UserManager<User> _userManager { get; init; } = userManager;
    private IPostService _postService { get; init; } = postService;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewBag.IsAuth = User.Identity?.IsAuthenticated ?? false;

        var posts = await _postService.GetAllPostsAsync();

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

using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using where_we_go.Models;

namespace where_we_go.Controllers;

public class HomeController(UserManager<User> userManager) : Controller
{
    private UserManager<User> _userManager { get; init; } = userManager;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        bool IsAuth = User.Identity?.IsAuthenticated ?? false;
        ViewBag.IsAuth = IsAuth;

        return View();

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

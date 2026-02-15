using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using where_we_go.DTO;
using where_we_go.Models;

namespace where_we_go.Controllers;

public class HomeController : Controller
{

    private readonly UserManager<User> _userManager;

    public HomeController(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

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

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

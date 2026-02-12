using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhereWeGo.Service;

namespace where_we_go.Controllers;

public class UserController(IUserService userService) : Controller
{
    private IUserService _userService { get; init; } = userService;

    [Authorize]
    public async Task<IActionResult> Me()
    {
        var user = await _userService.GetUserByEmailAsync("questiontime01@gmail.com");
        ViewData["User"] = user;
        if (user == null) return NotFound("User not found");
        return View();
    }

}



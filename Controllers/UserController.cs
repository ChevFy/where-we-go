using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using where_we_go.DTO;
using where_we_go.Models;

namespace where_we_go.Controllers;

public class UserController(UserManager<User> userManager) : Controller
{
    private UserManager<User> _userManager { get; init; } = userManager;

    [Authorize]
    public async Task<IActionResult> UserProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return RedirectToAction("Login", "Auth");
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return RedirectToAction("Login", "Auth");
        }
        var userReponse = new UserResponseDto(user);
        // Console.WriteLine(userReponse);
        return View(userReponse);
    }


}



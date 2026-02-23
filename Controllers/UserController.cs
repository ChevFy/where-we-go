using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using where_we_go.DTO;
using where_we_go.Models;

namespace where_we_go.Controllers;

public class UserController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager) : Controller
{
    private UserManager<User> _userManager { get; init; } = userManager;
    private RoleManager<IdentityRole> _roleManager { get; init; } = roleManager;

    
    [HttpGet]
    public async Task<IActionResult> UserProfile(string? username)
    {
        if(username is null)
            return RedirectToAction("Index","Home"); // ทำเป็น redirect ไปหน้า Home ไปก่อน
        
        var targetUser = await _userManager.FindByNameAsync(username);
        if(targetUser is null)
            return RedirectToAction("Index","Home"); // ทำเป็น redirect ไปหน้า Home ไปก่อน

        var roles = (await _userManager.GetRolesAsync(targetUser)).ToArray();
        var userResponse = new UserResponseDto(targetUser, roles);
        
        bool IsAuth = User.Identity?.IsAuthenticated ?? false;
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        bool isOwner = IsAuth && currentUserId == targetUser.Id;
        ViewBag.isOwner = isOwner;

        return View(userResponse);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> UserEdit()
    {
        var user = await _userManager.GetUserAsync(User);
        if(user is null)
            return NotFound();
        var roles = (await _userManager.GetRolesAsync(user)).ToArray();
        var userResponse = new UserResponseDto(user, roles);
        

        return View(userResponse);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult>  UpdateUser(UpdateUserDto model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return NotFound();

        user.Name = model.Name;
        user.UserName = model.userName;
        user.Bio = model.Bio;
        user.ProfileUrl = model.ProfileUrl;

        var result = await _userManager.UpdateAsync(user);

        if(result.Succeeded)
        {
            TempData["Success"] = "updated success";
            return RedirectToAction("User","Userprofile" , new { username = user.UserName });
        }

        
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }


}



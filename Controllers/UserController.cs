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

    
    public async Task<IActionResult> UserProfile(string? username)
    {
        if(username is null)
            return RedirectToAction("Index","Home"); // ทำเป็น redirect ไปหน้า Home ไปก่อน
        
        var targetUser = await _userManager.FindByNameAsync(username);
        if(targetUser is null)
            return RedirectToAction("Index","Home"); // ทำเป็น redirect ไปหน้า Home ไปก่อน

        var roles = (await _userManager.GetRolesAsync(targetUser)).ToArray();
        var userResponse = new UserResponseDto(targetUser, roles);
        
        return View(userResponse);
    }


}



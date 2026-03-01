using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Text.Json;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

using where_we_go.DTO;
using where_we_go.Models;
using where_we_go.Service;

namespace where_we_go.Controllers;

public class UserController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IUserService userService, IFileService fileService) : Controller
{
    private UserManager<User> _userManager { get; init; } = userManager;
    private RoleManager<IdentityRole> _roleManager { get; init; } = roleManager;
    private IUserService _userService { get; init; } = userService;

    private IFileService _fileService { get; init; } = fileService;


    [HttpGet]
    public async Task<IActionResult> UserProfile(string? username)
    {
        if (username is null)
            return RedirectToAction("Index", "Home"); // ทำเป็น redirect ไปหน้า Home ไปก่อน

        var targetUser = await _userManager.FindByNameAsync(username);
        if (targetUser is null)
            return RedirectToAction("Index", "Home"); // ทำเป็น redirect ไปหน้า Home ไปก่อน

        var roles = (await _userManager.GetRolesAsync(targetUser)).ToArray();
        var profileUrl = await _fileService.GeneratePresignedUrl(targetUser.ProfileImageKey);
        if(profileUrl is null)
                profileUrl = "https://cdn.pixabay.com/photo/2015/10/05/22/37/blank-profile-picture-973460_960_720.png";


        var userResponse = new UserResponseDto(targetUser, roles,profileUrl);

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
        if (user is null)
            return NotFound();
        var roles = (await _userManager.GetRolesAsync(user)).ToArray();
        var profileUrl = await _fileService.GeneratePresignedUrl(user.ProfileImageKey);
        if(profileUrl is null)
                profileUrl = "https://cdn.pixabay.com/photo/2015/10/05/22/37/blank-profile-picture-973460_960_720.png";

        var userResponse = new UserResponseDto(user, roles,profileUrl);


        return View(userResponse);
    }

    [Authorize]
    [HttpPut]
    [ValidateAntiForgeryToken]
    [Route("api/user/update")]
    public async Task<IActionResult> UpdateUser([FromBody] SelfUpdateUserDto model)
    {

        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = ModelState });
        }

        if (string.IsNullOrEmpty(model.userName))
            return BadRequest(new { message = "Username is required" });


        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return NotFound();

        //Check duplicate
        var existUser = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == model.userName && u.Id != user.Id);
        if (existUser is not null)
        {
            return BadRequest(new { message = "This Username already exist" });
        }

        user.Name = model.Name;
        user.UserName = model.userName;
        user.Bio = model.Bio;
        user.DateUpdated = DateTime.UtcNow;
        
        if (string.IsNullOrWhiteSpace(model.ProfileUrl))
                user.ProfileImageKey =  user.ProfileImageKey;
        else    
                 user.ProfileImageKey = model.ProfileUrl ;

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            TempData["Success"] = "updated success";
            return Ok(new
            {
                message = "Updated success",
                redirectUrl = Url.Action("UserProfile", "User", new { username = user.UserName })
            });
        }


        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return BadRequest(new { message = ModelState });
    }

    [HttpGet]
    public async Task<IActionResult> Test([FromQuery] UserQueryDto query)
    {
        var user = await _userService.GetUsersAsync(query);
        return View(user);
    }
}
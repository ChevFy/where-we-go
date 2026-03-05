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

public class UserController(UserManager<User> userManager, IUserService userService, IFileService fileService) : Controller
{

    [HttpGet]
    public async Task<IActionResult> UserProfile(string? username)
    {
        if (username is null)
            return RedirectToAction("Index", "Home"); // ทำเป็น redirect ไปหน้า Home ไปก่อน

        var targetUser = await userManager.FindByNameAsync(username);
        if (targetUser is null)
            return RedirectToAction("Index", "Home"); // ทำเป็น redirect ไปหน้า Home ไปก่อน

        var roles = (await userManager.GetRolesAsync(targetUser)).ToArray();
        var defaultUrl = "https://cdn.pixabay.com/photo/2015/10/05/22/37/blank-profile-picture-973460_960_720.png";
        var profileUrl = (await fileService.GeneratePresignedProfileUrlAsync(targetUser.ProfileImageKey)) ?? defaultUrl;


        var userResponse = new UserResponseDto(targetUser, roles, profileUrl);

        bool isAuth = User.Identity?.IsAuthenticated ?? false;
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        bool isOwner = isAuth && currentUserId == targetUser.Id;
        ViewBag.isOwner = isOwner;

        return View(userResponse);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> UserEdit()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
            return NotFound();
        var roles = (await userManager.GetRolesAsync(user)).ToArray();
        var defaultUrl = "https://cdn.pixabay.com/photo/2015/10/05/22/37/blank-profile-picture-973460_960_720.png";
        var profileUrl = (await fileService.GeneratePresignedProfileUrlAsync(user.ProfileImageKey)) ?? defaultUrl;

        var userResponse = new UserResponseDto(user, roles, profileUrl);

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


        var user = await userManager.GetUserAsync(User);
        if (user is null)
            return NotFound();

        //Check duplicate
        var existUser = await userManager.Users.FirstOrDefaultAsync(u => u.UserName == model.userName && u.Id != user.Id);
        if (existUser is not null)
        {
            return BadRequest(new { message = "This Username already exist" });
        }

        if (!string.IsNullOrWhiteSpace(model.Name))
            user.Name = model.Name;
        user.UserName = model.userName;
        user.Bio = model.Bio;
        user.DateUpdated = DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(model.ProfileUrl))
            user.ProfileImageKey = user.ProfileImageKey;
        else
            user.ProfileImageKey = model.ProfileUrl;

        var result = await userManager.UpdateAsync(user);

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
        var user = await userService.GetUsersAsync(query);
        return View(user);
    }
}
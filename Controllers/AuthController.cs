using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using where_we_go.Models;
using where_we_go.DTO;

namespace where_we_go.Controllers;

public class AuthController(SignInManager<User> signInManager, UserManager<User> userManager) : Controller
{
    private SignInManager<User> _signInManager { get; init; } = signInManager;
    private UserManager<User> _userManager { get; init; } = userManager;

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "No account found with this email");
            return View(model);
        }

        var hasPassword = await _userManager.HasPasswordAsync(user);

        if (!hasPassword)
        {
            var logins = await _userManager.GetLoginsAsync(user);
            var providers = string.Join(", ", logins.Select(l => l.LoginProvider));

            ModelState.AddModelError(string.Empty, $"This account was created using {providers}");
            return View(model);
        }


        var result = await _signInManager.PasswordSignInAsync(user, model.Password, isPersistent: true, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "Incorrect password. Please try again.");
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new User
        {
            UserName = model.Email,
            Email = model.Email,
            Name = $"{model.Firstname} {model.Lastname}",
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: true);
            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [HttpPost]
    public IActionResult ExternalLogin(string provider, string? returnUrl = null)
    {
        var redirectUrl = Url.Action("ExternalLoginCallback", "Auth", new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    [HttpGet]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null)
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            return RedirectToAction("Login");
        }

        var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true);
        if (result.Succeeded)
        {
            return LocalRedirect(returnUrl ?? "/");
        }

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        var username = info.Principal.FindFirstValue(ClaimTypes.GivenName);
        var name = info.Principal.FindFirstValue(ClaimTypes.Name) ?? "";

        var user = new User
        {
            UserName = username,
            Email = email,
            Name = name,
            EmailConfirmed = true,
            OAuthId = info.ProviderKey,
        };

        var createdResult = await _userManager.CreateAsync(user);
        if (createdResult.Succeeded)
        {
            await _userManager.AddLoginAsync(user, info);
            await _signInManager.SignInAsync(user, isPersistent: true);
            return LocalRedirect(returnUrl ?? "/");
        }

        return RedirectToAction("Login");
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}


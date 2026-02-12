using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using WhereWeGo.Service;
using System.Security.Claims;
using WhereWeGo.DTO;

namespace where_we_go.Controllers;

public class AuthController(IAuthService authService) : Controller
{
    private IAuthService _authService { get; init; } = authService;

    public IActionResult Login()
    {
        var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse") };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    public async Task<IActionResult> GoogleResponse()
    {
        var authenticationResult = await HttpContext.AuthenticateAsync();
        var claims = authenticationResult.Principal?.Identities.FirstOrDefault()?.Claims;
        var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var givenName = claims?.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value;
        var providerKey = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        await _authService.LoginGoogleAsync(new LoginGoogleDto
        {
            Email = email ?? string.Empty,
            Name = name ?? string.Empty,
            GivenName = givenName ?? string.Empty,
            ProviderKey = providerKey ?? string.Empty,
            Provider = GoogleDefaults.AuthenticationScheme
        });
        return RedirectToAction("Me", "User");
    }


    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}


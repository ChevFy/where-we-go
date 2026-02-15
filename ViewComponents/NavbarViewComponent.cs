
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using where_we_go.Models;
using System.Security.Claims;
using where_we_go.DTO;

namespace where_we_go.ViewComponents
{
    public class NavbarViewComponent(UserManager<User> userManager) : ViewComponent
    {

        private UserManager<User> _userManager { get; init; } = userManager;

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userPrincipal = HttpContext.User;
            bool IsAuth = User.Identity?.IsAuthenticated ?? false;
            ViewBag.IsAuth = IsAuth;
            
   
            var user = await _userManager.GetUserAsync(userPrincipal);
            if (user == null)
                    return View(null);
            var userReponse = new UserResponseDto(user);
            return View(userReponse);
        }
    }
}
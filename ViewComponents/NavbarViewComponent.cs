
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using where_we_go.Models;
using System.Security.Claims;
using where_we_go.DTO;

namespace where_we_go.ViewComponents
{
    public class NavbarViewComponent : ViewComponent
    {

        private readonly UserManager<User> _userManager;

        public NavbarViewComponent(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userPrincipal = HttpContext.User;
            bool IsAuth = User.Identity?.IsAuthenticated ?? false;
            ViewBag.IsAuth = IsAuth;
            
            var userId = _userManager.GetUserId(userPrincipal);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                    return View(null);
            var userReponse = new UserResponseDto(user);
            return View(userReponse);
        }
    }
}
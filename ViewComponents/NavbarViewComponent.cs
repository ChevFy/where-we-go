
using System.Security.Claims;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using where_we_go.DTO;
using where_we_go.Models;
using where_we_go.Service;

namespace where_we_go.ViewComponents
{
    public class NavbarViewComponent(UserManager<User> userManager, IFileService fileService) : ViewComponent
    {

        private UserManager<User> _userManager { get; init; } = userManager;
        private IFileService _fileService { get; init; } = fileService;

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userPrincipal = HttpContext.User;
            bool IsAuth = User.Identity?.IsAuthenticated ?? false;
            ViewBag.IsAuth = IsAuth;


            var user = await _userManager.GetUserAsync(userPrincipal);
            if (user == null)
                return View(null);

            var role = (await _userManager.GetRolesAsync(user)).ToArray();

            var profileUrl = await _fileService.GeneratePresignedProfileUrlAsync(user.ProfileImageKey);
            if(profileUrl is null)
                profileUrl = "https://cdn.pixabay.com/photo/2015/10/05/22/37/blank-profile-picture-973460_960_720.png";

            var userReponse = new UserResponseDto(user, role, profileUrl);
            return View(userReponse);
        }
    }
}

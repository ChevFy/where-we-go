
using Microsoft.AspNetCore.Mvc;
using where_we_go.Models;

namespace where_we_go.ViewComponents
{
    public class NavbarViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            bool IsAuth = User.Identity?.IsAuthenticated ?? false;
            string userName = User.Identity?.Name ?? "";

            ViewBag.IsAuth = IsAuth;
            ViewBag.UserName = userName;

            return View();
        }
    }
}
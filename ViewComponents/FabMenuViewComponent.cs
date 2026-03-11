using Microsoft.AspNetCore.Mvc;

namespace where_we_go.ViewComponents
{
    public class FabMenuViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            bool isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            ViewBag.IsAuth = isAuthenticated;

            return View();
        }
    }
}


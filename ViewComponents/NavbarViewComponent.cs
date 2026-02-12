
using Microsoft.AspNetCore.Mvc;

namespace where_we_go.ViewComponents
{
    public class NavbarViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View();
        }
    }
}
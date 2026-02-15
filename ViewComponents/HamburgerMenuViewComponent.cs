using Microsoft.AspNetCore.Mvc;

namespace where_we_go.ViewComponents
{
    public class NavItem
    {
        public string Text { get; set; } = null!;
        public string Href { get; set; } = "#";
        public string? IconClass { get; set; } 
    }

    public class HamburgerMenuViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(List<NavItem>? items)            
        {
            var menuItems = items ?? new List<NavItem>();
            return View(menuItems);
        }
    }
}
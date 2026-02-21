using Microsoft.AspNetCore.Mvc;
using where_we_go.Models;

namespace where_we_go.ViewComponents
{
    public class PostCardViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(Post post)
        {
            return View(post);
        }
    }
}
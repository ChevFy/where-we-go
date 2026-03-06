using Microsoft.AspNetCore.Mvc;

using where_we_go.Models;

namespace where_we_go.ViewComponents
{
    public class UserPostCardViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(PostDto post)
        {
            return View(post);
        }
    }
}



using Microsoft.AspNetCore.Mvc;

namespace where_we_go.ViewComponents
{
    public class InputBarViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(
            string targetId,
            string inputType,
            string placeHolder,
            string width = "100%", 
            string height = "50px"
            )

            {
            ViewBag.TargetId = targetId;
            ViewBag.inputType = inputType;
            ViewBag.placeHolder = placeHolder;
            ViewBag.width = width;
            ViewBag.height = height;
            return View();
        }
    }
}
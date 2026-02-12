
using Microsoft.AspNetCore.Mvc;

namespace where_we_go.ViewComponents
{
    public class MainButtonViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(
            string targetId,
            string buttonType,
            string text, 
            string color = "#00B4D8",
            string hoverColor = "#0077B6",
            string width = "10rem",
            string imageUrl = null!,
            string textColor = "white"
            )
            {

            ViewBag.Text = text;
            ViewBag.Color = color;
            ViewBag.HoverColor = hoverColor;
            ViewBag.Width = width;
            ViewBag.TargetId = targetId;
            ViewBag.ButtonType = buttonType;
            ViewBag.TextColor = textColor;
            ViewBag.ImageUrl = imageUrl;
            return View();
        }
    }
}
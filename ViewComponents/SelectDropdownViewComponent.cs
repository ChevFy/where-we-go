

using Microsoft.AspNetCore.Mvc;

namespace where_we_go.ViewComponents
{

    public class SelectDropdownItem
    {
        public string Text { get; set; } = null!;

        public string Value { get; set; } = null!;

    }

    public class SelectDropdownViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(
            string targetId,
            string mainText,
            List<SelectDropdownItem>? items = null,
            string width = "200px",
            string height = "40px")
        {
            ViewBag.width = width;
            ViewBag.height = height;
            ViewBag.mainText = mainText;
            ViewBag.targetId = targetId;
            var resultModel = items ?? new List<SelectDropdownItem>();
            return View(resultModel);
        }
    }
}
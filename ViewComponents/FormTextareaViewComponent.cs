using Microsoft.AspNetCore.Mvc;

namespace where_we_go.ViewComponents
{
    public class FormTextareaViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(
            string id,
            string name,
            string label,
            bool required = false,
            string placeholder = "",
            string value = "",
            string cssClass = "form-group",
            int rows = 4
        )
        {
            ViewBag.Id = id;
            ViewBag.Name = name;
            ViewBag.Label = label;
            ViewBag.Required = required;
            ViewBag.Placeholder = placeholder;
            ViewBag.Value = value;
            ViewBag.CssClass = cssClass;
            ViewBag.Rows = rows;
            return View();
        }
    }
}
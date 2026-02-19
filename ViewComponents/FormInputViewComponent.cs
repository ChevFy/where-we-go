using Microsoft.AspNetCore.Mvc;

namespace where_we_go.ViewComponents
{
    public class FormInputViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(
            string id,
            string name,
            string label,
            string type = "text",
            bool required = false,
            string placeholder = "",
            string value = "",
            string cssClass = "form-group"
        )
        {
            ViewBag.Id = id;
            ViewBag.Name = name;
            ViewBag.Label = label;
            ViewBag.Type = type;
            ViewBag.Required = required;
            ViewBag.Placeholder = placeholder;
            ViewBag.Value = value;
            ViewBag.CssClass = cssClass;
            return View();
        }
    }
}
using Microsoft.AspNetCore.Mvc;

namespace where_we_go.ViewComponents
{
    public class SelectViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(
            string name,
            List<SelectOption> options,
            string? selectedValue = null,
            string? label = null,
            string? cssClass = null)
        {
            var model = new SelectViewModel
            {
                Name = name,
                Options = options,
                SelectedValue = selectedValue,
                Label = label,
                CssClass = cssClass
            };
            return View(model);
        }
    }

    public class SelectViewModel
    {
        public required string Name { get; set; }
        public required List<SelectOption> Options { get; set; }
        public string? SelectedValue { get; set; }
        public string? Label { get; set; }
        public string? CssClass { get; set; }
    }

    public class SelectOption
    {
        public required string Value { get; set; }
        public required string Text { get; set; }
    }
}
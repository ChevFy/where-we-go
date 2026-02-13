

using Microsoft.AspNetCore.Mvc;

namespace where_we_go.ViewComponents
{

    public class SelectDropdownItem
    {
        public string Text {get; set;} = null!;
        
        public string Value {get; set;} = null!;

    }

    public class SelectDropdownViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync( 
            string targetId , 
            string mainText , 
            string width,
            string height,
            List<SelectDropdownItem> items)            
        {
            ViewBag.width = width;
            ViewBag.height = height;
            ViewBag.mainText = mainText;
            ViewBag.targetId = targetId;
            return View(items);
        }
    }
}
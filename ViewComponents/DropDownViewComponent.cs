

using Microsoft.AspNetCore.Mvc;

namespace where_we_go.ViewComponents
{

    public class DropDownItem
    {
        public string Text {get; set;} = null!;
        public string TargetId {get; set; }  =null!;
        public string Href {get; set;} = null!;
    }

    public class DropDownViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync( string mainText , List<DropDownItem> items)            
        {
            ViewBag.mainText = mainText;
            return View(items);
        }
    }
}
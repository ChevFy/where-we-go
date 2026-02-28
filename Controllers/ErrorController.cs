using Microsoft.AspNetCore.Mvc;

namespace where_we_go.Controllers
{
    public class ErrorController : Controller
    {
        [Route("404")]
        public async Task<IActionResult> NotFoundPage()
        {
            return View(); 
        }
    }
}
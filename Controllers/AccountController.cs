using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using where_we_go.Service;
using System.Security.Claims;
using where_we_go.DTO;


namespace where_we_go.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult RegisterPage()
        {
            return View();
        }
        
    }
}
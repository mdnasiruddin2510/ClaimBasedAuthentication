using Microsoft.AspNetCore.Mvc;

namespace ClaimBasedAuthentication.Core.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }
    }
}

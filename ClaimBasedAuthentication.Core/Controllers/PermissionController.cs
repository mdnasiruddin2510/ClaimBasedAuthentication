using Microsoft.AspNetCore.Mvc;

namespace ClaimBasedAuthentication.Core.Controllers
{
    public class PermissionController : Controller
    {
        public IActionResult Role()
        {
            return View();
        }
    }
}

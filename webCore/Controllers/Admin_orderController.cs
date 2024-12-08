using Microsoft.AspNetCore.Mvc;

namespace webCore.Controllers
{
    public class Admin_orderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

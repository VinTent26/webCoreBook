using Microsoft.AspNetCore.Mvc;

namespace webCore.Controllers
{
    public class CategoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

using Microsoft.AspNetCore.Mvc;

namespace webCore.Controllers
{
    public class PaymentController : Controller
    {
        public IActionResult Payment1()
        {
            return View();
        }
        public IActionResult Payment2()
        {
            return View();
        }
    }
}

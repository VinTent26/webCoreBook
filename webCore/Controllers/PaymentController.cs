using Microsoft.AspNetCore.Mvc;

namespace webCore.Controllers
{
    public class PaymentController : Controller
    {
        public IActionResult Payment1()
        {
            return View();
        }
    }
}

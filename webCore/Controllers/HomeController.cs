using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using webCore.Models;

namespace webCore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // Thực hiện hiển thị trang chủ (Index)
        [HttpGet]
        public IActionResult Index()
        {
            // Trả về view của trang chủ
            return View();
        }

        // Thực hiện trả về view Privacy
        public IActionResult Privacy()
        {
            return View();
        }

        // Xử lý lỗi khi có sự cố xảy ra
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

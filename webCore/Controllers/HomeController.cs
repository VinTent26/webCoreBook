using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using webCore.Models;
using webCore.Services;

namespace webCore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MongoDBService _mongoDBService;

        public HomeController(ILogger<HomeController> logger, MongoDBService mongoDBService)
        {
            _logger = logger;
            _mongoDBService = mongoDBService;
        }

        // Action Index, trả về trang chủ và lấy dữ liệu từ MongoDB
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách danh mục từ MongoDB
            var categories = await _mongoDBService.GetCategoriesAsync();
            ViewBag.Categories = categories;

            // Lấy danh sách sản phẩm từ MongoDB
            var products = await _mongoDBService.GetProductsAsync();
            ViewBag.Products = products;

            return View(); // Trả về view mặc định Index.cshtml
        }
    }
}

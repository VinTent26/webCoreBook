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

            // Lấy danh sách sản phẩm nhóm theo trạng thái Featured
            var groupedProducts = await _mongoDBService.GetProductsGroupedByFeaturedAsync();
            ViewBag.GroupedProducts = groupedProducts;

            return View(); // Trả về view Index.cshtml
        }
        // Trả về danh sách sách theo Position
        public async Task<IActionResult> GetProductsByPosition(int position)
        {
            // Lấy danh sách sách theo Position từ MongoDB
            var products = await _mongoDBService.GetProductsByCategoryPositionAsync(position);

            // Trả về Partial View
            return PartialView("_BookListPartial", products);
        }
    }
}

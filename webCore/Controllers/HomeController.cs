using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using webCore.Models;
using webCore.Services;
using System.Linq;

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
        // Lấy danh sách sản phẩm theo trạng thái Featured
        // Phương thức tìm kiếm
        public async Task<IActionResult> Search(string searchQuery)
        {
            if (string.IsNullOrEmpty(searchQuery))
            {
                return PartialView("_ProductList", new List<Product_admin>());
            }

            // Tìm kiếm sản phẩm từ MongoDB
            var allProducts = await _mongoDBService.GetProductsAsync(); // Lấy toàn bộ sản phẩm
            var searchResults = allProducts
                .Where(p => p.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Trả về PartialView với kết quả
            return PartialView("_ProductList", searchResults);
        }
    }
}

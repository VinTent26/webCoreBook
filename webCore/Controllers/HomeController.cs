using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using webCore.Models;
using webCore.Services;
using System.Linq;
using webCore.MongoHelper;

namespace webCore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MongoDBService _mongoDBService;
        private readonly ProductService _productService;
        private readonly CategoryService _categoryService;
        public HomeController(MongoDBService mongoDBService, ProductService productService, CategoryService categoryService)
        {
            _mongoDBService = mongoDBService;
            _productService = productService;
            _categoryService = categoryService;
        }

        // Action Index, trả về trang chủ và lấy dữ liệu từ MongoDB
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách danh mục từ MongoDB
            var categories = await _categoryService.GetCategoriesAsync();
            ViewBag.Categories = categories;

            // Lấy danh sách sản phẩm nhóm theo trạng thái Featured
            var groupedProducts = await _productService.GetProductsGroupedByFeaturedAsync();
            ViewBag.GroupedProducts = groupedProducts;

            return View(); // Trả về view Index.cshtml
        }
        // Trả về danh sách sách theo Position
        public async Task<IActionResult> GetProductsByPosition(int position)
        {
            // Lấy danh sách sách theo Position từ MongoDB
            var products = await _productService.GetProductsByCategoryPositionAsync(position);

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
            var allProducts = await _productService.GetProductsAsync(); // Lấy toàn bộ sản phẩm
            var searchResults = allProducts
                .Where(p => p.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Trả về PartialView với kết quả
            return PartialView("_ProductList", searchResults);
        }
    }
}

using Microsoft.AspNetCore.Http;
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
        private readonly MongoDBService _mongoDBService;
        private readonly ProductService _productService;
        private readonly CategoryService _categoryService;
        private readonly UserService _userService;

        public HomeController(MongoDBService mongoDBService, ProductService productService, CategoryService categoryService, UserService userService)
        {
            _mongoDBService = mongoDBService;
            _productService = productService;
            _categoryService = categoryService;
            _userService = userService;
        }

        // Action Index, trả về trang chủ và lấy dữ liệu từ MongoDB
        [ServiceFilter(typeof(SetLoginStatusFilter))]
        public async Task<IActionResult> Index(string sortOption = "default")
        {
            // Kiểm tra trạng thái đăng nhập từ session
            var isLoggedIn = HttpContext.Session.GetString("UserToken") != null;

            // Truyền thông tin vào ViewBag hoặc Model để sử dụng trong View
            ViewBag.IsLoggedIn = isLoggedIn;

            // Lấy danh sách danh mục từ MongoDB
            var categories = await _categoryService.GetCategoriesAsync();
            ViewBag.Categories = categories;

            // Lấy danh sách sản phẩm nhóm theo trạng thái Featured
            var groupedProducts = await _productService.GetProductsGroupedByFeaturedAsync();
            ViewBag.GroupedProducts = groupedProducts;

            // Get and sort products by the given sort option
            var allProducts = await _productService.GetProductsAsync();
            var sortedProducts = _productService.SortProducts(allProducts, sortOption);
            ViewBag.SortedProducts = sortedProducts;

            // Lấy danh sách sản phẩm nổi bật
            var featuredProducts = await _productService.GetFeaturedProductsAsync();
            ViewBag.FeaturedProducts = featuredProducts;

            // Lấy danh sách sản phẩm bán chạy
            var bestsellerProducts = await _productService.GetBestsellerProductsAsync();
            ViewBag.BestsellerProducts = bestsellerProducts;

            return View(); // Trả về view Index.cshtml
        }
        // Action này sẽ nhận yêu cầu AJAX để lấy danh sách sản phẩm đã sắp xếp
        public async Task<IActionResult> GetProductsByCategoryId(string categoryId)
        {
            // Gọi hàm lấy sản phẩm
            var products = await _productService.GetProductsByCategoryIdAsync(categoryId);

            // Trả về partial view với danh sách sản phẩm đã sắp xếp
            return PartialView("_BookListPartial", products);
        }

        // Phương thức tìm kiếm sản phẩm
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

        // Action để đăng nhập và lưu thông tin session
        [HttpPost]
        public async Task<IActionResult> Sign_in(User loginUser)
        {
            if (!ModelState.IsValid)
            {
                return View(loginUser);
            }

            // Lấy tài khoản từ MongoDB dựa trên email
            var user = await _userService.GetAccountByEmailAsync(loginUser.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Tài khoản không tồn tại.");
                return View(loginUser);
            }

            // So sánh mật khẩu
            if (loginUser.Password != user.Password)
            {
                ModelState.AddModelError("", "Mật khẩu không đúng.");
                return View(loginUser);
            }

            // Lưu thông tin đăng nhập vào session
            HttpContext.Session.SetString("UserToken", user.Token);
            HttpContext.Session.SetString("UserName", user.Name);

            // Sau khi đăng nhập thành công, chuyển hướng về trang chủ
            return RedirectToAction("Index", "Home");
        }

        // Action để đăng xuất, xóa session
        [HttpPost]
        public IActionResult Sign_out()
        {
            // Xóa thông tin session khi người dùng đăng xuất
            HttpContext.Session.Remove("UserToken");
            HttpContext.Session.Remove("UserName");

            return RedirectToAction("Index", "Home");
        }

        [HttpGet("api/breadcrumbs/{categoryId}")]
        public async Task<IActionResult> GetBreadcrumbs(string categoryId)
        {
            if (string.IsNullOrEmpty(categoryId))
            {
                return BadRequest("Category ID is required.");
            }

            var breadcrumbs = new List<Category>();
            string currentCategoryId = categoryId;

            while (!string.IsNullOrEmpty(currentCategoryId))
            {
                var category = await _categoryService.GetCategoryBreadcrumbByIdAsync(currentCategoryId);
                if (category != null)
                {
                    breadcrumbs.Insert(0, category);
                    currentCategoryId = category.ParentId;
                }
                else
                {
                    break;
                }
            }

            return Ok(breadcrumbs);
        }
    }
}

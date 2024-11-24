using Microsoft.AspNetCore.Http;
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
        private readonly IHttpContextAccessor _httpContextAccessor;
        public HomeController(ILogger<HomeController> logger, MongoDBService mongoDBService, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _mongoDBService = mongoDBService;
            _httpContextAccessor = httpContextAccessor;
        }

        // Action Index, trả về trang chủ và lấy dữ liệu từ MongoDB

        [ServiceFilter(typeof(SetLoginStatusFilter))]
        public async Task<IActionResult> Index()
        {

            var isLoggedIn = HttpContext.Items["IsLoggedIn"] != null && (bool)HttpContext.Items["IsLoggedIn"];

            // Truyền thông tin vào ViewBag hoặc Model để có thể sử dụng trong View
            ViewBag.IsLoggedIn = isLoggedIn;

            // Lấy danh sách danh mục từ MongoDB
            var categories = await _mongoDBService.GetCategoriesAsync();
            ViewBag.Categories = categories;

            // Lấy danh sách sản phẩm từ MongoDB
            var products = await _mongoDBService.GetProductsAsync();
            ViewBag.Products = products;

            return View(); // Trả về view mặc định Index.cshtml
        }
        [HttpGet("api/breadcrumbs/{categoryId}")]
        public async Task<IActionResult> GetBreadcrumbs(string categoryId)
        {
            if (string.IsNullOrEmpty(categoryId))
            {
                return BadRequest("Category ID is required.");
            }

            // Truy vấn danh mục breadcrumb
            var breadcrumbs = new List<Category>();
            string currentCategoryId = categoryId;

            while (!string.IsNullOrEmpty(currentCategoryId))
            {
                var category = await _mongoDBService.GetCategoryBreadcrumbByIdAsync(currentCategoryId);
                if (category != null)
                {
                    breadcrumbs.Insert(0, category); // Thêm vào đầu danh sách
                    currentCategoryId = category.ParentId; // Lấy danh mục cha
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

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
            // Kiểm tra trạng thái đăng nhập từ session
            var isLoggedIn = HttpContext.Session.GetString("UserToken") != null;

            // Truyền thông tin vào ViewBag hoặc Model để sử dụng trong View
            ViewBag.IsLoggedIn = isLoggedIn;

            // Lấy danh sách danh mục từ MongoDB
            var categories = await _mongoDBService.GetCategoriesAsync();
            ViewBag.Categories = categories;

            // Lấy danh sách sản phẩm từ MongoDB
            var products = await _mongoDBService.GetProductsAsync();
            ViewBag.Products = products;

            return View(); // Trả về view mặc định Index.cshtml
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
            var user = await _mongoDBService.GetAccountByEmailAsync(loginUser.Email);
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
                var category = await _mongoDBService.GetCategoryBreadcrumbByIdAsync(currentCategoryId);
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

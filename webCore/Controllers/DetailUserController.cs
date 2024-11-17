using Microsoft.AspNetCore.Mvc;
using webCore.Services;
using webCore.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace webCore.Controllers
{
    public class DetailUserController : Controller
    {
        private readonly MongoDBService _mongoDBService;

        public DetailUserController(MongoDBService mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }

        // Action hiển thị thông tin người dùng
        public async Task<IActionResult> Index()
        {
            // Lấy tên người dùng từ session
            var userName = HttpContext.Session.GetString("UserName");

            if (string.IsNullOrEmpty(userName))
            {
                // Nếu không có tên người dùng trong session, chuyển hướng đến trang đăng nhập
                return RedirectToAction("SignIn", "User");
            }

            // Lấy thông tin người dùng từ MongoDB theo tên
            var user = await _mongoDBService.GetUserByNameAsync(userName);

            if (user == null)
            {
                // Nếu không tìm thấy người dùng, hiển thị thông báo lỗi
                TempData["Message"] = "Không tìm thấy thông tin người dùng.";
                return View();
            }

            // Trả về view với thông tin người dùng
            return View(user);
        }

        // Action cập nhật thông tin người dùng
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(User user)
        {
            // Cập nhật thông tin người dùng trong MongoDB
            await _mongoDBService.UpdateUserAsync(user);

            // Sau khi cập nhật thành công, quay lại trang chi tiết
            return RedirectToAction("Index", "DetailUser");
        }
    }
}

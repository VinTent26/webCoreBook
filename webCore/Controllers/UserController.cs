using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using webCore.Models;
using webCore.Services;

namespace webCore.Controllers
{
    public class UserController : Controller
    {
        private readonly MongoDBService _mongoDBService;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public UserController(MongoDBService mongoDBService, IHttpContextAccessor httpContextAccessor)
        {
            _mongoDBService = mongoDBService;
            _httpContextAccessor = httpContextAccessor;
        }
        [ServiceFilter(typeof(SetLoginStatusFilter))]
        // Action để hiển thị form đăng ký
        [HttpGet]
        public IActionResult Sign_up()
        {
            return View();
        }

        // Action xử lý đăng ký
        [HttpPost]
        public async Task<IActionResult> Sign_up(User user)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem email đã tồn tại chưa
                var existingUser = await _mongoDBService.GetAccountByEmailAsync(user.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Tài khoản đã tồn tại.");
                    return View(user);
                }

                // Kiểm tra nếu mật khẩu và mật khẩu xác nhận không khớp nhau
                if (user.Password != user.ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "Mật khẩu và mật khẩu xác nhận không khớp.");
                    return View(user);
                }

                // Kiểm tra nếu account.Password là null hoặc chuỗi trống
                if (string.IsNullOrEmpty(user.Password))
                {
                    ModelState.AddModelError("Password", "Mật khẩu không được để trống.");
                    return View(user);
                }

                // Lưu tài khoản vào MongoDB
                await _mongoDBService.SaveUserAsync(user);

                // Thiết lập session cho tên người dùng
                HttpContext.Session.SetString("UserName", user.Name);

                return RedirectToAction("Index", "Home");
            }
            return View(user);
        }

        // Action để hiển thị form đăng nhập
        [HttpGet]
        public IActionResult Sign_in()
        {
            return View();  // Hiển thị trang đăng nhập
        }

        [ServiceFilter(typeof(SetLoginStatusFilter))]
        [HttpPost]
        public async Task<IActionResult> Sign_in(User loginUser)
        {
            if (!ModelState.IsValid)
            {
                // Trả lại view nếu ModelState không hợp lệ
                return View(loginUser);
            }

            // Lấy tài khoản từ MongoDB dựa vào email
            var user = await _mongoDBService.GetAccountByEmailAsync(loginUser.Email);

            if (user == null)
            {
                // Nếu tài khoản không tồn tại
                ModelState.AddModelError("", "Tài khoản không tồn tại.");
                return View(loginUser);
            }

            // So sánh mật khẩu nhập vào với mật khẩu trong MongoDB
            if (loginUser.Password != user.Password)
            {
                // Nếu mật khẩu không khớp
                ModelState.AddModelError("", "Mật khẩu không đúng.");
                return View(loginUser);
            }

            // Đăng nhập thành công
            HttpContext.Session.SetString("UserToken", user.Token);
            HttpContext.Session.SetString("UserName", user.Name);

            // Chuyển đến trang Index của controller Home
            return RedirectToAction("Index", "Home");
        }


        // Action xử lý đăng xuất
        [HttpPost]
        public IActionResult Sign_out()
        {
            // Xóa thông tin UserName và UserToken khỏi session
            HttpContext.Session.Remove("UserName");
            HttpContext.Session.Remove("UserToken");

            // Chuyển hướng về trang chủ
            return RedirectToAction("Index", "Home");
        }

    }
}

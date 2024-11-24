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

        public UserController(MongoDBService mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }

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

        [HttpPost]
        public async Task<IActionResult> Sign_in([FromBody] User loginUser)
        {
            if (ModelState.IsValid)
            {
                var user = await _mongoDBService.GetAccountByEmailAsync(loginUser.Email);

                if (user == null)
                {
                    return Json(new { success = false, message = "Tài khoản không tồn tại." });
                }

                if (loginUser.Password == user.Password)
                {
                    // Đăng nhập thành công, lưu thông tin vào session
                    HttpContext.Session.SetString("UserName", user.Name); // Lưu tên người dùng vào session
                    return Json(new { success = true, token = user.Token });
                }
                else
                {
                    return Json(new { success = false, message = "Mật khẩu không đúng." });
                }
            }

            return Json(new { success = false, message = "Thông tin không hợp lệ." });
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

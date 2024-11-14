using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        // Action xử lý đăng nhập
        [HttpPost]
        public async Task<IActionResult> Sign_in(User loginUser)
        {
            if (ModelState.IsValid)
            {

                // Lấy tài khoản từ MongoDB dựa vào email
                var user = await _mongoDBService.GetAccountByEmailAsync(loginUser.Email);

                if (user == null)
                {
                    // Nếu tài khoản không tồn tại
                    ModelState.AddModelError("", "Tài khoản không tồn tại.");
                    return View(loginUser);
                }

                // So sánh mật khẩu nhập vào với mật khẩu trong MongoDB (không mã hóa)
                if (loginUser.Password == user.Password)
                {
                    // Đăng nhập thành công
                    HttpContext.Session.SetString("UserToken", user.Token);
                    HttpContext.Session.SetString("UserName", user.Name);

                    // Chuyển đến trang Index của controller Home
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // Nếu mật khẩu không khớp
                    ModelState.AddModelError("", "Mật khẩu không đúng.");
                    return View(loginUser);
                }
            }

            // Trả lại view nếu ModelState không hợp lệ (ví dụ thiếu email hoặc mật khẩu)
            return View(loginUser);
        }
    }
}

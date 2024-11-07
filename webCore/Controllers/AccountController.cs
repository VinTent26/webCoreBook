using Microsoft.AspNetCore.Mvc;
using webCore.Models;
using webCore.Services;
using BCrypt.Net;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System;
using Microsoft.AspNetCore.Http;

namespace webCore.Controllers
{
    public class AccountController : Controller
    {
        private readonly MongoDBService _mongoDBService;

        public AccountController(MongoDBService mongoDBService)
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
        public async Task<IActionResult> Sign_up(Account account)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra nếu mật khẩu và mật khẩu xác nhận không khớp nhau
                if (account.Password != account.ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "Mật khẩu và mật khẩu xác nhận không khớp.");
                    return View(account);
                }

                // Kiểm tra nếu account.Password là null hoặc chuỗi trống
                if (string.IsNullOrEmpty(account.Password))
                {
                    ModelState.AddModelError("Password", "Mật khẩu không được để trống.");
                    return View(account);
                }

                // Lưu tài khoản vào MongoDB
                await _mongoDBService.SaveAccountAsync(account);

                return RedirectToAction("Sign_in");
            }
            return View(account);
        }

        // Action để hiển thị form đăng nhập
        [HttpGet]
        public IActionResult Sign_in()
        {
            return View();  // Hiển thị trang đăng nhập
        }

        // Action xử lý đăng nhập
        [HttpPost]
        public async Task<IActionResult> Sign_in(Account loginAccount)
        {
            if (ModelState.IsValid)
            {
         
                // Lấy tài khoản từ MongoDB dựa vào email
                var account = await _mongoDBService.GetAccountByEmailAsync(loginAccount.Email);

                if (account == null)
                {
                    // Nếu tài khoản không tồn tại
                    ModelState.AddModelError("", "Tài khoản không tồn tại.");
                    return View(loginAccount);
                }

                // So sánh mật khẩu nhập vào với mật khẩu trong MongoDB (không mã hóa)
                if (loginAccount.Password == account.Password)
                {
                    // Đăng nhập thành công
                    HttpContext.Session.SetString("UserToken", account.Token);

                    // Chuyển đến trang Index của controller Home
                    return RedirectToAction("Index", "User");
                }
                else
                {
                    // Nếu mật khẩu không khớp
                    ModelState.AddModelError("", "Mật khẩu không đúng.");
                    return View(loginAccount);
                }
            }

            // Trả lại view nếu ModelState không hợp lệ (ví dụ thiếu email hoặc mật khẩu)
            return View(loginAccount);
        }

    }
}

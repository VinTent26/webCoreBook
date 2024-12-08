using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using webCore.Models;
using webCore.MongoHelper;
using webCore.Services;

namespace webCore.Controllers
{
    public class Admin_singinController : Controller
    {
        private readonly AccountService _accountService;

        public Admin_singinController(AccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Tìm kiếm tài khoản trong MongoDB
            var account = (await _accountService.GetAccounts())
                .FirstOrDefault(a => a.Email == username && a.Password == password);

            if (account != null)
            {
                // Lưu token vào session
                HttpContext.Session.SetString("AdminToken", account.Token);
                HttpContext.Session.SetString("AdminName", account.FullName);
                HttpContext.Session.SetString("RoleId", account.RoleId);

                // Điều hướng đến trang dashboard
                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                ViewBag.ErrorMessage = "Tên tài khoản hoặc mật khẩu không đúng. Vui lòng thử lại.";
                return View("Index");
            }
        }

        [HttpPost]
        public IActionResult Logout()
        {
            // Xóa token khỏi session
            HttpContext.Session.Remove("AdminToken");

            return RedirectToAction("Index");
        }
    }
}
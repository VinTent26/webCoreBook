using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using webCore.Models;
using webCore.MongoHelper;
using webCore.Services;

public class DetailUserController : Controller
{
    private readonly MongoDBService _mongoDBService;
    private readonly UserService _userService;


    public DetailUserController(MongoDBService mongoDBService, UserService userService)
    {
        _mongoDBService = mongoDBService;
        _userService = userService;
    }

    // GET: DetailUser/Index
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userName = HttpContext.Session.GetString("UserName"); // Lấy thông tin UserName từ Session
        if (string.IsNullOrEmpty(userName))
        {
            return RedirectToAction("SignIn", "User");
        }

        var user = await _userService.GetUserByUsernameAsync(userName); // Lấy thông tin người dùng qua Username
        if (user == null)
        {
            TempData["Message"] = "Không tìm thấy thông tin người dùng.";
            return View();
        }

        // Đảm bảo ảnh đại diện hiện tại được gửi sang view
        ViewBag.ProfileImage = user.ProfileImage;

        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> Index(User model, IFormFile ProfileImage)
    {
        try
        {
            // Lấy tên người dùng từ session
            string currentUserName = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(currentUserName))
            {
                TempData["Message"] = "Không tìm thấy tên người dùng.";
                return RedirectToAction("SignIn", "User");
            }

            // Lấy thông tin người dùng hiện tại từ MongoDB
            var currentUser = await _userService.GetUserByUsernameAsync(currentUserName);
            if (currentUser == null)
            {
                TempData["Message"] = "Không tìm thấy thông tin người dùng.";
                return View(model);
            }

            // Cập nhật thông tin người dùng
            currentUser.Name = model.Name;
            HttpContext.Session.SetString("UserName", model.Name); // Cập nhật session với tên mới
            currentUser.Phone = model.Phone;
            currentUser.Gender = model.Gender;
            currentUser.Birthday = model.Birthday.HasValue
                ? DateTime.SpecifyKind(model.Birthday.Value.Date, DateTimeKind.Unspecified)
                : currentUser.Birthday;
            currentUser.Address = model.Address;

            // Cập nhật mật khẩu nếu có thay đổi
            if (!string.IsNullOrEmpty(model.Password))
            {
                if (long.TryParse(model.Password, out long numericPassword))
                {
                    currentUser.Password = numericPassword.ToString(); // Lưu mật khẩu dưới dạng chuỗi số
                }
                else
                {
                    var passwordHasher = new PasswordHasher<User>();
                    currentUser.Password = passwordHasher.HashPassword(currentUser, model.Password); // Mã hóa mật khẩu
                }
            }

            // Xử lý ảnh đại diện
            if (ProfileImage != null && ProfileImage.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    await ProfileImage.CopyToAsync(ms);
                    currentUser.ProfileImage = "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());
                }
            }

            // Thực hiện cập nhật thông tin người dùng trong MongoDB
            var updateResult = await _userService.UpdateUserAsync(currentUser);
            if (updateResult)
            {
                TempData["Message"] = "Cập nhật thông tin thành công!";
            }
            else
            {
                TempData["Message"] = "Không có thay đổi nào được thực hiện.";
            }

            // Đảm bảo ảnh đại diện hiển thị lại sau khi cập nhật
            ViewBag.ProfileImage = currentUser.ProfileImage;
        }
        catch (Exception ex)
        {
            TempData["Message"] = $"Đã xảy ra lỗi: {ex.Message}";
        }

        return View(model);
    }
}


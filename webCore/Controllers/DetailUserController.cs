using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using webCore.Models;
using webCore.Services;

public class DetailUserController : Controller
{
    private readonly MongoDBService _mongoDBService;

    public DetailUserController(MongoDBService mongoDBService)
    {
        _mongoDBService = mongoDBService;
    }

    // GET: DetailUser/Index
    public async Task<IActionResult> Index()
    {
        var userName = HttpContext.Session.GetString("UserName"); // Lấy thông tin UserName từ Session
        if (string.IsNullOrEmpty(userName))
        {
            return RedirectToAction("SignIn", "User");
        }

        var user = await _mongoDBService.GetUserByUsernameAsync(userName); // Lấy thông tin người dùng qua Username
        if (user == null)
        {
            TempData["Message"] = "Không tìm thấy thông tin người dùng.";
            return View();
        }

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
                return View(model);
            }

            // Lấy thông tin người dùng hiện tại từ MongoDB
            var currentUser = await _mongoDBService.GetUserByUsernameAsync(currentUserName);
            if (currentUser == null)
            {
                TempData["Message"] = "Không tìm thấy thông tin người dùng.";
                return View(model);
            }

            // Cập nhật thông tin người dùng, bao gồm tên người dùng mới
            currentUser.Name = model.Name;  // Cập nhật tên người dùng mới
            HttpContext.Session.SetString("UserName", model.Name); // Cập nhật lại session với tên mới

            currentUser.Phone = model.Phone;  // Cập nhật số điện thoại
            currentUser.Gender = model.Gender;  // Cập nhật giới tính
            currentUser.Birthday = model.Birthday.HasValue
                ? DateTime.SpecifyKind(model.Birthday.Value.Date, DateTimeKind.Unspecified)
                : currentUser.Birthday;  // Cập nhật ngày sinh
            currentUser.Address = model.Address;  // Cập nhật địa chỉ

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
                    currentUser.Password = passwordHasher.HashPassword(currentUser, model.Password);  // Mã hóa mật khẩu
                }
            }

            // Xử lý ảnh đại diện nếu có
            if (ProfileImage != null)
            {
                using (var ms = new MemoryStream())
                {
                    await ProfileImage.CopyToAsync(ms);
                    currentUser.ProfileImage = "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());
                }
            }

            // Thực hiện cập nhật thông tin người dùng trong MongoDB
            var updateResult = await _mongoDBService.UpdateUserAsync(currentUser);
            if (updateResult)
            {
                TempData["Message"] = "Cập nhật thông tin thành công!";
            }
            else
            {
                TempData["Message"] = "Không có thay đổi nào được thực hiện.";
            }
        }
        catch (Exception ex)
        {
            TempData["Message"] = $"Đã xảy ra lỗi: {ex.Message}";
        }

        return View(model);
    }
}


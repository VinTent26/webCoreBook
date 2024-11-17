using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using webCore.Models;
using webCore.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using webCore.MongoHelper;
using System.Linq;

namespace webCore.Controllers
{
    [AuthenticateHelper]
    public class AccountController : Controller
    {
        private readonly MongoDBService _mongoDBService;
        private readonly CloudinaryService _cloudinaryService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(MongoDBService mongoDBService, CloudinaryService cloudinaryService, ILogger<AccountController> logger)
        {
            _mongoDBService = mongoDBService;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        // Combine the two Index methods
        public async Task<IActionResult> Index()
        {
            // Fetch admin name from session
            var adminName = HttpContext.Session.GetString("AdminName");
            ViewBag.AdminName = adminName;

            try
            {
                // Fetch accounts asynchronously from MongoDB
                var accounts = await _mongoDBService.GetAccounts();
                return View(accounts); // Pass the accounts to the view
            }
            catch (Exception ex)
            {
                // Log error if fetching accounts fails
                _logger.LogError(ex, "Error fetching accounts from MongoDB.");
                return View("Error"); // Or any other error view
            }
        }

        public IActionResult Create()
        {
            var adminName = HttpContext.Session.GetString("AdminName");
            ViewBag.AdminName = adminName;
            return View();
        }

        // POST: Account/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Account_admin account, IFormFile Avatar)
        {
            if (ModelState.IsValid)
            {
                var existingAccount = (await _mongoDBService.GetAccounts())
                    .FirstOrDefault(a => a.Email == account.Email);

                if (existingAccount != null)
                {
                    // If email already exists, add error message and return to view
                    ModelState.AddModelError("Email", "Email này đã được sử dụng. Vui lòng chọn email khác.");
                    return View(account);
                }

                account.Id = Guid.NewGuid().ToString();

                // Handle avatar upload if provided
                if (Avatar != null && Avatar.Length > 0)
                {
                    try
                    {
                        account.Avatar = await _cloudinaryService.UploadImageAsync(Avatar);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading image to Cloudinary.");
                        ModelState.AddModelError("", "Failed to upload image. Please try again.");
                        return View(account);
                    }
                }

                try
                {
                    // Save the new account to MongoDB
                    await _mongoDBService.SaveAccountAsync(account);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving account to MongoDB.");
                    ModelState.AddModelError("", "Could not save account to database. Please try again.");
                    return View(account);
                }

                // Redirect to Index page after successful creation
                return RedirectToAction(nameof(Index));
            }

            return View(account);
        }
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            try
            {
                var account = (await _mongoDBService.GetAccounts()).FirstOrDefault(a => a.Id == id);
                if (account == null)
                    return NotFound();

                return View(account);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching account for editing.");
                return View("Error");
            }
        }

     [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(string id, Account_admin updatedAccount, IFormFile Avatar)
{
    if (id != updatedAccount.Id)
        return BadRequest("Account ID mismatch.");

    if (ModelState.IsValid)
    {
        try
        {
            // Lấy tài khoản từ MongoDB
            var existingAccount = (await _mongoDBService.GetAccounts())
                .FirstOrDefault(a => a.Id == id);

            if (existingAccount == null)
                return NotFound("Account not found.");

            // Kiểm tra nếu email bị trùng lặp (ngoại trừ email của chính tài khoản hiện tại)
            var duplicateEmailAccount = (await _mongoDBService.GetAccounts())
                .FirstOrDefault(a => a.Email == updatedAccount.Email && a.Id != id);
            if (duplicateEmailAccount != null)
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng. Vui lòng chọn email khác.");
                return View(updatedAccount);
            }

            // Cập nhật thông tin cơ bản
            existingAccount.FullName = updatedAccount.FullName;
            existingAccount.Email = updatedAccount.Email;
            existingAccount.Phone = updatedAccount.Phone;
            existingAccount.Status = updatedAccount.Status;
            existingAccount.RoleId = updatedAccount.RoleId;

            // Xử lý upload avatar nếu có
            if (Avatar != null && Avatar.Length > 0)
            {
                try
                {
                    existingAccount.Avatar = await _cloudinaryService.UploadImageAsync(Avatar);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading avatar.");
                    ModelState.AddModelError("", "Failed to upload avatar. Please try again.");
                    return View(updatedAccount);
                }
            }

            // Ghi lại thời gian cập nhật
            existingAccount.UpdatedAt = DateTime.UtcNow;

            // Lưu lại vào MongoDB
            await _mongoDBService.UpdateAccountAsync(existingAccount);

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating account.");
            ModelState.AddModelError("", "Could not update account. Please try again.");
        }
    }

    // Nếu có lỗi, trả lại view với thông tin tài khoản đã chỉnh sửa
    return View(updatedAccount);
}

    }
}


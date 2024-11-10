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
    }
}

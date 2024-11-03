using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using webCore.Models;
using webCore.Services;

namespace webCore.Controllers
{
    public class UserController : Controller
    {
        private readonly CloudinaryService _cloudinaryService;
        private readonly MongoDBService _mongoDBService;

        public UserController(CloudinaryService cloudinaryService, MongoDBService mongoDBService)
        {
            _cloudinaryService = cloudinaryService;
            _mongoDBService = mongoDBService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(User user, IFormFile profileImage)
        {
            // Check if the user has uploaded a new profile image
            if (profileImage != null && profileImage.Length > 0)
            {
                // Upload the image to Cloudinary and get the URL
                user.ProfileImage = await _cloudinaryService.UploadImageAsync(profileImage);
            }

            // Save or update the user in MongoDB
            await _mongoDBService.SaveUserAsync(user);

            // Redirect back to the Index view or display a success message
            return RedirectToAction("Index");
        }
    }
}

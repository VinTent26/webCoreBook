using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using webCore.Models;
using webCore.Services;

namespace webCore.Controllers
{
    public class ProductController : Controller
    {
        private readonly CloudinaryService _cloudinaryService;
        private readonly MongoDBService _mongoDBService;

        public ProductController(CloudinaryService cloudinaryService, MongoDBService mongoDBService)
        {
            _cloudinaryService = cloudinaryService;
            _mongoDBService = mongoDBService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product, IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                ModelState.AddModelError("", "Please upload a valid image file.");
                return View(product);
            }

            // Upload ảnh lên Cloudinary
            var imageUrl = await _cloudinaryService.UploadImageAsync(imageFile);
            if (string.IsNullOrEmpty(imageUrl))
            {
                ModelState.AddModelError("", "Failed to upload image to Cloudinary.");
                return View(product);
            }

            // Gán URL ảnh vào sản phẩm
            product.Image = imageUrl;

            // Lưu sản phẩm vào MongoDB
            await _mongoDBService.SaveProductAsync(product);

            return RedirectToAction("CreateSuccess");
        }

        public IActionResult CreateSuccess()
        {
            return View();
        }
    }
}

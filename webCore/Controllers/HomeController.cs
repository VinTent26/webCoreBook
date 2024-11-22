using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using webCore.Models;
using webCore.Services;

namespace webCore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MongoDBService _mongoDBService;

        public HomeController(ILogger<HomeController> logger, MongoDBService mongoDBService)
        {
            _logger = logger;
            _mongoDBService = mongoDBService;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy danh mục cha từ MongoDB
            var categories = await _mongoDBService.GetCategoriesAsync();
            ViewBag.Categories = categories;
            return View();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using webCore.Services;
using webCore.Models;
using System.Threading.Tasks;

namespace webCore.Controllers
{
    public class DetailUserController : Controller
    {
        private readonly MongoDBService _mongoDBService;

        public DetailUserController(MongoDBService mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }

        // Index action
        public IActionResult Index()
        {
            return View();  // Ensure this view exists (DetailUser/Index.cshtml)
        }

        // Login or get user by email
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _mongoDBService.GetAccountByEmailAsync(email);

            if (user != null && user.Password == password)
            {
                // Successfully authenticated user, pass user data to the view
                return View("Index", user);  // Pass user data to the Index view
            }

            return View("LoginFailed");  // If authentication fails
        }
    }
}

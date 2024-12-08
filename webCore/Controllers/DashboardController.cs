using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using webCore.MongoHelper;

namespace webCore.Controllers
{
    [AuthenticateHelper]
    public class DashboardController : Controller
    {
        private readonly ProductService _productService;
        private readonly OrderService _orderService;

        public DashboardController(ProductService productService, OrderService orderService)
        {
            _productService = productService;
            _orderService = orderService;
        }

        // GET: DashboardController
        public async Task<ActionResult> Index()
        {
            var token = HttpContext.Session.GetString("AdminToken");
            ViewBag.AdminName = HttpContext.Session.GetString("AdminName");
            ViewBag.Token = token;

            // Lấy số liệu thống kê từ service
            var totalProducts = await _productService.GetProductCountAsync();
            var totalOrders = await _orderService.GetTotalOrdersAsync();
            var totalRevenue = await _orderService.GetTotalRevenueAsync();

            // Truyền dữ liệu vào View thông qua ViewBag
            ViewBag.TotalProducts = totalProducts;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalRevenue = totalRevenue;

            return View();
        }

    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using webCore.MongoHelper;
using webCore.Services;

namespace webCore.Controllers
{
    [AuthenticateHelper]
    public class DashboardController : Controller
    {
        private readonly Order_adminService _orderadminService;
        private readonly ProductService _productService;
        private readonly OrderService _orderService;

        public DashboardController(ProductService productService, OrderService orderService, Order_adminService orderadminService)
        {
            _productService = productService;
            _orderService = orderService;
            _orderadminService = orderadminService;
        }

        // GET: DashboardController
        public async Task<ActionResult> Index()
        {
            var token = HttpContext.Session.GetString("AdminToken");
            ViewBag.AdminName = HttpContext.Session.GetString("AdminName");
            ViewBag.Token = token;

            // Lấy số liệu thống kê
            var totalProducts = await _productService.GetProductCountAsync();
            var totalOrders = await _orderService.GetTotalOrdersAsync();
            var totalRevenue = await _orderService.GetTotalRevenueAsync();

            // Lấy 3 đơn hàng gần đây nhất
            var recentOrders = await _orderadminService.GetRecentOrdersAsync();

            // Truyền dữ liệu vào ViewBag
            ViewBag.TotalProducts = totalProducts;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.RecentOrders = recentOrders;

            return View();
        }


    }
}

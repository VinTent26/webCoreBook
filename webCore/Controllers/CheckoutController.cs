using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using webCore.Models;
using webCore.MongoHelper;
using webCore.Services;

namespace webCore.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly CartService _cartService;
        private readonly OrderService _orderService;

        public CheckoutController(CartService cartService, OrderService orderService)
        {
            _cartService = cartService;
            _orderService = orderService;
        }
        [HttpGet]
       
        // Action để hiển thị form nhập thông tin thanh toán
        [HttpGet]
        public async Task<IActionResult> PaymentInfo()
        {
            var userId = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Sign_in", "User");
            }

            // Lấy thông tin giỏ hàng của người dùng
            var cart = await _cartService.GetCartByUserIdAsync(userId);
            if (cart == null || cart.Items.Count == 0)
            {
                return RedirectToAction("Cart");
            }

            // Tính tổng tiền
            decimal totalAmount = cart.Items.Sum(item => item.Price * item.Quantity);
            decimal finalAmount = totalAmount;

            // Truyền dữ liệu vào View
            return View(new PaymentInfoViewModel
            {
                TotalAmount = totalAmount,
                FinalAmount = finalAmount,
            });
        }

        // Action để xử lý thông tin thanh toán và lưu vào MongoDB
        [HttpPost]
        public async Task<IActionResult> ConfirmPayment(PaymentInfoViewModel model)
        {
            var userId = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Sign_in", "User");
            }

            // Lấy thông tin giỏ hàng của người dùng
            var cart = await _cartService.GetCartByUserIdAsync(userId);
            if (cart == null || cart.Items.Count == 0)
            {
                return RedirectToAction("Cart");
            }

            decimal totalAmount = cart.Items.Sum(item => item.Price * item.Quantity);
            decimal finalAmount = totalAmount;

            // Lưu đơn hàng vào MongoDB với trạng thái "pending"
            var order = new Order
            {
                UserId = userId,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                Items = cart.Items,
                TotalAmount = totalAmount,
                FinalAmount = finalAmount,
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };

            await _orderService.SaveOrderAsync(order);

            // Chuyển hướng đến trang đơn hàng chờ vận chuyển
            return RedirectToAction("OrderPending", "Order");
        }

    }
}

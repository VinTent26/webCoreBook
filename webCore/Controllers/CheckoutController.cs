using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        private readonly VoucherClientService _voucherClientService;
        private readonly CategoryProduct_adminService _categoryProductAdminService;

        public CheckoutController(CartService cartService, OrderService orderService, VoucherClientService voucherClientService, CategoryProduct_adminService categoryProduct_AdminService)
        {
            _cartService = cartService;
            _orderService = orderService;
            _voucherClientService = voucherClientService;
            _categoryProductAdminService = categoryProduct_AdminService;
        }
        [HttpGet]
       
        // Action để hiển thị form nhập thông tin thanh toán
        [HttpGet]
        public async Task<IActionResult> PaymentInfo()
        {
            var isLoggedIn = HttpContext.Session.GetString("UserToken") != null;

            // Truyền thông tin vào ViewBag hoặc Model để sử dụng trong View
            ViewBag.IsLoggedIn = isLoggedIn;
            // Lấy UserId từ session
            var userId = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(userId))
            {
                // Nếu chưa đăng nhập, chuyển hướng đến trang đăng nhập
                return RedirectToAction("Sign_in", "User");
            }

            // Lấy thông tin giỏ hàng của người dùng
            var cart = await _cartService.GetCartByUserIdAsync(userId);
            if (cart == null || cart.Items.Count == 0)
            {
                return RedirectToAction("Cart");
            }
            // Lấy danh sách các sản phẩm đã chọn từ session
            var selectedProductIds = JsonConvert.DeserializeObject<List<string>>(HttpContext.Session.GetString("SelectedProductIds") ?? "[]");

            // Lọc các sản phẩm đã chọn trong giỏ hàng
            var selectedItems = cart.Items.Where(item => selectedProductIds.Contains(item.ProductId)).ToList();

            // Tính tổng tiền
            decimal totalAmount = selectedItems.Sum(item => (item.Price * (1 - item.DiscountPercentage / 100)) * item.Quantity);
            // Lấy voucher giảm giá
            var voucherDiscount = HttpContext.Session.GetString("SelectedVoucher");
            decimal discountAmount = 0;

            if (!string.IsNullOrEmpty(voucherDiscount))
            {
                decimal discountValue = decimal.Parse(voucherDiscount);
                discountAmount = totalAmount * (discountValue / 100);
            }

            decimal finalAmount = totalAmount - discountAmount;

            // Truyền dữ liệu vào View
            return View(new PaymentInfoViewModel
            {
                Items = selectedItems,
                TotalAmount = totalAmount,
                DiscountAmount = discountAmount,
                FinalAmount = finalAmount,
            });
        }

        // Action để xử lý thông tin thanh toán và lưu vào MongoDB
        [HttpPost]
        public async Task<IActionResult> ConfirmPayment(PaymentInfoViewModel model)
        {
            var isLoggedIn = HttpContext.Session.GetString("UserToken") != null;

            // Truyền thông tin vào ViewBag hoặc Model để sử dụng trong View
            ViewBag.IsLoggedIn = isLoggedIn;

            // Lấy UserId từ session
            var userId = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(userId))
            {
                // Nếu chưa đăng nhập, chuyển hướng đến trang đăng nhập
                return RedirectToAction("Sign_in", "User");
            }

            // Lấy thông tin giỏ hàng của người dùng
            var cart = await _cartService.GetCartByUserIdAsync(userId);
            if (cart == null || cart.Items.Count == 0)
            {
                return RedirectToAction("Cart");
            }

            var selectedProductIds = JsonConvert.DeserializeObject<List<string>>(HttpContext.Session.GetString("SelectedProductIds") ?? "[]");

            // Lọc các sản phẩm đã chọn trong giỏ hàng
            var selectedItems = cart.Items.Where(item => selectedProductIds.Contains(item.ProductId)).ToList();

            // Tính toán lại tổng tiền
            decimal totalAmount = selectedItems.Sum(item => (item.Price * (1 - item.DiscountPercentage / 100)) * item.Quantity);

            // Lấy voucher giảm giá
            var voucherDiscount = HttpContext.Session.GetString("SelectedVoucher");
            decimal discountAmount = 0;

            string voucherId = HttpContext.Session.GetString("SelectedVoucherId"); // Lấy voucherId từ session
            if (!string.IsNullOrEmpty(voucherDiscount) && !string.IsNullOrEmpty(voucherId))
            {
                decimal discountValue = decimal.Parse(voucherDiscount);
                discountAmount = totalAmount * (discountValue / 100);

                // Tìm voucher trong cơ sở dữ liệu và cập nhật UsageCount
                var voucher = await _voucherClientService.GetVoucherByIdAsync(voucherId); // Giả sử có phương thức GetVoucherByIdAsync
                if (voucher != null)
                {
                    voucher.UsageCount += 1;  // Tăng UsageCount lên 1
                    await _voucherClientService.UpdateVoucherUsageCountAsync(voucher);  // Cập nhật voucher
                }
            }

            decimal finalAmount = totalAmount - discountAmount;

            // Lưu đơn hàng vào MongoDB với trạng thái "pending"
            var order = new Order
            {
                UserId = userId,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                Items = selectedItems,
                TotalAmount = totalAmount,
                DiscountAmount = discountAmount,
                FinalAmount = finalAmount,
                Status = "Đang chờ duyệt",
                CreatedAt = DateTime.UtcNow
            };

            await _orderService.SaveOrderAsync(order);
            await _cartService.RemoveItemsFromCartAsync(userId, selectedProductIds);
            foreach (var item in selectedItems)
            {
                var product = await _categoryProductAdminService.GetProductByIdAsync(item.ProductId);
                if (product != null)
                {
                    product.Stock -= item.Quantity;  // Trừ đi số lượng sản phẩm
                    await _categoryProductAdminService.UpdateProductAsync(product);  // Cập nhật lại sản phẩm trong MongoDB
                }
            }
            // Chuyển hướng đến trang đơn hàng chờ vận chuyển
            return RedirectToAction("PaymentHistory", "Checkout");
        }

        [HttpGet]
        public async Task<IActionResult> PaymentHistory()
        {
            var isLoggedIn = HttpContext.Session.GetString("UserToken") != null;

            // Truyền thông tin vào ViewBag hoặc Model để sử dụng trong View
            ViewBag.IsLoggedIn = isLoggedIn;
            // Lấy UserId từ session
            var userId = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(userId))
            {
                // Nếu chưa đăng nhập, chuyển hướng đến trang đăng nhập
                return RedirectToAction("Sign_in", "User");
            }

            // Lấy danh sách đơn hàng từ MongoDB theo UserId
            var orders = await _orderService.GetOrdersByUserIdAsync(userId);

            // Truyền dữ liệu vào View
            return View(orders);
        }
        [HttpGet]
        public async Task<IActionResult> OrderDetails(string orderId)
        {
            var isLoggedIn = HttpContext.Session.GetString("UserToken") != null;

            // Kiểm tra đăng nhập
            if (!isLoggedIn)
            {
                return RedirectToAction("Sign_in", "User");
            }

            // Tìm đơn hàng theo ID
            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
            {
                // Xử lý nếu không tìm thấy đơn hàng
                return NotFound("Không tìm thấy đơn hàng");
            }

            return View(order);
        }

    }
}

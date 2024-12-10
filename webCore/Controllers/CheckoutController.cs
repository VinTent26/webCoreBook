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
       

        // Action để hiển thị form nhập thông tin thanh toán
        [HttpGet]
        public async Task<IActionResult> PaymentInfo()
        {
            // Kiểm tra trạng thái đăng nhập từ session
            var isLoggedIn = HttpContext.Session.GetString("UserToken") != null;
            ViewBag.IsLoggedIn = isLoggedIn;

            // Lấy UserId từ session
            var userId = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(userId))
            {
                // Nếu chưa đăng nhập, chuyển hướng đến trang đăng nhập
                return RedirectToAction("Sign_in", "User");
            }

            // Kiểm tra xem có CartItem được lưu trong session (dành cho BuyNow)
            var cartItemSession = HttpContext.Session.GetString("CartItem");

            if (!string.IsNullOrEmpty(cartItemSession))
            {
                // Trường hợp BuyNow: xử lý với một sản phẩm cụ thể
                var cartItem = JsonConvert.DeserializeObject<CartItem>(cartItemSession);

                // Tính toán giá trị
                decimal totalAmount = cartItem.Price * cartItem.Quantity * (1 - cartItem.DiscountPercentage / 100);
                decimal finalAmount = totalAmount;

                // Trả về View với dữ liệu cho sản phẩm BuyNow
                return View(new PaymentInfoViewModel
                {
                    Items = new List<CartItem> { cartItem },
                    TotalAmount = totalAmount,
                    FinalAmount = finalAmount
                });
            }

            // Nếu không có CartItem trong session, xử lý như Checkout (toàn bộ giỏ hàng)
            var cart = await _cartService.GetCartByUserIdAsync(userId);
            if (cart == null || cart.Items == null || cart.Items.Count == 0)
            {
                // Nếu giỏ hàng rỗng, chuyển về trang giỏ hàng
                return RedirectToAction("Cart");
            }

            // Lấy danh sách các sản phẩm đã chọn từ session
            var selectedProductIds = JsonConvert.DeserializeObject<List<string>>(HttpContext.Session.GetString("SelectedProductIds") ?? "[]");
            var selectedItems = cart.Items.Where(item => selectedProductIds.Contains(item.ProductId)).ToList();

            // Tính toán tổng tiền
            decimal totalAmountCheckout = selectedItems.Sum(item => (item.Price * (1 - item.DiscountPercentage / 100)) * item.Quantity);
            decimal finalAmountCheckout = totalAmountCheckout;

            // Trả về View với dữ liệu cho giỏ hàng
            return View(new PaymentInfoViewModel
            {
                Items = selectedItems,
                TotalAmount = totalAmountCheckout,
                FinalAmount = finalAmountCheckout
            });
        }


        // Action để xử lý thông tin thanh toán và lưu vào MongoDB
        [HttpPost]
        public async Task<IActionResult> ConfirmPayment(PaymentInfoViewModel model)
        {
            var isLoggedIn = HttpContext.Session.GetString("UserToken") != null;
            ViewBag.IsLoggedIn = isLoggedIn;

            // Lấy UserId từ session
            var userId = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(userId))
            {
                // Nếu chưa đăng nhập, chuyển hướng đến trang đăng nhập
                return RedirectToAction("Sign_in", "User");
            }

            // Kiểm tra xem có CartItem được lưu trong session (BuyNow)
            var cartItemSession = HttpContext.Session.GetString("CartItem");

            if (!string.IsNullOrEmpty(cartItemSession))
            {
                // Trường hợp BuyNow: xử lý với một sản phẩm cụ thể
                var cartItem = JsonConvert.DeserializeObject<CartItem>(cartItemSession);

                // Tính toán giá trị
                decimal totalAmount = cartItem.Price * cartItem.Quantity * (1 - cartItem.DiscountPercentage / 100);
                decimal finalAmount = totalAmount;

                // Lưu đơn hàng vào MongoDB với trạng thái "pending"
                var buyNowOrder = new Order
                {
                    UserId = userId,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    Items = new List<CartItem> { cartItem },
                    TotalAmount = totalAmount,
                    DiscountAmount = 0, // Không áp dụng voucher cho BuyNow
                    FinalAmount = finalAmount,
                    Status = "Đang chờ duyệt",
                    CreatedAt = DateTime.UtcNow
                };

                await _orderService.SaveOrderAsync(buyNowOrder);

                // Xóa dữ liệu BuyNow khỏi session
                HttpContext.Session.Remove("CartItem");

                // Chuyển hướng đến trang lịch sử thanh toán
                return RedirectToAction("PaymentHistory", "Checkout");
            }

            // Trường hợp Checkout (giỏ hàng)
            var cart = await _cartService.GetCartByUserIdAsync(userId);
            if (cart == null || cart.Items == null || cart.Items.Count == 0)
            {
                return RedirectToAction("Cart");
            }

            // Lấy danh sách sản phẩm đã chọn từ session
            var selectedProductIds = JsonConvert.DeserializeObject<List<string>>(HttpContext.Session.GetString("SelectedProductIds") ?? "[]");
            var selectedItems = cart.Items.Where(item => selectedProductIds.Contains(item.ProductId)).ToList();

            if (!selectedItems.Any())
            {
                return RedirectToAction("Cart");
            }

            // Tính toán tổng tiền
            decimal totalAmountCheckout = selectedItems.Sum(item => (item.Price * (1 - item.DiscountPercentage / 100)) * item.Quantity);

            // Tính toán giảm giá từ voucher
            var voucherDiscount = HttpContext.Session.GetString("SelectedVoucher");
            decimal discountAmount = 0;

            string voucherId = HttpContext.Session.GetString("SelectedVoucherId"); // Lấy voucherId từ session
            if (!string.IsNullOrEmpty(voucherDiscount) && !string.IsNullOrEmpty(voucherId))
            {
                decimal discountValue = decimal.Parse(voucherDiscount);
                discountAmount = totalAmountCheckout * (discountValue / 100);

                // Tìm voucher trong cơ sở dữ liệu và cập nhật UsageCount
                var voucher = await _voucherClientService.GetVoucherByIdAsync(voucherId); // Giả sử có phương thức GetVoucherByIdAsync
                if (voucher != null)
                {
                    voucher.UsageCount += 1;  // Tăng UsageCount lên 1
                    await _voucherClientService.UpdateVoucherUsageCountAsync(voucher);  // Cập nhật voucher
                }
            }

            decimal finalAmountCheckout = totalAmountCheckout - discountAmount;

            // Lưu đơn hàng vào MongoDB với trạng thái "pending"
            var checkoutOrder = new Order
            {
                UserId = userId,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                Items = selectedItems,
                TotalAmount = totalAmountCheckout,
                DiscountAmount = discountAmount,
                FinalAmount = finalAmountCheckout,
                Status = "Đang chờ duyệt",
                CreatedAt = DateTime.UtcNow
            };

            await _orderService.SaveOrderAsync(checkoutOrder);
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

            // Xóa dữ liệu liên quan khỏi session
            HttpContext.Session.Remove("SelectedProductIds");
            HttpContext.Session.Remove("SelectedVoucher");

            // Chuyển hướng đến trang lịch sử thanh toán
            return RedirectToAction("PaymentHistory", "Checkout");
        }

        [HttpGet]
        public async Task<IActionResult> PaymentHistory(string ? status = null)
        {
            // Kiểm tra xem người dùng đã đăng nhập hay chưa
            var isLoggedIn = HttpContext.Session.GetString("UserToken") != null;
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

            // Lọc danh sách đơn hàng theo trạng thái nếu trạng thái không null
            if (!string.IsNullOrEmpty(status))
            {
                if (status == "Đang chờ duyệt")
                {
                    orders = orders.Where(o => o.Status == "Đang chờ duyệt").ToList();
                }
                else if (status == "Đã duyệt")
                {
                    orders = orders.Where(o => o.Status == "Đã duyệt").ToList();
                }
                else if (status == "Đã hủy")
                {
                    orders = orders.Where(o => o.Status == "Đã hủy").ToList();
                }
            }

            // Truyền trạng thái hiện tại và danh sách đơn hàng vào View
            ViewBag.CurrentStatus = status ?? "All";
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

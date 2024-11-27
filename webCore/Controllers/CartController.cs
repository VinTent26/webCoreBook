using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webCore.Models;
using webCore.MongoHelper;
using webCore.Services;

namespace webCore.Controllers
{
    public class CartController : Controller
    {
        private readonly CartService _cartService;

        public CartController(CartService cartService)
        {
            _cartService = cartService;
        }

        // Thêm sản phẩm vào giỏ hàng
        [HttpPost]
        public async Task<IActionResult> AddToCart(string productId, string title, decimal price,decimal discountpercentage, int quantity, string image)
        {
            // Lấy UserId từ session
            var userId = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(userId))
            {
                // Nếu chưa đăng nhập, trả về thông báo lỗi
                return Json(new { success = false, message = "Bạn cần đăng nhập để thêm vào giỏ hàng." });
            }

            // Lấy giỏ hàng từ dịch vụ hoặc tạo mới nếu chưa tồn tại
            var cart = await _cartService.GetCartByUserIdAsync(userId) ?? new Cart { UserId = userId };
            var existingItem = cart.Items.FirstOrDefault(item => item.ProductId == productId);

            if (existingItem != null)
            {
                // Nếu sản phẩm đã có trong giỏ, tăng số lượng
                existingItem.Quantity += quantity;
            }
            else
            {
                // Nếu chưa có, thêm mới sản phẩm vào giỏ
                cart.Items.Add(new CartItem
                {
                    ProductId = productId,
                    Title = title,
                    Price = price,
                    DiscountPercentage = discountpercentage,
                    Quantity = quantity,
                    Image = image
                });
            }

            // Cập nhật thời gian sửa đổi giỏ hàng
            cart.UpdatedAt = DateTime.UtcNow;

            // Lưu giỏ hàng vào MongoDB
            await _cartService.AddOrUpdateCartAsync(cart);

            // Trả về kết quả thành công
            return Json(new { success = true, message = "Sản phẩm đã được thêm vào giỏ hàng!" });
        }

        // Hiển thị giỏ hàng của người dùng
        [HttpGet]
        public async Task<IActionResult> Cart()
        {
            // Lấy UserId từ session
            var userId = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(userId))
            {
                // Nếu chưa đăng nhập, chuyển hướng đến trang đăng nhập
                return RedirectToAction("Sign_in", "User");
            }

            // Lấy giỏ hàng của người dùng từ dịch vụ
            var cart = await _cartService.GetCartByUserIdAsync(userId);

            // Kiểm tra nếu voucher đã được áp dụng, lấy thông tin từ session
            var selectedVoucher = HttpContext.Session.GetString("SelectedVoucher");
            if (!string.IsNullOrEmpty(selectedVoucher))
            {
                var voucherInfo = selectedVoucher.Split(',');

                // Lấy discount và discountType từ session
                var discount = voucherInfo[0];
                var discountType = voucherInfo[1];

                // Lưu thông tin voucher vào ViewData
                ViewData["VoucherDiscount"] = discount;
                ViewData["VoucherDiscountType"] = discountType;
            }

            // Trả về view với danh sách sản phẩm trong giỏ, hoặc danh sách trống nếu giỏ hàng null
            return View(cart?.Items ?? new List<CartItem>());
        }
        [HttpPost]
        public async Task<IActionResult> DeleteProduct(string productId)
        {
            // Lấy UserId từ session
            var userId = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(userId))
            {
                // Nếu chưa đăng nhập, trả về thông báo lỗi
                return Json(new { success = false, message = "Bạn cần đăng nhập để xóa sản phẩm." });
            }

            // Lấy giỏ hàng của người dùng
            var cart = await _cartService.GetCartByUserIdAsync(userId);
            if (cart == null)
            {
                return Json(new { success = false, message = "Giỏ hàng không tồn tại." });
            }

            // Tìm và xóa sản phẩm trong giỏ hàng
            var itemToRemove = cart.Items.FirstOrDefault(item => item.ProductId == productId);
            if (itemToRemove != null)
            {
                cart.Items.Remove(itemToRemove);
            }
            else
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại trong giỏ hàng." });
            }

            // Cập nhật giỏ hàng vào MongoDB
            await _cartService.AddOrUpdateCartAsync(cart);

            // Trả về kết quả thành công
            return Json(new { success = true, message = "Sản phẩm đã được xóa khỏi giỏ hàng." });
        }
    }
}

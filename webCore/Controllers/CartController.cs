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
    public class CartController : Controller
    {
        private readonly CartService _cartService;
        private readonly VoucherClientService _voucherService;

        public CartController(CartService cartService, VoucherClientService voucherService)
        {
            _cartService = cartService;
            _voucherService = voucherService;
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

            // Kiểm tra xem giỏ hàng có tồn tại và có sản phẩm hay không
            if (cart == null || cart.Items == null || cart.Items.Count == 0)
            {
                // Nếu giỏ hàng rỗng, trả về view với danh sách trống
                return View(new List<CartItem>());
            }

            // Lấy thông tin voucher từ session
            var voucherDiscount = HttpContext.Session.GetString("SelectedVoucher");

            // Lấy danh sách các sản phẩm đã chọn từ session (List<string>)
            var selectedProductIds = JsonConvert.DeserializeObject<List<string>>(HttpContext.Session.GetString("SelectedProductIds") ?? "[]");

            // Lọc ra các sản phẩm đã chọn trong giỏ hàng để tính tổng tiền
            var selectedItems = cart.Items.Where(item => selectedProductIds.Contains(item.ProductId.ToString())).ToList();

            // Tính toán tổng tiền cho các sản phẩm đã chọn
            decimal totalAmount = selectedItems.Sum(item => item.Price * item.Quantity);
            decimal discountAmount = 0;

            if (!string.IsNullOrEmpty(voucherDiscount))
            {
                decimal discountValue = decimal.Parse(voucherDiscount);
                discountAmount = totalAmount * (discountValue / 100);
            }

            decimal finalAmount = totalAmount - discountAmount;

            // Cập nhật các giá trị cần hiển thị vào ViewData
            ViewData["VoucherDiscount"] = voucherDiscount;  // Voucher giảm giá
            ViewData["TotalAmount"] = totalAmount;           // Tổng tiền trước giảm giá
            ViewData["FinalAmount"] = finalAmount;           // Tổng tiền sau giảm giá
            ViewData["SelectedProductIds"] = selectedProductIds; // Danh sách các sản phẩm đã chọn

            // Trả về danh sách tất cả các sản phẩm trong giỏ hàng
            return View(cart.Items); // Trả về tất cả các sản phẩm trong giỏ
        }


        [HttpPost]
        public IActionResult SaveSelectedProducts([FromBody] List<string> selectedProductIds)
        {
            // Kiểm tra xem dữ liệu có được nhận hay không
            if (selectedProductIds == null || !selectedProductIds.Any())
            {
                // Nếu không có sản phẩm nào được chọn, có thể ghi log hoặc trả về lỗi
                return Json(new { success = false, message = "No products selected" });
            }

            // In ra danh sách các sản phẩm đã chọn (logging)
            Console.WriteLine("Products selected: " + string.Join(", ", selectedProductIds));

            // Lưu danh sách sản phẩm đã chọn vào session
            HttpContext.Session.SetString("SelectedProductIds", JsonConvert.SerializeObject(selectedProductIds));

            // Trả về JSON xác nhận thành công
            return Json(new { success = true });
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
        // Hàm cập nhật số lượng sản phẩm trong giỏ hàng
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(string productId, int quantity)
        {
            // Lấy UserId từ session
            var userId = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(userId))
            {
                // Nếu chưa đăng nhập, trả về thông báo lỗi
                return Json(new { success = false, message = "Bạn cần đăng nhập để cập nhật số lượng." });
            }

            // Lấy giỏ hàng của người dùng
            var cart = await _cartService.GetCartByUserIdAsync(userId);
            if (cart == null)
            {
                return Json(new { success = false, message = "Giỏ hàng không tồn tại." });
            }

            // Tìm sản phẩm trong giỏ hàng
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                // Cập nhật số lượng
                item.Quantity = quantity;
                cart.UpdatedAt = DateTime.UtcNow;

                // Lưu giỏ hàng cập nhật vào MongoDB
                await _cartService.AddOrUpdateCartAsync(cart);

                // Trả về kết quả thành công
                return Json(new { success = true, message = "Số lượng sản phẩm đã được cập nhật." });
            }

            return Json(new { success = false, message = "Sản phẩm không tồn tại trong giỏ hàng." });
        }
    }
}

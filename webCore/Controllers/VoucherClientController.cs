using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using webCore.Models;
using webCore.MongoHelper;
using webCore.Services;

namespace webCore.Controllers
{
    public class VoucherClientController : Controller
    {
        private readonly VoucherClientService _voucherService;

        // Constructor để inject VoucherClientService
        public VoucherClientController(VoucherClientService voucherService)
        {
            _voucherService = voucherService;
        }

        // Phương thức hiển thị danh sách các voucher
        public async Task<IActionResult> VoucherClient()
        {
            var userId = HttpContext.Session.GetString("UserToken");

            // Kiểm tra nếu chưa đăng nhập, điều hướng về trang đăng nhập
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Sign_in", "User");
            }

            // Lấy tất cả voucher có trạng thái 'IsActive' là true
            var vouchers = await _voucherService.GetActiveVouchersAsync();

            // Trả về view với danh sách voucher
            return View(vouchers);
        }

        // API để lưu voucher vào session khi người dùng áp dụng
        [HttpPost]
        public IActionResult ApplyVoucher(string discount, string discountType)
        {
            if (string.IsNullOrEmpty(discount) || string.IsNullOrEmpty(discountType))
            {
                return Json(new { success = false, message = "Không có voucher hợp lệ." });
            }

            // Lưu thông tin voucher vào session hoặc database
            HttpContext.Session.SetString("SelectedVoucher", $"{discount},{discountType}");

            // Trả về thông tin thành công
            return Json(new { success = true });
        }
    }
}

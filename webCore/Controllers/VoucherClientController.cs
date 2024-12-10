using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System;
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
            var voucherDiscount = HttpContext.Session.GetString("SelectedVoucher");

            // Kiểm tra nếu chưa đăng nhập, điều hướng về trang đăng nhập
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Sign_in", "User");
            }

            // lấy tất cả voucher có trạng thái 'isActive' là true
            var vouchers = await _voucherService.GetActiveVouchersAsync();

            // Trả về view với danh sách voucher
            return View(vouchers);
        }

        [HttpPost]
        public IActionResult ApplyVoucher(string discount, string voucherId)
        {
            // Kiểm tra voucherId có hợp lệ không (đảm bảo nó là một ObjectId hợp lệ)
            ObjectId parsedVoucherId;
            if (!ObjectId.TryParse(voucherId, out parsedVoucherId))
            {
                // Nếu voucherId không hợp lệ, trả về thông báo lỗi
                return Json(new { success = false, message = "Voucher ID không hợp lệ." });
            }

            // Lưu thông tin voucher vào session
            HttpContext.Session.SetString("SelectedVoucher", discount);  // Lưu discount vào session
            HttpContext.Session.SetString("SelectedVoucherId", voucherId);  // Lưu voucherId (dạng chuỗi) vào session

            // Trả về JSON xác nhận thành công
            return Json(new { success = true });
        }


    }
}

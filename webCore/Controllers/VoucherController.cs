using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.Threading.Tasks;
using webCore.Models;
using webCore.MongoHelper;
using webCore.Services;

namespace webCore.Controllers
{
    public class VoucherController : Controller
    {
        private readonly MongoDBService _mongoDBService;
        private readonly VoucherService _voucherService;

        public VoucherController(VoucherService voucherService)
        {
            _voucherService = voucherService;
        }
        public async Task<IActionResult> Index()
        {
            // Fetch admin name from session
            var adminName = HttpContext.Session.GetString("AdminName");
            ViewBag.AdminName = adminName;
            // Fetch accounts asynchronously from MongoDB
            var vouchers = await _voucherService.GetVouchers();
            return View(vouchers); // Pass the accounts to the view
          
        }

        // GET: VoucherController/Create - Giao diện tạo voucher cho Admin
        public IActionResult Create()
        {
            var adminName = HttpContext.Session.GetString("AdminName");
            ViewBag.AdminName = adminName;
            return View();
        }

        // POST: VoucherController/Create - Xử lý tạo voucher từ Admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Voucher voucher)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra ngày bắt đầu và ngày kết thúc hợp lệ
                if (voucher.StartDate <= voucher.EndDate)
                {
                    voucher.UsageCount = 0; // Đặt số lần sử dụng ban đầu là 0
                    await _voucherService.CreateVoucherAsync(voucher);
                    TempData["Message"] = "Voucher đã được tạo thành công!";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Ngày bắt đầu phải trước ngày kết thúc.");
                }
            }

            return View(voucher);
        }
        // GET: VoucherController/Edit/{id}
        public async Task<IActionResult> Edit(string id)
        {
            var voucher = await _voucherService.GetVoucherByIdAsync(id);
            if (voucher == null)
            {
                return NotFound();
            }

            var adminName = HttpContext.Session.GetString("AdminName");
            ViewBag.AdminName = adminName;

            return View(voucher);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Voucher voucher)
        {
            if (ModelState.IsValid)
            {
                if (voucher.StartDate <= voucher.EndDate)
                {
                    var result = await _voucherService.UpdateVoucherAsync(id, voucher);
                    if (result)
                    {
                        TempData["Message"] = "Voucher đã được cập nhật thành công!";
                        return RedirectToAction("Index");
                    }
                    ModelState.AddModelError("", "Không thể cập nhật voucher.");
                }
                else
                {
                    ModelState.AddModelError("", "Ngày bắt đầu phải trước ngày kết thúc.");
                }
            }

            return View(voucher);
        }

        // GET: VoucherController/Delete/{id}
        public async Task<IActionResult> Delete(string id)
        {
            var voucher = await _voucherService.GetVoucherByIdAsync(id);
            if (voucher == null)
            {
                return NotFound();
            }

            var adminName = HttpContext.Session.GetString("AdminName");
            ViewBag.AdminName = adminName;

            return View(voucher);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var result = await _voucherService.DeleteVoucherAsync(id);

            if (result)
            {
                TempData["Message"] = "Voucher đã được xóa thành công!";
                // Redirect back to the Index page after successful deletion
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Không thể xóa voucher.");
            // In case of error, redirect back to the Index page to show an error message
            return RedirectToAction("Index");
        }


    }
}


using Microsoft.AspNetCore.Mvc;
using webCore.Services;
using webCore.Models;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Linq;
using webCore.MongoHelper;

namespace webCore.Controllers
{
    public class Admin_userController : Controller
    {
        private readonly User_adminService _useradminService;


        public Admin_userController(User_adminService useadminService)
        {
            _useradminService = useadminService;
        }

        // Hiển thị danh sách tất cả đơn hàng
        public async Task<IActionResult> Index()
        {
            try
            {
                var adminName = HttpContext.Session.GetString("AdminName");
                ViewBag.AdminName = adminName;
                // Lấy tất cả người dùng từ service
                var user = await _useradminService.GetAllUsersAsync();

                // Kiểm tra nếu không có người dùng nào
                if (user == null || user.Count == 0)
                {
                    TempData["ErrorMessage"] = "Không có người dùng nào để hiển thị.";
                    return View(new List<User>()); // Trả về danh sách trống nếu không có người dùng
                }

                // Nếu có người dùng, trả về view với danh sách người dùng
                return View(user);
            }
            catch (Exception ex)
            {
                // Xử lý khi có lỗi xảy ra
                TempData["ErrorMessage"] = "Lỗi khi tải . " + ex.Message;
                return RedirectToAction("Error"); // Chuyển hướng đến trang lỗi
            }
        }
        [HttpGet]
        public async Task<IActionResult> Detail(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound(); // Nếu id không hợp lệ, trả về lỗi 404
            }

            try
            {
                var adminName = HttpContext.Session.GetString("AdminName");
                ViewBag.AdminName = adminName;
                var user = await _useradminService.GetUserByIdAsync(id); // Gọi service để lấy thông tin đơn hàng
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Đơn hàng không tồn tại.";
                    return RedirectToAction("Index"); // Quay lại trang danh sách nếu đơn hàng không tìm thấy
                }

                return View(user);  // Trả về View chi tiết đơn hàng
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải chi tiết đơn hàng: " + ex.Message;
                return RedirectToAction("Error");
            }
        }
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            try
            {
                // Kiểm tra nếu id rỗng hoặc null
                if (string.IsNullOrEmpty(id))
                {
                    TempData["ErrorMessage"] = "ID không hợp lệ.";
                    return RedirectToAction("Index"); // Redirect đến trang Index
                }

                // Lấy người dùng theo ID
                var user = await _useradminService.GetUserByIdAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Người dùng không tồn tại.";
                    return RedirectToAction("Index"); // Redirect đến trang Index
                }

                // Xác định trạng thái mới: nếu đang là 1 thì đổi thành 0
                var newStatus = (user.Status == 1) ? 0 : 1;

                // Cập nhật trạng thái trong MongoDB
                var isUpdated = await _useradminService.UpdateUserStatusAsync(id, newStatus);

                if (isUpdated)
                {
                    TempData["SuccessMessage"] = "Trạng thái người dùng đã được cập nhật thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể cập nhật trạng thái.";
                }

                // Tải lại trang Index
                return RedirectToAction("Index"); // Redirect đến trang Index để tải lại dữ liệu
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi: " + ex.Message;
                return RedirectToAction("Index"); // Redirect đến trang Index nếu có lỗi
            }
        }


    }
}

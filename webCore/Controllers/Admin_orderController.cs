using Microsoft.AspNetCore.Mvc;
using webCore.Services;
using webCore.Models;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace webCore.Controllers
{
    public class Admin_orderController : Controller
    {
        private readonly Order_adminService _orderService;

        // Inject dịch vụ Order_adminService vào controller
        public Admin_orderController(Order_adminService orderService)
        {
            _orderService = orderService;
        }

        // Hiển thị danh sách tất cả đơn hàng
        public async Task<IActionResult> Index(string? status = null)
        {
            try
            {
                var adminName = HttpContext.Session.GetString("AdminName");
                ViewBag.AdminName = adminName;
                // Lấy tất cả đơn hàng từ service
                var orders = await _orderService.GetAllOrdersAsync();

                // Kiểm tra nếu không có đơn hàng nào
                if (orders == null || orders.Count == 0)
                {
                    TempData["ErrorMessage"] = "Không có đơn hàng nào để hiển thị.";
                    return View(new List<Order>()); // Trả về danh sách trống nếu không có đơn hàng
                }
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
                // Nếu có đơn hàng, trả về view với danh sách đơn hàng
                return View(orders);
            }
            catch (Exception ex)
            {
                // Xử lý khi có lỗi xảy ra
                TempData["ErrorMessage"] = "Lỗi khi tải danh sách đơn hàng. " + ex.Message;
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
                var order = await _orderService.GetOrderByIdAsync(id); // Gọi service để lấy thông tin đơn hàng
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Đơn hàng không tồn tại.";
                    return RedirectToAction("Index"); // Quay lại trang danh sách nếu đơn hàng không tìm thấy
                }

                return View(order);  // Trả về View chi tiết đơn hàng
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải chi tiết đơn hàng: " + ex.Message;
                return RedirectToAction("Error");
            }
        }


        [HttpPost]
        public async Task<IActionResult> UpdateStatus(string orderId, string newStatus)
        {
            if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(newStatus))
            {
                TempData["ErrorMessage"] = "Thông tin cập nhật không hợp lệ.";
                return RedirectToAction("Index"); // Quay lại trang danh sách đơn hàng
            }

            try
            {
                // Gọi phương thức cập nhật trạng thái đơn hàng
                var isUpdated = await _orderService.UpdateOrderStatusAsync(orderId, newStatus);

                if (isUpdated)
                {
                    TempData["SuccessMessage"] = "Trạng thái đơn hàng đã được cập nhật thành công.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Cập nhật trạng thái không thành công.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi cập nhật trạng thái: " + ex.Message;
            }

            // Trở về trang danh sách đơn hàng
            return RedirectToAction("Index");
        }


        // Trang lỗi chung
        public IActionResult Error()
        {
            return View(); // Hiển thị trang lỗi chung nếu có vấn đề
        }
    }
}

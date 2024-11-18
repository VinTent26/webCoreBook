using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using webCore.MongoHelper;
using webCore.Models;

namespace webCore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ForgotPasswordController : Controller
    {
        private readonly ForgotPasswordService _forgotPasswordService;

        public ForgotPasswordController(ForgotPasswordService forgotPasswordService)
        {
            _forgotPasswordService = forgotPasswordService;
        }

        // Giao diện nhập email để gửi OTP
        [HttpGet("send-otp-view")]
        public IActionResult SendOtpView()
        {
            return View("SendOtp");
        }

        // API gửi OTP qua email
        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOTP([FromForm] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Email không được để trống.";
                return View("SendOtp");
            }

            try
            {
                await _forgotPasswordService.GenerateAndSendOTP(email);
                ViewBag.Message = "OTP đã được gửi tới email của bạn.";
                return View("VerifyOtp");
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Lỗi: {ex.Message}";
                return View("SendOtp");
            }
        }

        // Giao diện xác minh OTP
        [HttpGet("verify-otp-view")]
        public IActionResult VerifyOtpView()
        {
            return View("VerifyOtp");
        }

        // API xác minh OTP
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOTP([FromForm] string email, [FromForm] string otp)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otp))
            {
                ViewBag.Error = "Email và OTP không được để trống.";
                return View("VerifyOtp");
            }

            var isVerified = await _forgotPasswordService.VerifyOTP(email, otp);
            if (!isVerified)
            {
                ViewBag.Error = "OTP không hợp lệ hoặc đã hết hạn.";
                return View("VerifyOtp");
            }

            ViewBag.Message = "OTP xác thực thành công. Bạn có thể đặt lại mật khẩu.";
            return View("ResetPassword");
        }

        // Giao diện đặt lại mật khẩu
        [HttpGet("reset-password-view")]
        public IActionResult ResetPasswordView()
        {
            return View("ResetPassword");
        }

        // API đặt lại mật khẩu
        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromForm] string email, [FromForm] string newPassword)
        {
            // Ở đây bạn cần tích hợp logic cập nhật mật khẩu vào cơ sở dữ liệu
            // Tạm thời chỉ trả về thông báo thành công
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(newPassword))
            {
                ViewBag.Error = "Email và mật khẩu không được để trống.";
                return View("ResetPassword");
            }

            ViewBag.Message = "Mật khẩu của bạn đã được đặt lại thành công!";
            return View("Success");
        }
    }
}

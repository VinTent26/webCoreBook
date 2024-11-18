using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webCore.Models;
using webCore.Services;

namespace webCore.MongoHelper
{

    public class ForgotPasswordService
    {
        private readonly IMongoCollection<ForgotPassword> _forgotPasswords;
        public ForgotPasswordService(MongoDBService mongoDBService)
        {
            _forgotPasswords = mongoDBService.ForgotPasswords;
        }
        public async Task<string> GenerateAndSendOTP(string email)
        {
            // Tạo mã OTP ngẫu nhiên
            var otp = new Random().Next(100000, 999999).ToString();
            var otpExpiry = DateTime.UtcNow.AddMinutes(5); // OTP hết hạn sau 5 phút

            // Tạo hoặc cập nhật thông tin OTP trong DB
            var forgotPassword = new ForgotPassword
            {
                Email = email,
                OTP = otp,
                OTPExpiry = otpExpiry
            };

            await _forgotPasswords.ReplaceOneAsync(
                f => f.Email == email,
                forgotPassword,
                new ReplaceOptions { IsUpsert = true } // Upsert nếu không tìm thấy email
            );

            // Gửi email chứa OTP
            SendEmail(email, "OTP Code", $"Your OTP code is: {otp}");

            return "OTP đã được gửi.";
        }

        // Xác minh OTP
        public async Task<bool> VerifyOTP(string email, string otp)
        {
            var forgotPassword = await _forgotPasswords
                .Find(f => f.Email == email && f.OTP == otp)
                .FirstOrDefaultAsync();

            // Kiểm tra OTP có hợp lệ và chưa hết hạn
            if (forgotPassword == null || forgotPassword.OTPExpiry < DateTime.UtcNow)
                return false;

            // Xóa OTP sau khi xác minh thành công
            await _forgotPasswords.DeleteOneAsync(f => f.Email == email);

            return true;
        }

        private void SendEmail(string to, string subject, string body)
        {
            // Tích hợp logic gửi email (SMTP hoặc thư viện như SendGrid)
            Console.WriteLine($"Email sent to {to}: {subject} - {body}");
        }
    }
}

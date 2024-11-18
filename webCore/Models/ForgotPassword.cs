using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webCore.Models
{
    public class ForgotPassword
    {
        public string Email { get; set; }  // Địa chỉ email của người dùng
        public string OTP { get; set; }   // Mã OTP được gửi qua email
        public DateTime? OTPExpiry { get; set; } 
    }
}

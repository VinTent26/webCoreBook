using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace webCore.Models
{
    [Table("Accounts")]
    public class Account_admin
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(100)]
        public string Password { get; set; }

        [MaxLength(100)]
        public string Token { get; set; } = GenerateRandomString(20);

        [Phone]
        [MaxLength(15)]
        public string Phone { get; set; }

        public string Avatar { get; set; }

        public string RoleId { get; set; }

        [MaxLength(50)]
        public string Status { get; set; }

        public bool Deleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        private static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}

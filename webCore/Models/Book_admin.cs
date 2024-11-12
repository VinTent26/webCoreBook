using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace webCore.Models
{
    [Table("Book")]
    public class Book_admin
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        private string title;
        [Required]
        [MaxLength(100)]
        public string Title
        {
            get => title;
            set
            {
                title = value;
                slug = GenerateSlug(value);
            }
        }

        [MaxLength(250)]
        public string Description { get; set; }

        [Column(TypeName = "decimal(10, 8)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal DiscountPercentage { get; set; }

        public int Stock { get; set; }

        public string Image { get; set; }

        [ForeignKey("Category_admin")] // Khóa ngoại liên kết với bảng Category
        public string CategoryId { get; set; } // Tên mới cho khóa ngoại

        // Thuộc tính mới để lưu tên danh mục
        [NotMapped] // Không lưu trong database
        public string CategoryTitle { get; set; }

        // Thiết lập quan hệ với bảng Category thông qua CategoryId
        public virtual Category_admin Category_admin { get; set; }

        [MaxLength(50)]
        public string Status { get; set; }

        public int Position { get; set; }

        public bool Deleted { get; set; } = false;

        private string slug;
        [MaxLength(100)]
        public string Slug
        {
            get => slug;
            private set => slug = value;
        }

        public DateTime? DeletedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Phương thức tạo slug
        private string GenerateSlug(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return string.Empty;

            // Chuyển sang chữ thường
            string slug = title.ToLowerInvariant();

            // Xóa dấu
            slug = RemoveDiacritics(slug);

            // Thay thế các ký tự không hợp lệ bằng dấu gạch nối
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", string.Empty);

            // Thay thế nhiều dấu gạch nối hoặc khoảng trắng bằng một dấu gạch nối
            slug = Regex.Replace(slug, @"[\s-]+", "-").Trim('-');

            return slug;
        }

        // Phương thức xóa dấu
        private string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new System.Text.StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}

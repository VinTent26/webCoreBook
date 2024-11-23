using System;

namespace webCore.Models
{
    public class Product
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal DiscountPercentage { get; set; }
        public string Image { get; set; }
        public string CategoryId { get; set; }
    }
}

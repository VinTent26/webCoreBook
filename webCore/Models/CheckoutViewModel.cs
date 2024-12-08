using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webCore.Models
{
    public class CheckoutViewModel
    {
        public List<CartItem> Items { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string VoucherDiscount { get; set; }
    }
}

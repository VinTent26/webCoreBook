using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webCore.Models
{
    public class PaymentInfoViewModel
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public List<CartItem> Items { get; set; }
    }
}


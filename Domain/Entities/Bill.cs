using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Bill
    {
        public string id { get; set; } = Guid.NewGuid().ToString();
        public decimal Amount { get; set; }
        public DateTime? PaidAt { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

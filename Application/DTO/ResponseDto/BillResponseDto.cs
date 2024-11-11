using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.ResponseDto
{
    public class BillResponseDto
    {
        public string BillId { get; set; }
        public PaymentStatus Status { get; set; }
        public decimal Amount { get; set; }
        public string Message { get; set; }
    }
}

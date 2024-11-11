using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.ResponseDto
{
    public class ConfirmPaymentResponseDto
    {
        public string WalletId { get; set; }
        public decimal NewBalance { get; set; }
        public decimal AmountAdded { get; set; }
        public string Message { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.ResponseDto
{
    public class RemoveFundsResponseDto
    {
        public string WalletId { get; set; }
        public decimal NewBalance { get; set; }
        public string Message { get; set; }
    }
}

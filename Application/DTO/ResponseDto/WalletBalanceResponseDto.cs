using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.ResponseDto
{
    public class WalletBalanceResponseDto
    {
        public decimal Balance { get; set; }
        public string Message { get; set; }
    }
}

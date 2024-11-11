using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.ResponseDto
{
    public class AddFundsResponseDto
    {
        public string WalletId { get; set; }
        public decimal Balance { get; set; }
        public string Message { get; set; }
        public string AuthorizationUrl { get; set; }
        public string Reference { get; set; }
    }

}

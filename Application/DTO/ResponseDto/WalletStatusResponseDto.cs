using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.ResponseDto
{
    public class WalletStatusResponseDto
    {
        public string WalletId { get; set; }
        public bool IsActive { get; set; }
        public string Message { get; set; }
    }
}

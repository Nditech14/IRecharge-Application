using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.ResponseDto
{
    public class PaystackVerifyData
    {
        public string status { get; set; }
        public decimal amount { get; set; }
        public string reference { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Untilities.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }

        public ApiResponse(T data, string message, int statusCode)
        {
            Success = true;
            Data = data;
            Message = message;
            StatusCode = statusCode;
        }

        public ApiResponse(string errorMessage)
        {
            Success = false;
            Data = default;
            Message = errorMessage;
        }
    }
}

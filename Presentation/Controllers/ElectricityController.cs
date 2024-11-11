using Application.DTO.RequestDto;
using Application.Service.Abstraction;
using Domain.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/electricity")]
    public class ElectricityController : ControllerBase
    {
        private readonly IBillService _billService;

        public ElectricityController(IBillService billService)
        {
            _billService = billService;
        }

       
        [HttpPost("verify")]
        public async Task<IActionResult> CreateBill([FromBody] CreateBillRequest request)
        {
            if (request == null || request.Amount <= 0)
                return BadRequest("Invalid bill request.");

            var response = await _billService.CreateBillAsync(request);
            if (response.Status == PaymentStatus.Failed)
                return BadRequest(response);

            return Ok(response);
        }

       
        [HttpPost("pay/{id}")]
        public async Task<IActionResult> PayBill([FromRoute] string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("Bill ID is required.");

            var response = await _billService.PayBillAsync(id);
            if (response.Status == PaymentStatus.Failed)
                return BadRequest(response);

            return Ok(response);
        }
    }
}

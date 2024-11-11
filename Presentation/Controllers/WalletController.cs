using Application.Service.Abstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/wallet")]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

     
        [HttpPost("create")]
        public async Task<IActionResult> CreateWallet()
        {
            var response = await _walletService.CreateWalletAsync();
            if (!response.Success)
                return StatusCode(response.StatusCode, response.Message);

            return StatusCode(response.StatusCode, response);
        }

       
        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance()
        {
            var response = await _walletService.GetWalletBalanceAsync();
            if (!response.Success)
                return StatusCode(response.StatusCode, response.Message);

            return Ok(response);
        }

        [HttpPost("fund")]
        public async Task<IActionResult> AddFunds([FromForm] decimal amount)
        {
            if (amount <= 0)
                return BadRequest("Amount must be greater than zero.");

            var response = await _walletService.AddFundsAsync(amount);
            if (!response.Success)
                return StatusCode(response.StatusCode, response.Message);

            return Ok(response);
        }
        [AllowAnonymous]
        [HttpGet("confirm-payment")]
        public async Task<IActionResult> ConfirmPayment([FromQuery(Name = "reference")] string reference)
        {
            var response = await _walletService.ConfirmPaymentAsync(reference);
            if (response.StatusCode == 200)
                return Ok(response);
            return BadRequest(response);
        }


        [HttpPost("withdraw")]
        public async Task<IActionResult> RemoveFunds([FromForm] decimal amount)
        {
            if (amount <= 0)
                return BadRequest("Amount must be greater than zero.");

            var response = await _walletService.RemoveFundsAsync(amount);
            if (!response.Success)
                return StatusCode(response.StatusCode, response.Message);

            return Ok(response);
        }

      
        [HttpPatch("deactivate")]
        public async Task<IActionResult> DeactivateWallet()
        {
            var response = await _walletService.DeactivateWalletAsync();
            if (!response.Success)
                return StatusCode(response.StatusCode, response.Message);

            return Ok(response);
        }

        
        [HttpPatch("reactivate")]
        public async Task<IActionResult> ReactivateWallet()
        {
            var response = await _walletService.ReactivateWalletAsync();
            if (!response.Success)
                return StatusCode(response.StatusCode, response.Message);

            return Ok(response);
        }

    }
}

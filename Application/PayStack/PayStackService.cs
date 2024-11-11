using Application.DTO.ResponseDto;
using Application.PayStcak;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Application.PayStack
{

    public class PayStackService : IPayStackService
    {
        private readonly PayStackSettings _settings;
        private readonly HttpClient _httpClient;

        public PayStackService(IOptions<PayStackSettings> settings)
        {
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_settings.BaseUrl)
            };
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.SecretKey}");
        }

        // Create a Paystack transaction
        public async Task<PaystackData> CreateTransactionAsync(decimal amount, string email, string reference)
        {
            var payload = new
            {
                email = email,
                amount = (int)(amount * 100), // Convert to kobo
                reference = reference,
                callback_url = _settings.CallbackUrl
            };

            var response = await _httpClient.PostAsJsonAsync("/transaction/initialize", payload);

            if (!response.IsSuccessStatusCode)
                return null;

            var responseContent = await response.Content.ReadFromJsonAsync<PaystackTransactionResponse>();
            return responseContent?.data;
        }

        // Verify payment after callback
        public async Task<bool> VerifyTransactionAsync(string reference)
        {
            var response = await _httpClient.GetAsync($"{_settings.BaseUrl}/transaction/verify/{reference}");
            if (!response.IsSuccessStatusCode)
                return false;

            var responseContent = await response.Content.ReadFromJsonAsync<PaystackTransactionVerifyResponse>();
            return responseContent?.data?.status == "success";
        }
    }
}

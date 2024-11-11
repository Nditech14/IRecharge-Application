using Newtonsoft.Json;
using System;

namespace Domain.Entities
{
    public class Wallet
    {
        public string id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("UserId")]
        public string UserId { get; set; }

        public string UserEmail { get; set; }
        public decimal Balance { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

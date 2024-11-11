using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Untilities.Communication.Implemenatation
{
     public  class PublishEvent
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PublishEvent> _logger;
       /* private readonly ServiceBusClient _serviceBusClient;*/

        public PublishEvent(IConfiguration configuration, ILogger<PublishEvent> logger
            /*ServiceBusClient serviceBusClient*/ 
            )
        {
            _configuration = configuration;
            _logger = logger;
           /* _serviceBusClient = serviceBusClient;*/
        }
        private async Task PublishEventAsync(string eventName, object eventData)
        {
            var topicName = _configuration.GetValue<string>($"AzureServiceBus:Topics:{eventName}");
            if (string.IsNullOrEmpty(topicName))
            {
                _logger.LogError($"PublishEventAsync: Topic name not configured for event {eventName}.");
                return;

            }

           
            /*var sender = _serviceBusClient.CreateSender(topicName);
            var messageBody = JsonSerializer.Serialize(eventData);
            var message = new ServiceBusMessage(messageBody)
            {
                Subject = eventName
            };

            try
            {
                await sender.SendMessageAsync(message);
                _logger.LogInformation($"PublishEventAsync: Event {eventName} published to topic {topicName}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"PublishEventAsync: Error publishing event {eventName} to topic {topicName}.");
            }*/
        }


    }
}

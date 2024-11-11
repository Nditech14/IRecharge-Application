using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Azure
{
    public class CosmosDbClientFactory
    {
        private readonly CosmosClient _client;
        private readonly CosmosDbSettings _settings;

        public CosmosDbClientFactory(IOptions<CosmosDbSettings> settings)
        {
            _settings = settings.Value;
            _client = new CosmosClient(_settings.Account, _settings.Key);
        }

        public Container GetContainer(string containerName)
        {
            var database = _client.GetDatabase(_settings.DatabaseName);
            return database.GetContainer(containerName);
        }

    }
}

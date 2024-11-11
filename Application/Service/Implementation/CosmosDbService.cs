using Application.Service.Abstraction;
using Infrastructure.ExternalService.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service.Implementation
{
    public class CosmosDbService<T> :  ICosmosDbService<T> where T : class
    {
        private readonly ICosmosDbRepository<T> _cosmosDbRepository;

        public CosmosDbService(ICosmosDbRepository<T> cosmosDbRepository)
        {
            _cosmosDbRepository = cosmosDbRepository;
        }

        public async Task<T> GetItemAsync(string id, string partitionKey)
        {
            var partitionKeyObj = new Microsoft.Azure.Cosmos.PartitionKey(partitionKey);
            return await _cosmosDbRepository.GetItemAsync(id, partitionKeyObj);
        }

        public async Task<IEnumerable<T>> GetItemsAsync(string query)
        {
            return await _cosmosDbRepository.GetItemsAsync(query);
        }

        public async Task AddItemAsync(T item)
        {
            await _cosmosDbRepository.AddItemAsync(item);
        }

        public async Task AddItemAsync(T item, string partitionKey)
        {
            await _cosmosDbRepository.AddItemAsync(item, partitionKey);
        }
        public async Task<IEnumerable<string>> GetAllActiveWalletEmailsAsync()
        {
            
            var emails = await _cosmosDbRepository.GetAllActiveWalletEmailsAsync();
            return emails;
        }

        public async Task UpdateItemAsync(string id, T item, string partitionKey)
        {
            await _cosmosDbRepository.UpdateItemAsync(id, item, partitionKey);
        }

        public async Task DeleteItemAsync(string id, string partitionKey)
        {
            await _cosmosDbRepository.DeleteItemAsync(id, partitionKey);
        }

        public async Task<IEnumerable<T>> SearchItemsByNameAsync(string name)
        {
            return await _cosmosDbRepository.SearchItemsByNameAsync(name);
        }
        public async Task<T> GetWalletByUserIdAsync(string userId)
        {
            return await _cosmosDbRepository.GetWalletByUserIdAsync(userId);
        }

        public async Task<T> SearchItemByNameAsync(string name)
        {
            return await _cosmosDbRepository.SearchItemByNameAsync(name);
        }


        public async Task<IEnumerable<T>> GetItemsWithLowStockAsync(int threshold)
        {
            return await _cosmosDbRepository.GetItemsWithLowStockAsync(threshold);
        }

        public async Task<(IEnumerable<T> Items, string ContinuationToken)> GetItemsWithContinuationTokenAsync(string continuationToken, int maxItemCount, string partitionKey = null)
        {
            return await _cosmosDbRepository.GetItemsWithContinuationTokenAsync(continuationToken, maxItemCount, partitionKey);
        }

        public async Task<(IEnumerable<T> Items, string ContinuationToken)> GetItemsWithContinuationTokenAsynczz(string continuationToken, int maxItemCount, string partitionKey = null)
        {
            return await _cosmosDbRepository.GetItemsWithContinuationTokenAsyncz(continuationToken, maxItemCount, partitionKey);
        }


    }
}

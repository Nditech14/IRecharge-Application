using Microsoft.Azure.Cosmos;

namespace Infrastructure.ExternalService.Abstraction
{
    public interface ICosmosDbRepository<T>
    {
        Task AddItemAsync(T item);
        Task AddItemAsync(T item, string partitionKey);
        Task DeleteItemAsync(string id, string partitionKey);
        Task<T> GetItemAsync(string id, PartitionKey partitionKey);
        FeedIterator<T> GetItemQueryIterator(QueryDefinition query, string continuationToken = null, QueryRequestOptions requestOptions = null);
        Task<IEnumerable<T>> GetItemsAsync(string queryString);
        Task<(IEnumerable<T> Items, string ContinuationToken)> GetItemsWithContinuationTokenAsync(string continuationToken, int maxItemCount = 30, string partitionKey = null);
        Task<(IEnumerable<T> Items, string ContinuationToken)> GetItemsWithContinuationTokenAsyncz(string continuationToken, int maxItemCount = 30, string query = null, string partitionKey = null);
        Task<IEnumerable<T>> GetItemsWithLowStockAsync(int threshold);
        Task<T> SearchItemByNameAsync(string name);
        Task<IEnumerable<T>> SearchItemsByNameAsync(string name);
        Task UpdateItemAsync(string id, T item, string partitionKey);
        Task<T> GetWalletByUserIdAsync(string userId);
        Task<IEnumerable<string>> GetAllActiveWalletEmailsAsync();
    }
}
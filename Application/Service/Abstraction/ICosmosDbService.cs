namespace Application.Service.Abstraction
{
    public interface ICosmosDbService<T> where T : class
    {
        Task AddItemAsync(T item);
        Task AddItemAsync(T item, string partitionKey);
        Task DeleteItemAsync(string id, string partitionKey);
        Task<T> GetItemAsync(string id, string partitionKey);
        Task<IEnumerable<T>> GetItemsAsync(string query);
        Task<(IEnumerable<T> Items, string ContinuationToken)> GetItemsWithContinuationTokenAsync(string continuationToken, int maxItemCount, string partitionKey = null);
        Task<(IEnumerable<T> Items, string ContinuationToken)> GetItemsWithContinuationTokenAsynczz(string continuationToken, int maxItemCount, string partitionKey = null);
        Task<IEnumerable<T>> GetItemsWithLowStockAsync(int threshold);
        Task<T> SearchItemByNameAsync(string name);
        Task<IEnumerable<T>> SearchItemsByNameAsync(string name);
        Task UpdateItemAsync(string id, T item, string partitionKey);
        Task<T> GetWalletByUserIdAsync(string userId);
        Task<IEnumerable<string>> GetAllActiveWalletEmailsAsync();
    }
}
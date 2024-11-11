
namespace Infrastructure.Untilities.CachManager
{
    public interface IRedisCacheManager
    {
        Task<T> GetItemAsync<T>(string key);
        Task RemoveItemAsync(string key);
        Task SetItemAsync<T>(string key, T item, TimeSpan? expiry = null);
    }
}
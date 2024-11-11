using Domain.Entities;
using Infrastructure.ExternalService.Abstraction;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Infrastructure.ExternalService.Implementation
{
    public class CosmosDbRepository<T> : ICosmosDbRepository<T>
    {
        private readonly Container _InventoryContainer;
        private readonly Container _PurchaseRequestContainer;

        public CosmosDbRepository(CosmosClient dbClient, IConfiguration configuration)
        {
            var databaseName = configuration["CosmosDb:DatabaseName"];
            _InventoryContainer = dbClient.GetContainer(databaseName, configuration["CosmosDb:Containers:ContainerOne"]);
            _PurchaseRequestContainer = dbClient.GetContainer(databaseName, configuration["CosmosDb:Containers:ContainerTwo"]);



        }


        private Container GetContainer()
        {
            if (typeof(T) == typeof(Wallet))
            {
                return _InventoryContainer;
            }

            else if (typeof(T) == typeof(Bill))
            {
                return _PurchaseRequestContainer;
            }

            else
                throw new ArgumentException($"No container available for type {typeof(T).Name}");
            {
            }
        }


        public async Task<T> GetItemAsync(string id, PartitionKey partitionKey)
        {
            var container = GetContainer();
            try
            {
                var response = await container.ReadItemAsync<T>(id, partitionKey);
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return default;
            }
        }


        public async Task<IEnumerable<T>> GetItemsAsync(string queryString)
        {
            var _container = GetContainer();
            var query = _container.GetItemQueryIterator<T>(new QueryDefinition(queryString));
            List<T> results = new List<T>();

            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                results.AddRange(response.ToList());
            }
            return results;
        }


        public async Task AddItemAsync(T item)
        {
            var _container = GetContainer();
            await _container.CreateItemAsync(item);
        }





        public async Task AddItemAsync(T item, string partitionKey)
        {
            var _container = GetContainer();
            await _container.CreateItemAsync(item, new PartitionKey(partitionKey));
        }


        public async Task UpdateItemAsync(string id, T item, string partitionKey)
        {
            var _container = GetContainer();
            await _container.UpsertItemAsync(item, new PartitionKey(partitionKey));
        }


        public async Task DeleteItemAsync(string id, string partitionKey)
        {
            var _container = GetContainer();
            await _container.DeleteItemAsync<T>(id, new PartitionKey(partitionKey));
        }


        public async Task<(IEnumerable<T> Items, string ContinuationToken)> GetItemsWithContinuationTokenAsync(string continuationToken, int maxItemCount = 30, string partitionKey = null)
        {
            var _container = GetContainer();
            var queryRequestOptions = new QueryRequestOptions { MaxItemCount = maxItemCount };

            if (partitionKey != null)
            {
                queryRequestOptions.PartitionKey = new PartitionKey(partitionKey);
            }

            var queryIterator = _container.GetItemQueryIterator<T>(continuationToken: continuationToken, requestOptions: queryRequestOptions);
            var results = new List<T>();
            string newContinuationToken = null;

            while (queryIterator.HasMoreResults)
            {
                var response = await queryIterator.ReadNextAsync();
                results.AddRange(response);
                newContinuationToken = response.ContinuationToken;

                if (results.Count >= maxItemCount)
                {
                    break;
                }
            }

            return (results, newContinuationToken);
        }


        public FeedIterator<T> GetItemQueryIterator(QueryDefinition query, string continuationToken = null, QueryRequestOptions requestOptions = null)
        {
            var _container = GetContainer();
            return _container.GetItemQueryIterator<T>(query, continuationToken, requestOptions);
        }

        public async Task<IEnumerable<T>> SearchItemsByNameAsync(string name)
        {
            var _container = GetContainer();


            var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.Name = @name")
                                    .WithParameter("@name", name);

            var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);
            var results = new List<T>();


            while (queryIterator.HasMoreResults)
            {
                var response = await queryIterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            return results;
        }
        public async Task<IEnumerable<string>> GetAllActiveWalletEmailsAsync()
        {
            var _container = GetContainer();

            // Define the query to fetch emails of all active wallets
            var queryDefinition = new QueryDefinition(
                "SELECT c.UserEmail FROM c WHERE c.IsActive = true AND c.UserEmail != ''");

            var queryIterator = _container.GetItemQueryIterator<EmailResult>(queryDefinition);
            var emails = new List<string>();

            // Iterate through the results and extract the emails
            while (queryIterator.HasMoreResults)
            {
                var response = await queryIterator.ReadNextAsync();
                emails.AddRange(response.Select(item => item.UserEmail));
            }

            return emails;
        }

        public async Task<T> SearchItemByNameAsync(string name)
        {
            var container = GetContainer();
            var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.Name = @name")
                                    .WithParameter("@name", name);

            var queryIterator = container.GetItemQueryIterator<T>(queryDefinition);

            if (queryIterator.HasMoreResults)
            {
                var response = await queryIterator.ReadNextAsync();
                return response.FirstOrDefault();
            }

            return default;
        }
        public async Task<T> GetBalanceByUserIdAsync(string userId)
        {
            var container = GetContainer();
            var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.UserId = @userId")
                                    .WithParameter("@name", userId);

            var queryIterator = container.GetItemQueryIterator<T>(queryDefinition);

            if (queryIterator.HasMoreResults)
            {
                var response = await queryIterator.ReadNextAsync();
                return response.FirstOrDefault();
            }

            return default;
        }
        public async Task<T> GetWalletByUserIdAsync(string userId)
        {
            var container = GetContainer();
            var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.UserId = @userId")
                                    .WithParameter("@userId", userId);

            using var queryIterator = container.GetItemQueryIterator<T>(queryDefinition);

            // Fetch the first page of results
            if (queryIterator.HasMoreResults)
            {
                var response = await queryIterator.ReadNextAsync();
                return response.FirstOrDefault();
            }

            return default;
        }


        public async Task<IEnumerable<T>> GetItemsWithLowStockAsync(int threshold)
        {
            var _container = GetContainer();


            var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.Quantity < @threshold")
                                    .WithParameter("@threshold", threshold);

            var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);
            var results = new List<T>();


            while (queryIterator.HasMoreResults)
            {
                var response = await queryIterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            return results;
        }
        public async Task<(IEnumerable<T> Items, string ContinuationToken)> GetItemsWithContinuationTokenAsyncz(string continuationToken, int maxItemCount = 30, string query = null, string partitionKey = null)
        {
            var _container = GetContainer();
            var queryRequestOptions = new QueryRequestOptions { MaxItemCount = maxItemCount };

            if (partitionKey != null)
            {
                queryRequestOptions.PartitionKey = new PartitionKey(partitionKey);
            }

            // Query based on the provided query string (e.g., filtering by status)
            var queryIterator = _container.GetItemQueryIterator<T>(new QueryDefinition(query), continuationToken: continuationToken, requestOptions: queryRequestOptions);

            var results = new List<T>();
            string newContinuationToken = null;

            while (queryIterator.HasMoreResults)
            {
                var response = await queryIterator.ReadNextAsync();
                results.AddRange(response);
                newContinuationToken = response.ContinuationToken;

                if (results.Count >= maxItemCount)
                {
                    break;
                }
            }

            return (results, newContinuationToken);
        }




    }

    internal class EmailResult
    {
        public string UserEmail { get; set; }
    }
}

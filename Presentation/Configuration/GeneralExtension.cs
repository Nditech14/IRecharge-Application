using Application.PayStcak;
using Application.Service.Abstraction;
using Application.Service.Implementation;
using Domain.Entities;
using Infrastructure.ExternalService.Abstraction;
using Infrastructure.ExternalService.Implementation;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Presentation.Configuration
{
    public static class GeneralExtension
    {
        public static IServiceCollection RegisterApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register CosmosClient with DI (Singleton)
            services.AddSingleton(provider =>
            {
                var cosmosDbConfig = configuration.GetSection("CosmosDb");
                var account = cosmosDbConfig["Account"];
                var key = cosmosDbConfig["Key"];

                // Validate Cosmos DB configuration
                if (string.IsNullOrWhiteSpace(account) || string.IsNullOrWhiteSpace(key))
                {
                    throw new ArgumentException("Cosmos DB account and key must be provided in the configuration.");
                }

                try
                {
                    return new CosmosClient(account, key);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Failed to create CosmosClient instance.", ex);
                }
            });

            // Register ICosmosDbRepository and ICosmosDbService as generic services
            services.AddScoped(typeof(ICosmosDbRepository<>), typeof(CosmosDbRepository<>));
            services.AddScoped(typeof(ICosmosDbService<>), typeof(CosmosDbService<>));
            services.Configure<PayStackSettings>(configuration.GetSection("PayStackSettings"));
            services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<PayStackSettings>>().Value);


            // Register a specific instance of ICosmosDbService for Wallet
            _ = services.AddScoped<ICosmosDbService<Wallet>>(provider =>
            {
                var cosmosDbRepository = provider.GetRequiredService<ICosmosDbRepository<Wallet>>();
                return new CosmosDbService<Wallet>(cosmosDbRepository);
            });


            // Register HttpContextAccessor for accessing the HTTP context
            services.AddHttpContextAccessor();

            return services;
        }
    }
}

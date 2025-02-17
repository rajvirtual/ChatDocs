namespace ChatDocsBackEnd.Extensions
{
    using Azure.Core;
    using Azure.Identity;
    using ChatDocsBackEnd.Services;
    using Microsoft.Azure.Cosmos;

    public static class CosmosExtensions
    {
        public static IServiceCollection AddCosmosDbService(this IServiceCollection services, IConfiguration configuration)
        {
            string cosmosEndpoint = configuration["AzureCosmosDB:Endpoint"]!;
            string databaseId = configuration["AzureCosmosDB:DatabaseName"]!;
            string containerId = configuration["AzureCosmosDB:ContainerName"]!;

#if DEBUG
            TokenCredential credentials;
            string tenantId = configuration["AppSettings:TenantId"]!;
            credentials = new AzureCliCredential(new AzureCliCredentialOptions
            {
                TenantId = tenantId,
            });

#else
                credentials = new DefaultAzureCredential();
#endif
            CosmosSerializationOptions options = new()
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
            };

            var cosmosClient = new CosmosClient(cosmosEndpoint,
                credentials, new CosmosClientOptions
                {
                    SerializerOptions = options
                });

            var container = cosmosClient.GetContainer(databaseId, containerId);

            var cosmosDbService = new CosmosDbService(container);

            services.AddSingleton<ICosmosDbService>(cosmosDbService);

            return services;
        }
    }
}

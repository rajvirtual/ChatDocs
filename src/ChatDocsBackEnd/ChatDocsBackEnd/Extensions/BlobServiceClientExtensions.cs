namespace ChatDocsBackEnd.Extensions
{
    using Azure.Storage;
    using Azure.Storage.Blobs;

    public static class BlobServiceClientExtensions
    {
        public static IServiceCollection AddBlobServiceClient(this IServiceCollection services, IConfiguration configuration)
        {
            string storageAccountName = configuration["AzureStorage:AccountName"]!;
            string storageAccountKey = configuration["AzureStorage:AccountKey"]!;

            var storageCredentials = new StorageSharedKeyCredential(storageAccountName, storageAccountKey);
            var client = new BlobServiceClient(new Uri($"https://{storageAccountName}.blob.core.windows.net"), storageCredentials);

            services.AddSingleton(client);

            return services;
        }
    }
}

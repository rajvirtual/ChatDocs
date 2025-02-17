namespace ChatDocsBackEnd.Extensions
{
    using Azure.Core;
    using Azure.Identity;
    using Azure.Storage.Blobs;

    public static class BlobServiceClientExtensions
    {
        public static IServiceCollection AddBlobServiceClient(this IServiceCollection services, IConfiguration configuration)
        {
            string storageAccountName = configuration["AzureStorage:AccountName"]!;
            TokenCredential credentials;
#if DEBUG
            string tenantId = configuration["AppSettings:TenantId"]!;
            credentials = new AzureCliCredential(new AzureCliCredentialOptions
            {
                TenantId = tenantId,
            });
#else
                credentials = new DefaultAzureCredential();
#endif
            var client = new BlobServiceClient(new Uri($"https://{storageAccountName}.blob.core.windows.net"), credentials);

            services.AddSingleton(client);

            return services;
        }
    }
}

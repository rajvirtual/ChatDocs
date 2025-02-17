using Azure.Storage.Blobs;

namespace ChatDocsBackEnd.Extensions
{
    using ChatDocsBackEnd.Services;
    public static class BlobServiceExtensions
    {
        public static IServiceCollection AddBlobService(this IServiceCollection services, IConfiguration configuration)
        {
            var _storageContainerName = configuration["AzureStorage:ContainerName"];
            var blobServiceClient = services.BuildServiceProvider().GetRequiredService<BlobServiceClient>();
            var containerClient = blobServiceClient.GetBlobContainerClient(_storageContainerName);
            containerClient.CreateIfNotExists();
            var blobService = new BlobService(containerClient);
            services.AddSingleton<IBlobService>(blobService);
            return services;
        }
    }
}

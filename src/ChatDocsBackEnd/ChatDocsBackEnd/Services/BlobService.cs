using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace ChatDocsBackEnd.Services
{
    public class BlobService : IBlobService
    {
        private readonly BlobContainerClient _blobContainerClient;

        public BlobService(BlobContainerClient blobContainerClient)
        {
            _blobContainerClient = blobContainerClient;
        }

        public async Task<string> UploadBlobAsync(string blobName, Stream stream)
        {
            var blobClient = _blobContainerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(stream, overwrite: true);
            return blobClient.Uri.AbsoluteUri.ToString();
        }

        public async Task DeleteBlobAsync(string blobName)
        {
            var blobClient = _blobContainerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync();
        }

        public string GetBlobSasUri(string blobName, DateTimeOffset expiryTime)
        {
            var blobClient = _blobContainerClient.GetBlobClient(blobName);

            if (blobClient.CanGenerateSasUri)
            {
                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = _blobContainerClient.Name,
                    BlobName = blobName,
                    Resource = "b",
                    ExpiresOn = expiryTime
                };

                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                Uri sasUri = blobClient.GenerateSasUri(sasBuilder);
                return sasUri.ToString();
            }
            else
            {
                throw new InvalidOperationException("BlobClient cannot generate SAS URI. Ensure the client is authorized with Shared Key credentials.");
            }
        }
    }
}

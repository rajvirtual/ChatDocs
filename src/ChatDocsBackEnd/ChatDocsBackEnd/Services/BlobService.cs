using Azure.Storage.Blobs;

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
    }
}

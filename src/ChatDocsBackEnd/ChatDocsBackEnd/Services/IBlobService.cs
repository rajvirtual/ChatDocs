namespace ChatDocsBackEnd.Services
{
    public interface IBlobService
    {
        Task<string> UploadBlobAsync(string blobName, Stream stream);
        Task DeleteBlobAsync(string blobName);
    }
}

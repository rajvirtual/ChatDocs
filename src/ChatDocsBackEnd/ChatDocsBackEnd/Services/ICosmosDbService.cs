namespace ChatDocsBackEnd.Services
{
    using ChatDocsBackEnd.Data;
    public interface ICosmosDbService
    {
        Task<List<CosmosDBSearchResult>> ListDocumentsAsync();
        Task<string?> DeleteDocumentsAsync(string documentId);
        Task CreateItemsAsync(IEnumerable<DataChunk> chunks);
        Task<List<(CosmosDBSearchResult Chunk, double Similarity)>> SearchSimilarDocuments(ReadOnlyMemory<float> queryEmbeddings,
                                               int topK = 5,
                                               CancellationToken cancellationToken = default);
    }
}

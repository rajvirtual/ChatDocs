namespace ChatDocsBackEnd.Services
{
    using ChatDocsBackEnd.Data;
    using Microsoft.Azure.Cosmos;

    public class CosmosDbService : ICosmosDbService
    {
        private readonly Container _cosmosContainer;
        public CosmosDbService(Container cosmosContainer)
        {
            _cosmosContainer = cosmosContainer;
        }

        public async Task<List<CosmosDBSearchResult>> ListDocumentsAsync()
        {
            var query = new QueryDefinition(
                @"SELECT DISTINCT c.documentId, c.documentName 
                  FROM c 
                  WHERE c.documentType = 'Pdf'");

            var iterator = _cosmosContainer.GetItemQueryIterator<CosmosDBSearchResult>(query);
            var results = new List<CosmosDBSearchResult>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response);
            }

            return results;
        }

        public async Task<string?> DeleteDocumentsAsync(string documentId)
        {
            var query = new QueryDefinition(
         "SELECT c.id, c.documentId, c.documentType, c.documentName, c.PartitionKey FROM c WHERE c.documentId = @docId")
         .WithParameter("@docId", documentId);
            var iterator = _cosmosContainer.GetItemQueryIterator<DataChunk>(query);

            string? documentName = null;

            List<(string Id, string PartitionKey)> itemsToDelete = new();

            while (iterator.HasMoreResults)
            {
                var chunks = await iterator.ReadNextAsync();
                foreach (var chunk in chunks)
                {
                    documentName ??= chunk.DocumentName;
                    itemsToDelete.Add((chunk.Id, chunk.PartitionKey));
                }
            }

            foreach (var group in itemsToDelete.GroupBy(item => item.PartitionKey))
            {
                var batch = _cosmosContainer.CreateTransactionalBatch(new PartitionKey(group.Key));
                foreach (var item in group)
                {
                    batch.DeleteItem(item.Id);
                }

                var response = await batch.ExecuteAsync();
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to delete items in batch for documentId: {documentId}");
                }
            }

            return documentName;
        }

        public async Task CreateItemsAsync(IEnumerable<DataChunk> chunks)
        {
            foreach (var group in chunks.GroupBy(chunk => chunk.PartitionKey))
            {
                var batch = _cosmosContainer.CreateTransactionalBatch(new PartitionKey(group.Key));
                foreach (var chunk in group)
                {
                    batch.CreateItem(chunk);
                }

                var response = await batch.ExecuteAsync();
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Failed to create items in batch");
                }
            }
        }

        public async Task<List<(CosmosDBSearchResult Chunk, double Similarity)>> SearchSimilarDocuments(ReadOnlyMemory<float> queryEmbeddings, 
                                       int topK = 5,
                                       CancellationToken cancellationToken = default)
        {
            var searchQuery = new QueryDefinition(
                     @"SELECT TOP @topK
                  c.id,
                  c.partitionKey,
                  c.documentType,
                  c.documentId,
                  c.documentName,
                  c.pageIndex,
                  c.chunkIndex,
                  c.content,
                  c.normalizedContent,
                  c.source,
                  VectorDistance(c.embedding, @queryVector) as similarity
                  FROM c
                  ORDER BY VectorDistance(c.embedding, @queryVector)
                  ")
                     .WithParameter("@topK", topK)
                     .WithParameter("@queryVector", queryEmbeddings.ToArray());

            var queryRequestOptions = new QueryRequestOptions
            {
                MaxBufferedItemCount = topK,
                MaxConcurrency = -1
            };

            var results = new List<(CosmosDBSearchResult, double)>();

            using (var iterator = _cosmosContainer.GetItemQueryIterator<dynamic>(
            searchQuery,
            requestOptions: queryRequestOptions))
            {
                while (iterator.HasMoreResults)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var response = await iterator.ReadNextAsync(cancellationToken);
                    foreach (var item in response)
                    {
                        results.Add((CosmosDBSearchResult.FromCosmosQuery(item),
                            (double)item.similarity));
                    }
                }
            }

            // make sure same page numbers are merged together for context
            var groupedResults = results
                    .GroupBy(r => r.Item1.PageIndex)
                    .Select(group => (new CosmosDBSearchResult
                    {
                        Id = group.First().Item1.Id,
                        PageIndex = group.Key,
                        Content = string.Join(" ", group.Select(r => r.Item1.Content)),
                        NormalizedContent = string.Join(" ", group.Select(r => r.Item1.NormalizedContent)),
                        DocumentId = group.First().Item1.DocumentId,
                        DocumentName = group.First().Item1.DocumentName,
                        DocumentType = group.First().Item1.DocumentType,
                        Source = group.First().Item1.Source,
                        Similarity = group.Sum(r => r.Item2)
                    }, group.Max(r => r.Item2)))
                    .OrderByDescending(r => r.Item2)
                    .ToList();

            return groupedResults.ToList();
        }

    }
}

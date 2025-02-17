namespace ChatDocsBackEnd.Data
{
    public record CosmosDBSearchResult
    {
        public required string Id { get; set; }
        public required string DocumentType { get; set; }
        public required string DocumentId { get; set; }
        public string? DocumentName { get; set; } = null;
        public required int PageIndex { get; set; }
        public int ChunkIndex { get; set; }
        public required string Content { get; set; }
        public required string NormalizedContent { get; set; }
        public string? Source { get; set; }
        public double? Similarity { get; set; }

        public static CosmosDBSearchResult FromCosmosQuery(dynamic item)
        {
            return new CosmosDBSearchResult
            {
                Id = item.id,
                DocumentType = item.documentType,
                DocumentId = item.documentId,
                PageIndex = item.pageIndex,
                DocumentName = item.documentName,
                ChunkIndex = item.chunkIndex,
                Content = item.content,
                NormalizedContent = item.normalizedContent,
                Source = item.source ?? null,
                Similarity = item.similarity
            };
        }
    }
}

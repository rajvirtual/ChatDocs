using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ChatDocsBackEnd.Data
{
    public record DataChunk
    {
        [JsonProperty(PropertyName = "id")]
        public required string Id { get; set; }
        public required string DocumentType { get; set; } //(Pdf,Wiki,Loop)
        [JsonProperty(PropertyName = "documentId")]
        public required string DocumentId { get; set; } // Unique ID for the document
        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey => $"{DocumentType}_{DocumentId[..2]}";
        public string DocumentName { get; set; } = ""; // Name of the document, applicable only for PDFs
        public required int PageIndex { get; set; } // Page number in the PDF
        public int ChunkIndex { get; set; } // Chunk index within the page
        public required string Content { get; set; } // Text content of the chunk

        public required string NormalizedContent { get; set; } // Text content of the chunk
        public required float[] Embedding { get; set; } // Embedding vector for the chunk
        public List<string> Tags { get; set; } = []; // Tags for the chunk
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Timestamp

        public string? Source { get; set; } // Optional source of the document
    }

    public enum DocumentType
    {
        Pdf,
        Wiki,
        Loop
    }
}

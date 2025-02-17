namespace ChatDocsBackEnd.Options
{
    public class OpenAIOptions
    {
        public const string SectionName = "AzureOpenAI";

        public required string ApiKey { get; set; }
        public required string ModelName { get; set; }
        public required string EmbeddingDeploymentName { get; set; }
        public string? Endpoint { get; set; }
    }
}

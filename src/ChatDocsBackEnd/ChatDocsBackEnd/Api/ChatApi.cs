namespace ChatDocsBackEnd.Api
{
    using ChatDocsBackEnd.Data;
    using ChatDocsBackEnd.Services;
    using ChatDocsBackEnd.Utils;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.AI;
    using System.Runtime.CompilerServices;
    using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

    public static class ChatApi
    {
        public static void MapChatApiEndpoints(this WebApplication app)
        {
            app.MapPost("/chat", Chat);
        }

        public static async Task<IResult> Chat(
            [FromServices] ICosmosDbService cosmosDbService,
            [FromServices] IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
            HttpContext httpContext,
            [FromServices] IChatClient chatClient,
            [FromBody] ChatRequest chatRequest,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(chatRequest.UserQuery))
            {
                return Results.BadRequest("User Query cannot be empty");
            }

            var normalizedUserQuery = TextNormalizer.NormalizeText(chatRequest.UserQuery);
            var queryEmbeddings = await embeddingGenerator.GenerateEmbeddingVectorAsync(normalizedUserQuery, null, cancellationToken);

            var searchResults = await cosmosDbService.SearchSimilarDocuments(
                   queryEmbeddings,
                   topK: 5,
                   cancellationToken);

            httpContext.Response.Headers.Append("Content-Type", "text/plain; charset=utf-8");
            httpContext.Response.Headers.Append("Transfer-Encoding", "chunked");
            httpContext.Response.Headers.Append("Connection", "keep-alive");
            // Disable response buffering
            httpContext.Response.Headers.Append("X-Content-Type-Options", "nosniff");

            await foreach (var response in GenerateStreamingResponse(chatRequest.UserQuery,
                              searchResults,
                              chatClient,
                              cancellationToken))
            {
                var formattedMessage = $"{response}\n\n";//front end knows to split on \n\n
                await httpContext.Response.WriteAsync(formattedMessage, cancellationToken);
                await httpContext.Response.Body.FlushAsync(cancellationToken);
            }

            // Send an end event// this is parsed by front end to know to continue.
            await httpContext.Response.WriteAsync("[DONE]\n\n", cancellationToken);

            return Results.Ok();
        }

        private static async IAsyncEnumerable<string> GenerateStreamingResponse(string query,
         List<(CosmosDBSearchResult Chunk, double Similarity)> context,
         IChatClient chatClient,
        [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var formattedContext = context.Select(r =>
                $"""
            [Document Information]
            Document Name: {r.Chunk.DocumentName}
            Page Number: {r.Chunk.PageIndex}
            Source: {r.Chunk.Source ?? "N/A"}
            Relevance Score: {r.Similarity:F2}
            
            Content:
            {r.Chunk.Content}
            """);

            var chatMessages = new List<ChatMessage>();

            var message = $"""
                Use ONLY the following document excerpts to answer the question. 

                Documents:
                {string.Join("\n\n", formattedContext)}

                Question: {query}

                Respond directly with the relevant information, following the provided rules.
                """;

            chatMessages.Add(new ChatMessage
            {
                Role = ChatRole.System,
                Text = """
        You are a technical assistant that answers ONLY using the provided documents. Follow these strict rules:

        - Provide information EXACTLY as stated in the documents.
        - If the answer is not in the documents, reply: "The provided documents do not contain this information."
        - Do not add introductions, summaries, or conclusions.
        - If asked to summarize, generate a structured summary using the key points from the documents.  
        - Do not use titles or headings.
        - Do not include general knowledge outside the provided documents.
        - Use concise sentences and bullet points (-) only when listing multiple items.
        - Always cite the source in the format: [Document: filename, Page: X, Link: URL]
        - If the source documents contain conflicting information, acknowledge this and cite both sources.
        """
            });

            // Add user message
            chatMessages.Add(new ChatMessage { Role = ChatRole.User, Text = message });

            var options = new ChatOptions
            {
                Temperature = 0.7f,
                MaxOutputTokens = 800
            };

            var streamingResponse = chatClient.CompleteStreamingAsync(
                chatMessages, options, cancellationToken);

            await foreach (var response in streamingResponse.WithCancellation(cancellationToken))
            {
                if (response?.Text != null)
                {
                    yield return response.Text;
                }
            }
        }
    }
}

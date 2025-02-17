#pragma warning disable
namespace ChatDocsBackEnd.Api
{
    using ChatDocsBackEnd.Data;
    using ChatDocsBackEnd.Services;
    using ChatDocsBackEnd.Utils;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.AI;
    using Microsoft.SemanticKernel.Text;
    using UglyToad.PdfPig;
    using UglyToad.PdfPig.Content;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;

    public static class DocumentApi
    {
        private const int CHUNK_SIZE = 300;
        private const int OVERLAP_SIZE = 100;

        public static void MapDocumentApiEndpoints(this WebApplication app)
        {
            app.MapGet("/documents", ListDocumentsAsync);
            app.MapDelete("/documents/{documentId}", DeleteDocumentsAsync);
            app.MapPost("/documents", Upload).DisableAntiforgery();
        }

        public static async Task<IResult> ListDocumentsAsync([FromServices] ICosmosDbService cosmosDbService)
        {
            try
            {
                return Results.Ok(await cosmosDbService.ListDocumentsAsync());
            }
            catch (Exception ex)
            {
                // Handle general exceptions
                return Results.Problem(detail: ex.Message, statusCode: 500);
            }
        }

        public static async Task<IResult> Upload([FromServices] IBlobService blobService,
                                                [FromServices] ICosmosDbService cosmosDbService,
                                                IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
                                                IFormFile file)
        {
            try
            {
                var fileName = file.FileName;
                using var fileStream = file.OpenReadStream();
                var url = await blobService.UploadBlobAsync(fileName, fileStream);

                // chunk the file and create embeddings 
                var documentId = Guid.NewGuid().ToString(); // Generate a unique DocumentId
                int paragraphIndex = 0;
                fileStream.Seek(0, SeekOrigin.Begin);
                var pdf = PdfDocument.Open(fileStream);
                var pages = pdf.GetPages().ToList();
                foreach (var page in pages)
                {
                    var pageText = ExtractStructuredContent(page);
                    if (string.IsNullOrWhiteSpace(pageText)) continue;
                    var paragraphs = TextChunker.SplitPlainTextParagraphs([pageText], maxTokensPerParagraph: CHUNK_SIZE,
                        overlapTokens: OVERLAP_SIZE).ToList();
                    var paragraphsWithEmbeddings = await embeddingGenerator.
                        GenerateAndZipAsync(paragraphs);
                    var manualChunks =
                               paragraphsWithEmbeddings.Select(p => new DataChunk
                               {
                                   Id = Guid.NewGuid().ToString(),
                                   DocumentType = DocumentType.Pdf.ToString(),
                                   DocumentId = documentId,
                                   DocumentName = fileName,
                                   PageIndex = page.Number,
                                   ChunkIndex = ++paragraphIndex,
                                   Content = p.Value,
                                   NormalizedContent = TextNormalizer.NormalizeText(p.Value),
                                   Embedding = p.Embedding.Vector.ToArray(),
                                   Source = url,
                               });

                    await cosmosDbService.CreateItemsAsync(manualChunks);
                }

                var result = new
                {
                    documentId = documentId,
                    documentName = fileName,
                    message = "File uploaded successfully."
                };

                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: 500);
            }
        }

        public static async Task<IResult> DeleteDocumentsAsync([FromServices] ICosmosDbService cosmosDbService,
                                                               [FromServices] IBlobService blobService,
                                                                string documentId)
        {
            try
            {
                var documentName = await cosmosDbService.DeleteDocumentsAsync(documentId);

                if (documentName != null)
                {
                    // Delete from Blob
                    await blobService.DeleteBlobAsync(documentName);
                }

                return Results.Ok("Document deleted succesfully");
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: 500);
            }
        }

        private static string ExtractStructuredContent(Page page)
        {
            try
            {
                // Get text blocks using multiple PdfPig extractors for better accuracy
                var letters = page.Letters;

                // Use NearestNeighbour for basic word formation
                var words = NearestNeighbourWordExtractor.Instance.GetWords(letters);

                // Use Docstrum for better block detection
                var docstrumBlocks = DocstrumBoundingBoxes.Instance.GetBlocks(words);

                // Combine results for better accuracy
                var structuredBlocks = docstrumBlocks
                    .Select(block => new
                    {
                        Text = block.Text,
                        BoundingBox = block.BoundingBox
                    })
                    .OrderBy(b => b.BoundingBox.Top)
                    .ThenBy(b => b.BoundingBox.Left);

                // Join blocks preserving structure
                return string.Join(
                    Environment.NewLine + Environment.NewLine,
                    structuredBlocks.Select(block =>
                        block.Text.ReplaceLineEndings(" ").Trim()));
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
    }
}

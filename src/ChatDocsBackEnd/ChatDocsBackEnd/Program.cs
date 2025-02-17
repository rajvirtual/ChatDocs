using ChatDocsBackEnd.Api;
using ChatDocsBackEnd.Extensions;
using Microsoft.Extensions.Azure;

namespace ChatDocsBackEnd
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddEmbeddingGenerator(builder.Configuration);
            builder.Services.AddOpenAIChatClient(builder.Configuration);
            builder.Services.AddCosmosDbService(builder.Configuration);
            builder.Services.AddBlobServiceClient(builder.Configuration);
            builder.Services.AddBlobService(builder.Configuration);
            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseCors("AllowAll");

            app.UseAuthorization();

            app.MapDocumentApiEndpoints();
            app.MapChatApiEndpoints();
            app.Run();
        }
    }
}

namespace ChatDocsBackEnd.Extensions
{
    using Azure.AI.OpenAI;
    using Microsoft.Extensions.AI;
    using System.ClientModel;

    public static class OpenAIChatClientExtensions
    {
        public static IServiceCollection AddOpenAIChatClient(this IServiceCollection services, IConfiguration configuration)
        {
            // Get configuration values
            string azureOpenAIEndpoint = configuration["AzureOpenAI:Endpoint"]!;
            string modelName = configuration["AzureOpenAI:ModelName"]!;
            string apiKey = configuration["AzureOpenAI:ApiKey"]!;

            IChatClient chatClient = new AzureOpenAIClient(new Uri(azureOpenAIEndpoint),
                new ApiKeyCredential(apiKey)).AsChatClient(modelName).AsBuilder()
                .UseFunctionInvocation().Build();

            services.AddSingleton(services =>
            {
                return chatClient;
            });

            return services;
        }
    }
}

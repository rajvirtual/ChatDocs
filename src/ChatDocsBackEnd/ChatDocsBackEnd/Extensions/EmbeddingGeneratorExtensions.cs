namespace ChatDocsBackEnd.Extensions
{
    using Azure.AI.OpenAI;
    using Azure.Core;
    using Azure.Identity;
    using Microsoft.Extensions.AI;

    public static class EmbeddingGeneratorExtensions
    {
        public static IServiceCollection AddEmbeddingGenerator(this IServiceCollection services,
            IConfiguration configuration)
        {
            string azureOpenAIEndpoint = configuration["AzureOpenAI:Endpoint"]!;
            string deploymentName = configuration["AzureOpenAI:EmbeddingDeploymentName"]!;
            TokenCredential credential;
#if DEBUG
            credential = new AzureCliCredential(new AzureCliCredentialOptions
            {
                TenantId = configuration["AppSettings:TenantId"]!,
            });

#else
                credential = new DefaultAzureCredential();
#endif

            var generator = new AzureOpenAIClient(new Uri(azureOpenAIEndpoint), credential)
                .AsEmbeddingGenerator(deploymentName);

            services.AddSingleton(generator);

            return services;
        }
    }
}

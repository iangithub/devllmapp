using Microsoft.Identity.Client;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace RagAgentLinebot.Models
{
    public class RagTrafficLawPlugin
    {
        private readonly IConfiguration _configuration;
        
        public RagTrafficLawPlugin(IServiceProvider serviceProvider)
        {
            _configuration = serviceProvider.GetService<IConfiguration>();
        }

        [KernelFunction, Description("Query Taiwan Traffic Law information.")]
        public string GetLawInformation(
        [Description("traffic law of user question.")]
        string query)
        {
            //Embedding Model Config
            var aoaiEmbeddingConfig = new AzureOpenAIConfig
            {
                APIKey = _configuration["AzureOpenAIChatApiKey"],
                Deployment = _configuration["AzureOpenAIEmbeddingDeploymentName"],
                Endpoint = _configuration["AzureOpenAIChatEndpoint"],
                APIType = AzureOpenAIConfig.APITypes.ChatCompletion,
                Auth = AzureOpenAIConfig.AuthTypes.APIKey
            };
            var aoaiChatConfig = new AzureOpenAIConfig
            {
                APIKey = _configuration["AzureOpenAIChatApiKey"],
                Deployment = _configuration["AzureOpenAIChatDeploymentName"],
                Endpoint = _configuration["AzureOpenAIChatEndpoint"],
                APIType = AzureOpenAIConfig.APITypes.ChatCompletion,
                Auth = AzureOpenAIConfig.AuthTypes.APIKey
            };
            var kernelMemory = new KernelMemoryBuilder()
                                .WithAzureOpenAITextGeneration(aoaiChatConfig)
                                .WithAzureOpenAITextEmbeddingGeneration(aoaiEmbeddingConfig)
                                .WithQdrantMemoryDb(_configuration["Cloud_Qdrant_Endpoint"], _configuration["Cloud_Qdrant_AccessKey"])
                                .Build<MemoryServerless>();

            var ans = kernelMemory.AskAsync(query, minRelevance: 0.6f, index: "taiwan_traffic_law").GetAwaiter().GetResult();

            var reference = ans.RelevantSources.Count > 0 ? $" (Ref: {ans.RelevantSources[0].SourceName})" : string.Empty;
            return $"{ans.Result}(ref:{reference})";
        }
    }
}

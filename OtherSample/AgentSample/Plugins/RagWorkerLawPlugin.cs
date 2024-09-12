using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using DocumentFormat.OpenXml.Office2010.Word;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using MongoDB.Bson.IO;

public class RagWorkerLawPlugin
{
    [KernelFunction, Description("Query Taiwan Labor Law information.")]
    public string GetLawInformation(
        [Description("labor law of user question.")]
        string query)
    {
        //Embedding Model Config
        var aoaiEmbeddingConfig = new AzureOpenAIConfig
        {
            APIKey = AppConfig.AzureOpenAIChatApiKey,
            Deployment = AppConfig.AzureOpenAIEmbeddingDeploymentName,
            Endpoint = AppConfig.AzureOpenAIChatEndpoint,
            APIType = AzureOpenAIConfig.APITypes.ChatCompletion,
            Auth = AzureOpenAIConfig.AuthTypes.APIKey
        };
        var aoaiChatConfig = new AzureOpenAIConfig
        {
            APIKey = AppConfig.AzureOpenAIChatApiKey,
            Deployment = AppConfig.AzureOpenAIChatDeploymentName,
            Endpoint = AppConfig.AzureOpenAIChatEndpoint,
            APIType = AzureOpenAIConfig.APITypes.ChatCompletion,
            Auth = AzureOpenAIConfig.AuthTypes.APIKey
        };
        var kernelMemory = new KernelMemoryBuilder()
                                        .WithAzureOpenAITextGeneration(aoaiChatConfig)
                                        .WithAzureOpenAITextEmbeddingGeneration(aoaiEmbeddingConfig)
                                        .WithQdrantMemoryDb(AppConfig.Cloud_Qdrant_Endpoint, AppConfig.Cloud_Qdrant_AccessKey)
                                        .Build<MemoryServerless>();

        var ans = kernelMemory.AskAsync(query, minRelevance: 0.6f, index: "taiwan_worker_law").GetAwaiter().GetResult();
        var reference = ans.RelevantSources.Count > 0 ? $" (Ref: {ans.RelevantSources[0].SourceName})" : string.Empty;
        return $"{ans.Result}(ref:{reference})";
    }
}



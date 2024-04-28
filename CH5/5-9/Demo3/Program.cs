using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;

namespace IntroSample
{
    internal class Program
    {
        private const string deploy_Name = "xxxx";
        private const string deploy_embedding_Name = "xxxx";
        private const string aoai_Endpoint = "https://xxxx.openai.azure.com";
        private const string api_Key = "xxxxx";
        private const string search_Endpoint = "https://xxxx.search.windows.net";
        private const string search_ApiKey = "xxxxxx";


        static async Task Main(string[] args)
        {

            //Chat Model Config
            var aoaiChatConfig = new AzureOpenAIConfig
            {
                APIKey = api_Key,
                Deployment = deploy_Name,
                Endpoint = aoai_Endpoint,
                APIType = AzureOpenAIConfig.APITypes.ChatCompletion,
                Auth = AzureOpenAIConfig.AuthTypes.APIKey
            };

            //Embedding Model Config
            var aoaiEmbeddingConfig = new AzureOpenAIConfig
            {
                APIKey = api_Key,
                Deployment = deploy_embedding_Name,
                Endpoint = aoai_Endpoint,
                APIType = AzureOpenAIConfig.APITypes.ChatCompletion,
                Auth = AzureOpenAIConfig.AuthTypes.APIKey
            };

            var kernelMemory = new KernelMemoryBuilder()
                            .WithAzureOpenAITextGeneration(aoaiChatConfig)
                            .WithAzureOpenAITextEmbeddingGeneration(aoaiEmbeddingConfig)
                            .WithAzureAISearchMemoryDb(search_Endpoint, search_ApiKey)
                            .Build<MemoryServerless>();

            await ImportKm(kernelMemory);

            Console.WriteLine("\n\n");

            //AskAsync是經過參考向量檢索資料後，再做文本生成的結果，簡單來說就是答案
            var ans = await kernelMemory.AskAsync("闖紅燈罰多少錢", minRelevance: 0.8f);
            Console.WriteLine(ans.Result);
            foreach (var source in ans.RelevantSources)
            {
                Console.WriteLine($"source:{source.DocumentId}");
            }

            Console.WriteLine("\n=========================================\n");

            //SearchAsync是指經過向量檢索後的結果，尚未經過文本生成，也就是說這個結果是向量檢索的結果，簡單來說就是參考資料
            var search_ref = await kernelMemory.SearchAsync("闖紅燈罰多少錢", limit: 2, minRelevance: 0.8f);
            foreach (var item in search_ref.Results)
            {
                Console.WriteLine(item.Partitions.First().Text);
                Console.WriteLine(item.Partitions.First().Relevance);
                Console.WriteLine();
            }

            Console.ReadLine();
        }

        static async Task ImportKm(MemoryServerless memory)
        {
            await memory.ImportDocumentAsync(Path.Combine(Directory.GetCurrentDirectory(), "道路交通管理處罰條例.pdf"), documentId: "taiwan_traffic_law");
        }
    }
}
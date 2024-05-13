using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace IntroSample
{
    internal class Program
    {
        private const string aoai_embedding_Name = "xxxx";
        private const string aoai_Endpoint = "https://xxxx.openai.azure.com";
        private const string api_Key = "xxxxxx";

        static async Task Main(string[] args)
        {
            //Chat Model Config
            var ollamaConfig = new OpenAIConfig
            {
                Endpoint = "http://localhost:11434/v1",
                TextModel = "llama3:8b",
                APIKey = "0"
            };

            //Embedding Model Config
            var aoaiEmbeddingConfig = new AzureOpenAIConfig
            {
                APIKey = api_Key,
                Deployment = aoai_embedding_Name,
                Endpoint = aoai_Endpoint,
                APIType = AzureOpenAIConfig.APITypes.ChatCompletion,
                Auth = AzureOpenAIConfig.AuthTypes.APIKey
            };

            var kernelMemory = new KernelMemoryBuilder()
                                .WithOpenAITextGeneration(ollamaConfig)
                                .WithAzureOpenAITextEmbeddingGeneration(aoaiEmbeddingConfig)
                                .WithQdrantMemoryDb("http://localhost:6333")
                                .Build<MemoryServerless>();

            //await ImportKm(kernelMemory);

            Console.WriteLine("\n============ QA Start  (AskAsync) ===================\n");


            //AskAsync是經過參考向量檢索資料後，再做文本生成的結果，簡單來說就是答案
            var ans = await kernelMemory.AskAsync("闖紅燈罰多少錢，請以中文回答", minRelevance: 0.6f);
            Console.WriteLine(ans.Result);
            foreach (var source in ans.RelevantSources)
            {
                Console.WriteLine($"source:{source.DocumentId}");
            }

            Console.WriteLine("\n============== SearchAsync ================\n");

            //SearchAsync是指經過向量檢索後的結果，尚未經過文本生成，也就是說這個結果是向量檢索的結果，簡單來說就是參考資料
            var search_ref = await kernelMemory.SearchAsync("闖紅燈罰多少錢，請以中文回答", limit: 2, minRelevance: 0.6f);
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
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;

namespace IntroSample
{
    internal class Program
    {
        private const string deploy_Name = "xxxx";
        private const string deploy_embedding_Name = "xxxx";
        private const string aoai_Endpoint = "https://xxxx.openai.azure.com";
        private const string api_Key = "xxxxx";


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
                .WithSimpleVectorDb()
                .Build<MemoryServerless>();

            await ImportKm(kernelMemory);

            var ans = await kernelMemory.AskAsync("交通法規裡的道路是指什麼", minRelevance: 0.8f);
            Console.WriteLine(ans.Result);
            foreach (var source in ans.RelevantSources)
            {
                Console.WriteLine($"source:{source.DocumentId}");
            }

            Console.ReadLine();
        }

        static async Task ImportKm(MemoryServerless memory)
        {
            await memory.ImportTextAsync("一、道路：指公路、街道、巷衖、廣場、騎樓、走廊或其他供公眾通行之地方。");
            await memory.ImportTextAsync("二、車道：指以劃分島、護欄或標線劃定道路之部分，及其他供車輛行駛之道路。");
            await memory.ImportTextAsync("三、人行道：指為專供行人通行之騎樓、走廊，及劃設供行人行走之地面道路，與人行天橋及人行地下道。");
            await memory.ImportTextAsync("四、行人穿越道：指在道路上以標線劃設，供行人穿越道路之地方。");
            await memory.ImportTextAsync("五、標誌：指管制道路交通，表示警告、禁制、指示，而以文字或圖案繪製之標牌。");
        }
    }
}
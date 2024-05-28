using Microsoft.KernelMemory;
using Microsoft.KernelMemory.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using MongoDB.Driver;

namespace IntroSample
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //Chat Embedding Model Config
            var ollamaEmbeddingConfig = new OllamaEmbeddingGeneratorConfig
            {
                Endpoint = "http://localhost:11434/api/embeddings", // for OllamaEmbedding
                EmbeddingModel = "snowflake-arctic-embed"
            };

            //Chat Model Config
            var ollamaGenerationConfig = new OpenAIConfig
            {
                Endpoint = "http://localhost:11434/v1",
                TextModel = "phi3",
                APIKey = "0"
            };
            var kernelMemory = new KernelMemoryBuilder()
                                .WithOpenAITextGeneration(ollamaGenerationConfig)
                                .WithCustomEmbeddingGenerator(new OllamaEmbeddingGenerator(ollamaEmbeddingConfig) { })
                                .WithQdrantMemoryDb("http://localhost:6333")
                                .Build<MemoryServerless>();

            //實務上知識庫的資料與向量DB的維運與RAG應用是分開的，這裡只是為了Demo方便，所以一起放在同一個程式碼中
            await ImportKm(kernelMemory);

            Console.WriteLine("\n============ QA Start  (AskAsync) ===================\n");

            //AskAsync是經過參考向量檢索資料後，再做文本生成的結果，簡單來說就是答案
            var ans = await kernelMemory.AskAsync("What is NASA's plan for Discovery?");
            Console.WriteLine(ans.Result);
            foreach (var source in ans.RelevantSources)
            {
                Console.WriteLine($"source:{source.DocumentId}");
            }

            Console.ReadLine();
        }

        static async Task ImportKm(MemoryServerless memory)
        {
            await memory.ImportTextAsync(@"By Susan M. Niebur with David W. Brown, Editor
When it started in the early 1990s, NASA’s Discovery Program represented a breakthrough in the way NASA explores space. Providing opportunities for low-cost planetary science missions, the Discovery Program has funded a series of relatively small, focused, and innovative missions to investigate the planets and small bodies of our solar system.
For over 30 years, Discovery has given scientists a chance to dig deep into their imaginations and find inventive ways to unlock the mysteries of our solar system and beyond. As a complement to NASA’s larger “flagship” planetary science explorations, Discovery’s continuing goal is to achieve outstanding results by launching more, smaller missions using fewer resources and shorter development times.
This book draws on interviews with program managers, engineers, and scientists from Discovery’s early missions. It takes an in-depth look at the management techniques they used to design creative and cost-effective spacecraft that continue to yield ground-breaking scientific data, drive new technology innovations, and achieve what has never been done before.
", documentId: "nasa-ebook");
        }
    }
}
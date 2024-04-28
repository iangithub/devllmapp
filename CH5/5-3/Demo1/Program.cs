using Microsoft.SemanticKernel;

namespace IntroSample
{
    internal class Program
    {
        private const string deploy_Name = "xxxx";
        private const string aoai_Endpoint = "https://xxxx.openai.azure.com";
        private const string api_Key = "xxxxx";

        static async Task Main(string[] args)
        {
            Console.WriteLine("bot: 你想聽什麼主題的故事呢? \n");
            Console.Write("you: ");
            string storySubject = Console.ReadLine();
            Console.Write("\n");
            Console.WriteLine("bot: 故事的角色是什麼呢? \n");
            Console.Write("you: ");
            string storyRole = Console.ReadLine();
            Console.Write("\n");

            //use Azure OpenAI API
            var kernel = Kernel.CreateBuilder()
                .AddAzureOpenAIChatCompletion(
                 deploymentName: deploy_Name,   // Azure OpenAI Deployment Name
                 endpoint: aoai_Endpoint, // Azure OpenAI Endpoint
                 apiKey: api_Key  // Azure OpenAI Key
                ).Build();

            //use OpenAI API
            // var kernel = Kernel.CreateBuilder()
            //                 .AddOpenAIChatCompletion(
            //                  modelId: "xxxxx",   // OpenAI model ID
            //                  apiKey: "xxxxxxx"  // OpenAI Key
            //                 ).Build();


            // 從目錄內載入Plugin
            var pluginsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
            var plugin = kernel.ImportPluginFromPromptDirectory(Path.Combine(pluginsDirectory, "WriterPlugin"));
            KernelFunction fairyTablesFun = plugin["FairyTales"];

            // 置入 Prompt Template 參數值，並叫用GPT模型等得生成結果
            var result = (await kernel.InvokeAsync(
                            fairyTablesFun,
                            arguments: new()
                            {
                                { "story_subject", storySubject },
                                { "story_role", storyRole }
                            })
                        ).ToString();


            Console.WriteLine(result);

            Console.ReadLine();
        }
    }
}
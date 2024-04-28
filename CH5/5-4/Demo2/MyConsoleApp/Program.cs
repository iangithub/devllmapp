using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace IntroSample
{
    internal class Program
    {
        private const string deploy_Name = "xxxx";
        private const string aoai_Endpoint = "https://xxxx.openai.azure.com";
        private const string api_Key = "xxxxx";

        static async Task Main(string[] args)
        {

            var kernel = Kernel.CreateBuilder()
                .AddAzureOpenAIChatCompletion(
                 deploymentName: deploy_Name,   // Azure OpenAI Deployment Name
                 endpoint: aoai_Endpoint, // Azure OpenAI Endpoint
                 apiKey: api_Key  // Azure OpenAI Key
                ).Build();



            // Import the Plugin from the plugins directory.
            var pluginsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
            var plugin = kernel.ImportPluginFromPromptDirectory(Path.Combine(pluginsDirectory, "WriterPlugin"));
            KernelFunction fbFun = plugin["facebook"];


            //置入 Prompt Template 參數值
            KernelArguments arguments = new() { { "user_input", "ChatGPT對開發工程師的影響" } };


            //叫用GPT模型等待生成結果
            // var result = (await kernel.InvokeAsync(fbFun, arguments)).ToString();
            // Console.WriteLine(result);

            //使用 stream 模式顯示結果
            await foreach (var result in kernel.InvokeStreamingAsync(fbFun, arguments))
            {
                Console.Write(result);
            }

            Console.ReadLine();
        }
    }
}
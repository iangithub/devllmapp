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


            const string funcDefinition = @"
            你是一位facebook小編，請使用輕鬆灰諧的語氣，撰寫下列主題的貼文，內容500個字以內，#zh-tw
            ###
            {{$user_input}}
            ###
            ";


            //置入 Prompt Template 參數值
            KernelArguments arguments = new() { { "user_input", "ChatGPT對開發工程師的影響" } };

            //多參數範例
            // KernelArguments arguments = new() {
            //     { "parameter1", "xxxx" },
            //     { "parameter2", "xxxx" },
            //     { "parameter3", "xxxx" }
            //  };

            //動態配置API參數值
            //KernelArguments arguments = new(new OpenAIPromptExecutionSettings { MaxTokens = 500, Temperature = 0.5 }) { { "user_input", "ChatGPT對開發工程師的影響" } };

            //叫用GPT模型等待生成結果            
            //var result = (await kernel.InvokePromptAsync(funcDefinition, arguments)).ToString();
            //Console.WriteLine(result);

            //使用 stream 模式顯示結果
            await foreach (var result in kernel.InvokePromptStreamingAsync(funcDefinition, arguments))
            {
                Console.Write(result);
            }

            Console.ReadLine();
        }
    }
}
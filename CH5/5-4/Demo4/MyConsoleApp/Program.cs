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

            //Load Plugins
            kernel.Plugins.AddFromType<DataTimePlugin>();

            // Invoke native function get current date time
            var currentDateTime = await kernel.InvokeAsync<string>("DataTimePlugin", "GetCurrentDateTime");
            var currentDateTimePrompt = $"現在時間是{currentDateTime} \n";

            while (true)
            {

                Console.WriteLine("bot > 請輸入你的問題：");
                System.Console.Write("User > ");
                var user_q = Console.ReadLine();

                if (user_q == "exit")
                {
                    break;
                }

                //叫用GPT模型等待生成結果
                var result = (await kernel.InvokePromptAsync(currentDateTimePrompt + user_q)).ToString();
                System.Console.Write("Assistant > ");
                Console.WriteLine(result);

            }
            Console.ReadLine();
        }
    }
}
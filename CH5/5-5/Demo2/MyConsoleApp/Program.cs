using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using MyConsoleApp.Plugins;

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

            //Import the Plugin from Type
            kernel.Plugins.AddFromType<WeatherServicePlugin>();

            // Import the Plugin from the plugins directory.
            var pluginsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
            var plugin = kernel.ImportPluginFromPromptDirectory(Path.Combine(pluginsDirectory, "StylistPlugin"));
            KernelFunction styleFun = plugin["Style"];

            Console.WriteLine("bot > 請告訴我，你所在的城市，我將根據天氣資訊為你提供今日穿搭建議");
            Console.Write("User > ");
            string city = Console.ReadLine();

            KernelArguments arguments = new() { { "city", city } };

            // kernel.PromptRendered += (sender, args) =>
            // {
            //     Console.WriteLine("=========== PromptRendered Start ===========");
            //     Console.WriteLine(args.RenderedPrompt);
            //     Console.WriteLine("=========== PromptRendered End ===========\n\n");
            // };


            var result = (await kernel.InvokeAsync(styleFun, arguments)).ToString();
            Console.WriteLine(result);

            Console.ReadLine();
        }
    }
}
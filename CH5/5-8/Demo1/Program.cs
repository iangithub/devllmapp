using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace IntroSample
{
    internal class Program
    {

        static async Task Main(string[] args)
        {

            string model = "mistralai/Mixtral-8x7B-Instruct-v0.1";
            string apiKey = "xxxx";

            var promptTemplate = @"將以下內容翻譯成繁體中文  
            ==========
            {{$content}}  
            ==========
            中文翻譯：
            ";


            var content = @"1. When technology firm OpenAI ousted its CEO Sam Altman last week with little warning and less explanation, it set off shockwaves throughout Silicon Valley and beyond. ";

            //use HuggingFace
            Kernel kernel = Kernel.CreateBuilder()
             .AddHuggingFaceTextGeneration(model: model, apiKey: apiKey).Build();

            var promptFun = kernel.CreateFunctionFromPrompt(promptTemplate);
            var result = await kernel.InvokeAsync(promptFun, arguments: new() { { "content", content } });

            Console.Write(result);
            Console.ReadLine();
        }
    }
}
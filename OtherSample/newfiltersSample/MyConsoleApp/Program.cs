using Newtonsoft.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace IntroSample
{
    internal class Program
    {
        private const string deploy_Name = "xxxx";
        private const string aoai_Endpoint = "https://xxxx.openai.azure.com";
        private const string api_Key = "xxxx";


        static async Task Main(string[] args)
        {

            var kernel = Kernel.CreateBuilder()
                .AddAzureOpenAIChatCompletion(
                 deploymentName: deploy_Name,   // Azure OpenAI Deployment Name
                 endpoint: aoai_Endpoint, // Azure OpenAI Endpoint
                 apiKey: api_Key  // Azure OpenAI Key
                ).Build();


            kernel.PromptRenderFilters.Add(new MyPromptFilter());
            // kernel.FunctionInvocationFilters.Add(new MyFunctionFilter());

            // Import the Plugin from the plugins directory.
            var pluginsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
            var plugin = kernel.ImportPluginFromPromptDirectory(Path.Combine(pluginsDirectory, "WriterPlugin"));
            KernelFunction writerFun = plugin["Writer"];

            Console.WriteLine("bot > 今晚你創作什麼主題的短文呢？");
            Console.Write("User > ");
            string subject = Console.ReadLine();

            KernelArguments arguments = new() { { "fewshot_sample", Style1() }, { "post_subject", subject } };

            var result = (await kernel.InvokeAsync(writerFun, arguments)).ToString();
            Console.WriteLine($"\n\n ========= AI Result ===================");
            Console.WriteLine(result);
            Console.ReadLine();
        }

        static string Style1()
        {
            return @"
            竹籬上 停留著 蜻蜓
            玻璃瓶裡插滿 小小 森林
            青春 嫩綠的很 鮮明
            
            百葉窗 折射的 光影
            像有著心事的 一張 表情
            而你 低頭拆信 想知道關於我的事情
            
            月色搖晃樹影 穿梭在熱帶雨林
            你離去的原因從來不說明
            你的謊像陷阱我最後才清醒
            幸福只是水中的倒影
            
            月色搖晃樹影穿梭在熱帶雨林
            悲傷的雨不停全身血淋淋
            那深陷在沼澤我不堪的愛情
            是我無能為力的傷心
            
            蘆葦花開歲已寒 若霜又降路遙漫長
            牆外是誰在吟唱 鳳求凰
            梨園台上 西皮二黃
            卻少了妳 無人問暖
            誰在彼岸 天涯一方
            
            在夢裡我醞釀著情緒
            等回憶等那一種熟悉
            人世間最溫柔的消息
            是曾經被你擁入懷裡";
        }

    }


    class MyFunctionFilter : IFunctionInvocationFilter
    {
        public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
        {
            // before function invocation
            Console.WriteLine($"\n\n ========= before function invocation ===================");
            Console.WriteLine($"Invoking: {context.Function.Name} \n\n");
            // Console.WriteLine($"FunctionInvokingContext Arguments: {JsonConvert.SerializeObject(context.Arguments)}");
            // Console.WriteLine($"FunctionInvokingContext Metadata: {JsonConvert.SerializeObject(context.Metadata)}");

            await next(context);

            // after function invocation
            Console.WriteLine($"\n\n ========= after function invocation ===================");
            var metadata = context.Result.Metadata;
            if (metadata is not null && metadata.ContainsKey("Usage"))
            {
                Console.WriteLine($"Token usage: {JsonConvert.SerializeObject(metadata["Usage"])} \n\n");
            }
        }
    }

    class MyPromptFilter : IPromptRenderFilter
    {
        public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
        {
            Console.WriteLine($"\n\n ========= Rendering prompt ===================");
            Console.WriteLine($"Function Name : {context.Function.Name} \n\n");
            Console.WriteLine(JsonConvert.SerializeObject(context.Arguments));

            await next(context);

            Console.WriteLine($"\n\n ========= Rendered prompt ===================");
            Console.WriteLine(JsonConvert.SerializeObject(context.RenderedPrompt));
        }

    }

}
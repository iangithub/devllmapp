using Newtonsoft.Json;
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


            kernel.PromptFilters.Add(new MyPromptFilter());
            kernel.FunctionFilters.Add(new MyFunctionFilter());

            // Import the Plugin from the plugins directory.
            var pluginsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
            var plugin = kernel.ImportPluginFromPromptDirectory(Path.Combine(pluginsDirectory, "WriterPlugin"));
            KernelFunction writerFun = plugin["Writer"];

            Console.WriteLine("bot > 今晚你創作什麼主題的短文呢？");
            Console.Write("User > ");
            string subject = Console.ReadLine();

            KernelArguments arguments = new() { { "fewshot_sample", Style1() }, { "post_subject", subject } };
            //KernelArguments arguments = new() { { "fewshot_sample", Style2() }, { "post_subject", subject } };

            var result = (await kernel.InvokeAsync(writerFun, arguments)).ToString();
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

        static string Style2()
        {

            return @"和平分手以後 手應該 接什麼
            手放開 或是 守舊夢 夢什麼
            夢醒時分然後 分不清 是白晝
            還是夜 染黑了枕頭 偷走快樂
            
            樂極生悲以後 悲哀能 接什麼
            愛不愛 說來 太陳舊 就別說
            說到剩下沉默 莫非到了盡頭
            頭一次 我放棄爭辯 變木偶
            
            紅袖添香要安靜繁榮
            遊蕩一街已驚擾全城
            能讓我們長留在這家園
            要牽手報讀家政
            
            情像酒家靠仿舊馳名
            人像新聞有幾多長青
            惟願我們名字像對春聯
            對得久遠便相稱
            
            等她的笑 等她的愛
            等她等了不知不覺一千夜
            等她的吻 等她擁抱
            輾轉不覺花開等到花凋謝
            讓我每夜暖一些
            回憶日昨有一個落寞造夢者";

        }
    }


    class MyFunctionFilter : IFunctionFilter
    {
        public void OnFunctionInvoked(FunctionInvokedContext context)
        {
            //Console.WriteLine($"FunctionInvokedContext : {JsonConvert.SerializeObject(context)} \n\n");

            var metadata = context.Result.Metadata;

            if (metadata is not null && metadata.ContainsKey("Usage"))
            {
                Console.WriteLine($"Token usage: {JsonConvert.SerializeObject(metadata["Usage"])} \n\n");
            }
        }

        public void OnFunctionInvoking(FunctionInvokingContext context)
        {
            Console.WriteLine($"Invoking: {context.Function.Name} \n\n");
            // Console.WriteLine($"FunctionInvokingContext Arguments: {JsonConvert.SerializeObject(context.Arguments)}");
            // Console.WriteLine($"FunctionInvokingContext Metadata: {JsonConvert.SerializeObject(context.Metadata)}");
        }
    }

    class MyPromptFilter : IPromptFilter
    {
        public void OnPromptRendered(PromptRenderedContext context)
        {
            Console.WriteLine($"RenderedPrompt: {JsonConvert.SerializeObject(context.RenderedPrompt)} \n\n");
        }

        public void OnPromptRendering(PromptRenderingContext context)
        {
            Console.WriteLine($"Rendering prompt for {context.Function.Name} \n\n");
            //Console.WriteLine($"PromptRenderingContext Arguments: {JsonConvert.SerializeObject(context.Arguments)}");
        }
    }

}
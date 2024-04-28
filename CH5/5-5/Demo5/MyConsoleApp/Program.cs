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

            //add HttpPlugin
            kernel.Plugins.AddFromType<HttpPlugin>();

            // Import the Plugin from the plugins directory.
            var pluginsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
            var plugin = kernel.ImportPluginFromPromptDirectory(Path.Combine(pluginsDirectory, "SummarizePlugin"));
            KernelFunction summarizeFun = plugin["Summarize"];

            #region show kernel plugins and functions
            // var functions = kernel.Plugins.GetFunctionsMetadata();
            // Console.WriteLine("*****************************************");
            // Console.WriteLine("****** Registered plugins and functions ******");
            // Console.WriteLine("*****************************************");
            // Console.WriteLine();
            // foreach (KernelFunctionMetadata func in functions)
            // {
            //     Console.WriteLine($"Plugin: {func.PluginName} , func_Name >  {func.Name}, func_Description > {func.Description}");

            //     //列出參數
            //     if (func.Parameters.Count > 0)
            //     {
            //         Console.WriteLine("func_Params > ");
            //         foreach (var p in func.Parameters)
            //         {
            //             Console.WriteLine($"Parameter_Name > {p.Name}: {p.Description}");
            //             Console.WriteLine($"Parameter_DefaultValue > '{p.DefaultValue}'");
            //         }
            //     }
            // }
            // Console.WriteLine("=========================================\n\n");
            #endregion

            while (true)
            {
                Console.WriteLine("Assistant > 您好，請留下您的寶貴意見 \n");
                Console.Write("User > ");
                var user_Input = Console.ReadLine();
                Console.Write("\n");

                if (string.Compare(user_Input, "exit", true) == 0)
                {
                    Console.WriteLine("Assistant > bye........");
                    break;
                }

                var summarizeResult = (await kernel.InvokeAsync(summarizeFun, arguments: new() { { "user_input", user_Input } })).ToString();

                dynamic jsonObject = JsonConvert.DeserializeObject(summarizeResult);

                Console.WriteLine($"Assistant > 已收到您的回饋，【{jsonObject.summary}】，謝謝您的寶貴意見，我們將會持續改進以提供更好的服務。\n");

                //請確保另一個web api application 已經啟動
                var arguments = new KernelArguments() { { "url", "https://localhost:7000/api/CustomerFeedback" }, { "json", summarizeResult } };
                await kernel.InvokeAsync<string>("HttpPlugin", "Post", arguments);

            }
            Console.ReadLine();
        }
    }
}


/*
sample case

1.為了來阿里山，所以入住阿里山英迪格。飯店的位置有他先天的限制，座落在被民宅包圍，離阿里山森林遊樂區還有一小時的車程，對於幻想可以衝到祝山看日出的旅客，除非開車，否則是有難度的，因此，我們是抱著不看日出，隨遇而安的心情來的。必須讚美一下飯店的接駁行程，讓我們輕易的前往奮起湖，又能暢遊阿里山森林遊樂區，對於只靠雙腿的遊客真的幫助很大。飯店很新（廢話），很高科技，用AI可以播音樂，開燈，關電視之類的，適合懶人，前提是要先讓她聽得懂中文（大笑）。標準房的房間其實不算大，但也足夠四個人一起聊天哈啦。比較奇怪是無浴缸的房型，浴室也很大，感覺比例不太協調，導致走道很小，但缺點的反面是優點，上廁所就沒有壓迫感XDDD。早餐我覺得蠻不錯，提供的麵包類都很好吃，特別是馬告生吐司，各類可頌等，餐點總類不是最多，但也提供了多樣選擇，尤其是加入很多阿里山當地的食材。房間的零食飲料都是免費享用，這點非常體貼。不過，我還是忍不住要評論房間的地毯，尤其穿着襪子跟飯店拖鞋要特別當心，地毯與地板的高低差太容易被絆倒跟腳滑。

2.本來看到了負評有點擔心，實際入住後，發現很優質很滿意啊！入住的是七樓山景房，房間看出去遠眺山景，視野開闊。四點入住後到頂樓，有無邊際溫水游泳池和熱水池，提供毛巾浴袍和水，還有美麗的雲海相伴，晚上點燈更氣氛很迷人！晚餐是另外自費半自助式套餐，雖然一個人3千多不算便宜，但主食（我們點黑毛牛排）很好吃，自助式的餐點雖然不大但每道都很好吃，甚至有不少很新鮮的海鮮，廣島生蠔另人驚艷，非常肥美！頂樓就可以看日出也是賣點，不必一早4點就上祝山還不見得看得到，櫃台很貼心告知大約日出時刻，睡到時間差不多直接上頂樓就可以欣賞啦！早餐小而美，選擇雖然不是非常多，但細節用心，肉鬆不是用廉價豆粉而是有肉脯的，有現做蛋料理，麵類都很美味！終於有阿里山賓館外的好飯店可以選擇了，下次還會選擇英迪格來住！

3.來慶生覺得很雷訂了兩間房一間雙床房給長輩，另一間一張大床房給我們自己慶生，結果居然兩間都給我們雙床房，我確認過很多次確定自己是訂對的房型甚至事先打電話確認細節，結果還是出錯。原本以為是佈置好才會讓我們上去結果我們在房門口遇到來送蛋糕的員工整個就很尷尬。因為我們提早到，就在大廳等但沒有跟我們說茶飲在哪裡是我們自己看到自己拿但其他人就是裝好送過去。短髮櫃檯人員服務態度完全感覺不到是五星級飯店該有的樣子，非常冷淡而且其他服務人員告知其他房客的資訊也都沒有說，不知道是不是因為我們買專案價格所以差別待遇，總之感受很差。等電梯要等很久因為全部只有兩部，早餐晚餐還不錯但飲料選擇很少基本上只有鋁箔包的果汁跟鋁罐裝的飲料。入住時間是下午四點但check out時間是十一點，除了頂樓外沒什麼設施可以使用，入住二樓跟五樓基本上是完全沒有景而且房間偏小。結論大概就是沒有這個價位該有的服務品質跟景色，對整體體驗非常失望。

*/
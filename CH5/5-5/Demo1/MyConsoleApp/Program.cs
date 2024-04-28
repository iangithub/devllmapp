using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using MyConsoleApp.Plugins.CRM;

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
            kernel.Plugins.AddFromType<CustomerServicePlugin>();

            // Import the Plugin from the plugins directory.
            var pluginsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
            var plugin = kernel.ImportPluginFromPromptDirectory(Path.Combine(pluginsDirectory, "EmailPlugin"));
            KernelFunction writeEmailFun = plugin["WriteEmail"];

            Console.WriteLine("bot > 我是Email產生器小幫手，請輸入收信者姓名：");
            Console.Write("User > ");
            string customerName = Console.ReadLine();

            Console.WriteLine("bot > 請輸入Email內容主題：");
            Console.Write("User > ");
            string emailSubject = Console.ReadLine();

            //調用CRM系統查詢客戶資料
            var customer = await kernel.InvokeAsync<CustomerData>("CustomerServicePlugin", "QueryCustomerData", new KernelArguments { { "customerName", customerName } });

            if (customer is null)
            {
                Console.WriteLine("bot > CRM系統中找不到此客戶資料。");
            }
            else
            {
                //取得客戶資料後，將資料傳入EmailPlugin中的WriteEmail函數
                KernelArguments arguments = new() {
                    { "customer_name", customer.Name },
                    { "customer_email", customer.Email },
                    { "customer_gender", customer.Gender },
                    { "my_name", "Ian" },
                    { "email_subject", emailSubject }
                };

                //調用EmailPlugin中的WriteEmail函數
                var result = (await kernel.InvokeAsync(writeEmailFun, arguments)).ToString();
                Console.WriteLine(result);
            }
            Console.ReadLine();
        }
    }
}
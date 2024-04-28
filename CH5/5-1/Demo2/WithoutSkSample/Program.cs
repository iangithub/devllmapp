using Newtonsoft.Json;
using System.Text;
using WithoutSkSample.GPT4;

namespace WithoutSkSample
{
    internal class Program
    {
        //API授權金鑰
        const string api_Key = "{aoai_key}";
        const string aoai_Service_Name = "{aoai_service_name}";
        const string deployment_Name = "{aoai_deploy_name}";
        const string api_Version = "2023-07-01-preview";


        //使用 Chat Completions API 搭配 GPT-4 模型
        const string api_Endpoint = $"https://{aoai_Service_Name}.openai.azure.com/openai/deployments/{deployment_Name}/chat/completions?api-version={api_Version}";


        static async Task Main(string[] args)
        {
            //user input prompt
            Console.WriteLine("請輸入你想要問的問題：");
            var prompt = Console.ReadLine();

            //注入今天的日期
            //prompt += $"\n ### 今天的日期是{DateTime.Now}";

            try
            {

                using (HttpClient client = new HttpClient())
                {
                    //設定模型扮演的角色
                    var requestModel = new RequestModel("現在開始你是一位萬事通小幫手。");
                    //加入改造後的prompt
                    requestModel.AddUserMessages(prompt);

                    //API請求
                    var json = JsonConvert.SerializeObject(requestModel);
                    var data = new StringContent(json, Encoding.UTF8, "application/json");
                    client.DefaultRequestHeaders.Add("api-key", api_Key);
                    var response = await client.PostAsync(api_Endpoint, data);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    //API回應
                    var completion = JsonConvert.DeserializeObject<Completion>(responseContent);

                    //輸出模型生成結果
                    Console.WriteLine(completion.Choices[0].Message.Content);

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
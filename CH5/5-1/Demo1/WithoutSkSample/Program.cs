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
            var prompt = @"隨著科技的進步，人工智能（AI）在各個領域中的應用越來越廣泛，教育領域也不例外。
                    近年來，ChatGPT（生成式對話機器人）已經在教學領域中發揮了重要作用，
                    從而為教師和學生提供了更多的學習機會和資源。
                    本文將探討ChatGPT在教育領域的幾個主要應用，以及它如何改變了當代教育的面貌。
                    1. 輔助教學
                    ChatGPT可作為教師的助手，協助他們解答學生的疑問。這樣一來，教師便能專注於教授課程內容，
                    同時保證每位學生都能得到足夠的關注。此外，ChatGPT具有自然語言處理（NLP）功能，
                    能夠理解並回答各種問題，有助於學生在課堂以外的時間獲得即時反饋。
                    2. 個性化學習
                    ChatGPT具有強大的學習能力，能夠根據每個學生的需求和興趣提供個性化的學習計劃。
                    這意味著學生可以在自己的節奏下學習，避免了因跟不上課程進度而感到沮喪的情況。
                    此外，ChatGPT還能夠根據學生的學習情況給出建議，幫助他們在學習過程中取得更好的成果。
                    3. 語言學習助手
                    對於正在學習外語的學生，ChatGPT可以作為一個出色的語言學習助手。它可以與學生進行即時對話，
                    幫助他們練習口語和聽力技能。此外，ChatGPT還
                    能提供寫作建議，協助學生改進他們的寫作技巧。這樣的互動式學習方式對於提高學生的語言水平具有很大的幫助。
                    4. 在線評估與測試
                    ChatGPT可以自動生成各種題型的試題，為教師提供了一個簡單而有效的評估工具。
                    這不僅可以節省教師編制試題的時間，還能確保試題的多樣性和客觀性。
                    此外，ChatGPT還能夠進行自動評分，為教師提供及時的學生表現反饋。";

            //村上春樹風格-內容改寫優化
            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GPT4", "PromptTemplate.txt");

                // 以template方式，進行prompt改造
                string prompt_template = File.ReadAllText(filePath);
                prompt_template = prompt_template.Replace("{{user_prompt}}", prompt);

                using (HttpClient client = new HttpClient())
                {
                    //設定模型扮演的角色
                    var requestModel = new RequestModel("現在開始你是一位專欄作家。");
                    //加入改造後的prompt
                    requestModel.AddUserMessages(prompt_template);

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
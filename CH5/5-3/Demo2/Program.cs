using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

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

            // Create chat history
            ChatHistory history = new("你是一位英文翻譯專家，負責將中文翻譯成英文，並採用生活化用語的翻譯風格，避免使用過於冷門的單字");

            // Get chat completion service
            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            Console.WriteLine("請輸入中文句子，我會幫你翻譯成英文");

            // Start the conversation
            Console.Write("User > ");
            string? userInput;
            while ((userInput = Console.ReadLine()) != null)
            {
                history.AddUserMessage(userInput);

                // Get the response from the AI
                var result = chatCompletionService.GetStreamingChatMessageContentsAsync(
                                    history,
                                    kernel: kernel);

                // Stream the results
                string fullMessage = "";
                var first = true;
                await foreach (var content in result)
                {
                    if (content.Role.HasValue && first)
                    {
                        Console.Write("Assistant > ");
                        first = false;
                    }
                    Console.Write(content.Content);
                    fullMessage += content.Content;
                }
                Console.WriteLine();

                // Add the message from the agent to the chat history
                history.AddAssistantMessage(fullMessage);

                // Get user input again
                Console.Write("User > ");
            }

            Console.ReadLine();
        }
    }
}
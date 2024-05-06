using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace IntroSample
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

#pragma warning disable SKEXP0010
            var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(modelId: "llama3:8b", apiKey: null, endpoint: new Uri("http://localhost:11434"))
            .Build();

            // Create chat history
            ChatHistory history = new("你是一位萬能的AI助手");

            // Get chat completion service
            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            Console.WriteLine("Assistant > 我是AI助手，我能幫你什麼嗎？");

            // Start the conversation
            Console.Write("User > ");
            string? userInput;
            while ((userInput = Console.ReadLine()) != null)
            {
                history.AddUserMessage(userInput + "，請以中文回答");

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
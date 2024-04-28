using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
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

            // Import the Plugin from the plugins directory.
            using StreamReader reader = new(Path.Combine(Directory.GetCurrentDirectory(), "Plugins", "Chat.yaml"));
            KernelFunction prompt = kernel.CreateFunctionFromPromptYaml(
                reader.ReadToEnd(),
                promptTemplateFactory: new HandlebarsPromptTemplateFactory()
            );

            ChatHistory chatMessages = [];

            while (true)
            {
                System.Console.Write("Bot > 請輸入貼文主題 \n");
                // Get user input
                System.Console.Write("User > ");
                chatMessages.AddUserMessage(Console.ReadLine()!);

                var result = kernel.InvokeStreamingAsync<StreamingChatMessageContent>(
                                    prompt,
                                    arguments: new() { { "messages", chatMessages } });

                ChatMessageContent? chatMessageContent = null;

                await foreach (var content in result)
                {
                    System.Console.Write(content);
                    if (chatMessageContent == null)
                    {
                        System.Console.Write("Assistant > ");
                        chatMessageContent = new ChatMessageContent(
                            content.Role ?? AuthorRole.Assistant,
                            content.ModelId!,
                            content.Content!,
                            content.InnerContent,
                            content.Encoding,
                            content.Metadata);
                    }
                    else
                    {
                        chatMessageContent.Content += content;
                    }
                }
                System.Console.WriteLine("\n");

                chatMessages.Add(chatMessageContent!);
            }

            Console.ReadLine();
        }
    }
}
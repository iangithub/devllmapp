using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

public class BasicAgent
{
    private readonly Kernel _kernel;
    public BasicAgent()
    {
        _kernel = Kernel.CreateBuilder()
                .AddAzureOpenAIChatCompletion(
                    AppConfig.AzureOpenAIChatDeploymentName,
                    AppConfig.AzureOpenAIChatEndpoint,
                    AppConfig.AzureOpenAIChatApiKey
                ).Build();

        // Add the DateTimePlugin
        _kernel.Plugins.Add(KernelPluginFactory.CreateFromType<DateTimePlugin>());
    }

    public async Task ChatCompletionAgentAsync()
    {
        // Define the agent
        ChatCompletionAgent agent =
            new()
            {
                Name = "CommonAgent", //assistant name
                Instructions = @"You are a general assistant who can help answer user questions.
                Please use Traditional Chinese and Taiwanese when answering questions.", //system instructions
                Kernel = _kernel,
                ExecutionSettings = new OpenAIPromptExecutionSettings() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions },
            };
        ChatHistory chat = [];

        // Respond to user input
        await InvokeAgentAsync("你能提供台灣首都的基本資訊與介紹嗎？");
        await InvokeAgentAsync("那有什麼美食可以推薦呢？");
        await InvokeAgentAsync("那通常現在時節的天氣會是如何呢？"); // about date time question

        async Task InvokeAgentAsync(string input)
        {
            chat.Add(new ChatMessageContent(AuthorRole.User, input));

            await foreach (ChatMessageContent content in agent.InvokeAsync(chat))
            {
                chat.Add(content);

                Console.WriteLine($"# {content.Role} - {content.AuthorName ?? "*"}: '{content.Content}' \n\n");
            }
        }
    }

}
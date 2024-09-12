//2個RAG Agent，一個是道路安全法律Agent，另一個是勞基法Agent


using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

public class MultiRagAgent
{
    private readonly ILogger _logger;
    private readonly MemoryServerless _memory;
    public MultiRagAgent(ILogger logger)
    {
        //Embedding Model Config
        var aoaiEmbeddingConfig = new AzureOpenAIConfig
        {
            APIKey = AppConfig.AzureOpenAIChatApiKey,
            Deployment = AppConfig.AzureOpenAIEmbeddingDeploymentName,
            Endpoint = AppConfig.AzureOpenAIChatEndpoint,
            APIType = AzureOpenAIConfig.APITypes.ChatCompletion,
            Auth = AzureOpenAIConfig.AuthTypes.APIKey
        };

        var aoaiChatConfig = new AzureOpenAIConfig
        {
            APIKey = AppConfig.AzureOpenAIChatApiKey,
            Deployment = AppConfig.AzureOpenAIChatDeploymentName,
            Endpoint = AppConfig.AzureOpenAIChatEndpoint,
            APIType = AzureOpenAIConfig.APITypes.ChatCompletion,
            Auth = AzureOpenAIConfig.AuthTypes.APIKey
        };

        _memory = new KernelMemoryBuilder()
                                        .WithAzureOpenAITextGeneration(aoaiChatConfig)
                                        .WithAzureOpenAITextEmbeddingGeneration(aoaiEmbeddingConfig)
                                        .WithQdrantMemoryDb(AppConfig.Cloud_Qdrant_Endpoint, AppConfig.Cloud_Qdrant_AccessKey)
                                        .Build<MemoryServerless>();
    }

    private ChatCompletionAgent TrafficLawAgent()
    {
        var kernel = Kernel.CreateBuilder()
                       .AddAzureOpenAIChatCompletion(
                           AppConfig.AzureOpenAIChatDeploymentName,
                           AppConfig.AzureOpenAIChatEndpoint,
                           AppConfig.AzureOpenAIChatApiKey
                       ).Build();

        kernel.Plugins.AddFromType<RagTrafficLawPlugin>();
        kernel.AutoFunctionInvocationFilters.Add(new AutoFunctionInvocationFilter(_logger));

        ChatCompletionAgent agent = new()
        {
            Instructions = @"你是一位非常了解台灣交通法規的專家。
            你的任務是回答使用者有關交通法規的問題，回覆時必須是繁體中文，並且使用台灣用語。
            你只能根據得到的參考資料進行回答，當沒有參考資料時，就直接回覆：'很抱歉，目前法規資料沒有相關資訊，我無法回答'。

            請注意:
            你只能回答與台灣交通法規以及勞動法規相關的問題。
            你只能回答與台灣交通法規以及勞動法規相關的問題。
            你只能回答與台灣交通法規以及勞動法規相關的問題。
            ",
            Name = "Taiwan_Traffic_Law_specialist",
            Id = "Taiwan_Traffic_Law_specialist",
            Kernel = kernel
        };
        agent.ExecutionSettings = new OpenAIPromptExecutionSettings()
        {
            Temperature = 0.2
        };
        return agent;
    }

    private ChatCompletionAgent WorkerLawAgent()
    {
        var kernel = Kernel.CreateBuilder()
                       .AddAzureOpenAIChatCompletion(
                           AppConfig.AzureOpenAIChatDeploymentName,
                           AppConfig.AzureOpenAIChatEndpoint,
                           AppConfig.AzureOpenAIChatApiKey
                       ).Build();

        kernel.Plugins.AddFromType<RagWorkerLawPlugin>();
        kernel.AutoFunctionInvocationFilters.Add(new AutoFunctionInvocationFilter(_logger));

        var agent = new ChatCompletionAgent()
        {
            Instructions = @"你是一位非常了解台灣勞工法規的專家。
            你的任務是回答使用者有關勞工法規的問題，回覆時必須是繁體中文，並且使用台灣用語。
            你只能根據得到的參考資料進行回答，當沒有參考資料時，就直接回覆：'很抱歉，目前法規資料沒有相關資訊，我無法回答'。

            請注意:
            你只能回答與台灣交通法規以及勞動法規相關的問題。
            你只能回答與台灣交通法規以及勞動法規相關的問題。
            你只能回答與台灣交通法規以及勞動法規相關的問題。
            ",
            Name = "Taiwan_Worker_Law_specialist",
            Id = "Taiwan_Worker_Law_specialist",
            Kernel = kernel
        };
        agent.ExecutionSettings = new OpenAIPromptExecutionSettings()
        {
            Temperature = 0.2
        };
        return agent;
    }

    public async Task ChatCompletionAgentAsync()
    {
        var traffLawAgent = TrafficLawAgent();
        var workerLawAgent = WorkerLawAgent();

        AgentGroupChat agent = new(traffLawAgent, workerLawAgent) { };

        ChatHistory chat = [];
        var user_Input = string.Empty;

        Console.WriteLine("Agent : 您好，我是台灣法律專家，請問需要什麼服務：");
        while (true)
        {
            Console.Write("User : ");
            user_Input = Console.ReadLine();

            if (user_Input == "exit")
            {
                break;
            }

            // Invoke chat and display messages.
            agent.AddChatMessage(new ChatMessageContent(AuthorRole.User, user_Input));
            await foreach (ChatMessageContent content in agent.InvokeAsync())
            {
                chat.Add(content);
                Console.WriteLine($"# {content.Role} : '{content.Content}'");
            }

        }
    }

    public class AutoFunctionInvocationFilter(ILogger logger) : IAutoFunctionInvocationFilter
    {
        private readonly ILogger _logger = logger;

        public async Task OnAutoFunctionInvocationAsync(AutoFunctionInvocationContext context, Func<AutoFunctionInvocationContext, Task> next)
        {
            // Example: get function information
            logger.LogTrace("functionName: {functionName}", context.Function.Name);

            // // Example: get chat history
            //logger.LogTrace("ChatHistory: {ChatHistory}", JsonSerializer.Serialize(context.ChatHistory));

            // // Example: get information about all functions which will be invoked
            var functionCalls = FunctionCallContent.GetFunctionCalls(context.ChatHistory.Last()).ToList();

            functionCalls.ForEach(functionCall
                            => logger.LogTrace(
                                "Function call requests: {PluginName}-{FunctionName}",
                                functionCall.PluginName,
                                functionCall.FunctionName));

            await next(context);

            logger.LogTrace("Function {FunctionName} succeeded.", context.Function.Name);
            // logger.LogTrace("Function result: {Result}", context.Result.ToString());

            // var usage = context.Result.Metadata?["Usage"];
            // if (usage is not null)
            // {
            //     logger.LogTrace("Usage: {Usage}", JsonSerializer.Serialize(usage));
            // }
        }
    }

    public async Task ImportKm()
    {
        Console.WriteLine("=== Import Start ===");

        await _memory.ImportDocumentAsync(Path.Combine(Directory.GetCurrentDirectory(), "docs", "道路交通管理處罰條例.pdf")
        , documentId: "taiwan_traffic_law", index: "taiwan_traffic_law");

        await _memory.ImportDocumentAsync(Path.Combine(Directory.GetCurrentDirectory(), "docs", "勞動基準法.pdf")
        , documentId: "taiwan_worker_law", index: "taiwan_worker_law");

        Console.WriteLine("=== Import Done ===");
    }

}


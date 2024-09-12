namespace RagAgentLinebot.Models
{
    using Azure.AI.OpenAI;
    using DocumentFormat.OpenXml.Wordprocessing;
    using Microsoft.Extensions.Configuration;
    //2個RAG Agent，一個是道路安全法律Agent，另一個是勞基法Agent
    using Microsoft.Extensions.Logging;
    using Microsoft.Identity.Client;
    using Microsoft.KernelMemory;
    using Microsoft.SemanticKernel;
    using Microsoft.SemanticKernel.Agents;
    using Microsoft.SemanticKernel.Agents.Chat;
    using Microsoft.SemanticKernel.ChatCompletion;
    using Microsoft.SemanticKernel.Connectors.OpenAI;
    using static RagAgentLinebot.Models.MultiRagAgent;

    public class MultiRagAgent
    {
        private readonly ILogger<MultiRagAgent> _logger;
        private readonly MemoryServerless _memory;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly Kernel _kernel;
        private const string InnerSelectionInstructions =
        $$$"""
        Select which participant will take the next turn based on the conversation history.
        
        Only choose from these participants:
        - Taiwan_Traffic_Law_specialist
        - Taiwan_Worker_Law_specialist
        
        Respond in JSON format. The JSON schema can include only:
        {
            "name": "string (the name of the assistant selected for the next turn)",
            "reason": "string (the reason for the participant was selected)"
        }
        
        History:
        {{${{{KernelFunctionSelectionStrategy.DefaultHistoryVariableName}}}}}
        """;

        private const string OuterTerminationInstructions =
        $$$"""
        Determine if user request has been fully answered.
        
        Respond in JSON format.  The JSON schema can include only:
        {
            "isAnswered": "bool (true if the user request has been fully answered)",
            "reason": "string (the reason for your determination)"
        }
        
        History:
        {{${{{KernelFunctionTerminationStrategy.DefaultHistoryVariableName}}}}}
        """;

        public MultiRagAgent(ILogger<MultiRagAgent> logger, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            //Embedding Model Config
            var aoaiEmbeddingConfig = new AzureOpenAIConfig
            {
                APIKey = configuration["AzureOpenAIChatApiKey"],
                Deployment = configuration["AzureOpenAIEmbeddingDeploymentName"],
                Endpoint = configuration["AzureOpenAIChatEndpoint"],
                APIType = AzureOpenAIConfig.APITypes.ChatCompletion,
                Auth = AzureOpenAIConfig.AuthTypes.APIKey
            };

            var aoaiChatConfig = new AzureOpenAIConfig
            {
                APIKey = configuration["AzureOpenAIChatApiKey"],
                Deployment = configuration["AzureOpenAIChatDeploymentName"],
                Endpoint = configuration["AzureOpenAIChatEndpoint"],
                APIType = AzureOpenAIConfig.APITypes.ChatCompletion,
                Auth = AzureOpenAIConfig.AuthTypes.APIKey
            };

            _memory = new KernelMemoryBuilder()
                                            .WithAzureOpenAITextGeneration(aoaiChatConfig)
                                            .WithAzureOpenAITextEmbeddingGeneration(aoaiEmbeddingConfig)
                                            .WithQdrantMemoryDb(_configuration["Cloud_Qdrant_Endpoint"], _configuration["Cloud_Qdrant_AccessKey"])
                                            .Build<MemoryServerless>();
            _kernel = Kernel.CreateBuilder()
                          .AddAzureOpenAIChatCompletion(
                              _configuration["AzureOpenAIChatDeploymentName"],
                              _configuration["AzureOpenAIChatEndpoint"],
                              _configuration["AzureOpenAIChatApiKey"]
                          ).Build();
        }


        private ChatCompletionAgent TrafficLawAgent()
        {
            var kernel = Kernel.CreateBuilder()
                           .AddAzureOpenAIChatCompletion(
                               _configuration["AzureOpenAIChatDeploymentName"],
                               _configuration["AzureOpenAIChatEndpoint"],
                               _configuration["AzureOpenAIChatApiKey"]
                           ).Build();

            kernel.Plugins.AddFromType<RagTrafficLawPlugin>(serviceProvider: _serviceProvider);
            kernel.AutoFunctionInvocationFilters.Add(new AutoFunctionInvocationFilter(_logger));

            ChatCompletionAgent agent = new()
            {
                Instructions = 
                """
                你是一位非常了解台灣交通法規的專家。
                你的任務是回答使用者有關交通法規的問題，回覆時必須是繁體中文，並且使用台灣用語。

                請注意:
                你只能回答與台灣交通法規以及勞動法規相關的問題。
                你只能回答與台灣交通法規以及勞動法規相關的問題。
                你只能回答與台灣交通法規以及勞動法規相關的問題。
                """,
                Name = "Taiwan_Traffic_Law_Agent",
                Id = "Taiwan_Traffic_Law_Agent",
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
                               _configuration["AzureOpenAIChatDeploymentName"],
                               _configuration["AzureOpenAIChatEndpoint"],
                               _configuration["AzureOpenAIChatApiKey"]
                           ).Build();
            kernel.Plugins.AddFromType<RagWorkerLawPlugin>(serviceProvider: _serviceProvider);
            kernel.AutoFunctionInvocationFilters.Add(new AutoFunctionInvocationFilter(_logger));

            var agent = new ChatCompletionAgent()
            {
                Instructions = 
                """
                你是一位非常了解台灣勞工法規的專家。
                你的任務是回答使用者有關勞工法規的問題，回覆時必須是繁體中文，並且使用台灣用語。

                請注意:
                你只能回答與台灣交通法規以及勞動法規相關的問題。
                你只能回答與台灣交通法規以及勞動法規相關的問題。
                你只能回答與台灣交通法規以及勞動法規相關的問題。
                """,
                Name = "Taiwan_Worker_Law_Agent",
                Id = "Taiwan_Worker_Law_Agent",
                Kernel = kernel
            };
            agent.ExecutionSettings = new OpenAIPromptExecutionSettings()
            {
                Temperature = 0.2
            };
            return agent;
        }

        public async Task<string> ChatCompletionAgentAsync(string user_Input)
        {
            var traffLawAgent = TrafficLawAgent();
            var workerLawAgent = WorkerLawAgent();

            AgentGroupChat agent = new(traffLawAgent, workerLawAgent) { };

            ChatHistory chat = [];

            // Invoke chat and display messages.
            agent.AddChatMessage(new ChatMessageContent(AuthorRole.User, user_Input));
            var response = string.Empty;
            await foreach (ChatMessageContent content in agent.InvokeAsync())
            {
                chat.Add(content);

                response = content.Content;
            }
            return response;
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

        private sealed record AgentSelectionResult(string name, string reason);
    }

    class ApprovalTerminationStrategy : TerminationStrategy
    {
        protected override Task<bool> ShouldAgentTerminateAsync(Agent agent, IReadOnlyList<ChatMessageContent> history, CancellationToken cancellationToken)
            => Task.FromResult(history[history.Count - 1].Content?.Contains("approve", StringComparison.OrdinalIgnoreCase) ?? false);
    }
}

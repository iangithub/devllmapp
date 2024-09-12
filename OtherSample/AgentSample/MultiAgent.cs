using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;

public class MultiAgent
{
    private readonly Kernel _kernel;
    private readonly ILogger _logger;
    public MultiAgent(ILogger logger)
    {
        _kernel = Kernel.CreateBuilder()
                .AddAzureOpenAIChatCompletion(
                    AppConfig.AzureOpenAIChatDeploymentName,
                    AppConfig.AzureOpenAIChatEndpoint,
                    AppConfig.AzureOpenAIChatApiKey
                ).Build();

        _kernel.AutoFunctionInvocationFilters.Add(new AutoFunctionInvocationFilter(logger));
    }

    private ChatCompletionAgent GetWriterAgent()
    {
        ChatCompletionAgent WriterAgent =
                    new()
                    {
                        Instructions = @"你是一位非常理解村上春樹寫作風格的研究者，擅長模擬村上春樹寫作風格，進行短文章的創作。
                        你的任務是根據使用者的需求，創作一篇符合村上春樹寫作風格的文章，文章內容必須符合繁體中文，並且使用台灣用語。",
                        Name = "Writer",
                        Kernel = _kernel
                    };
        return WriterAgent;
    }

    private ChatCompletionAgent GetReviewerAgent()
    {
        ChatCompletionAgent ReviewerAgent =
                    new()
                    {
                        Instructions = @"你是一位非常理解村上春樹寫作風格的研究者，你的任務如下

                        ##任務
                        1.審查文章，確保文章風格是符合村上春樹的寫作風格。
                        2.文章內容必須符合使者所要求的主題。
                        3.不可以有抄襲村上春樹內容的情況。
                        4.文章內容必須符合繁體中文，並且使用台灣用語。
                        
                        ### Important Reminder
                        如果你發現文章內容抄襲了村上春樹，必須做「reject」，這很重要，
                        如果你沒有做到，將會被視為工作失誤，並且您將會被解雇。

                        一旦符合所有審核要求，你只需回覆「approve」即可，如果未通過審查，請回覆「reject」。",
                        Name = "Reviewer",
                        Kernel = _kernel
                    };
        return ReviewerAgent;
    }

    private ChatCompletionAgent GetTranslateReviewerAgent()
    {
        ChatCompletionAgent TranslateReviewerAgent =
                    new()
                    {
                        Instructions = @"你是一位中英文翻譯專家，你的任務是審查中英的翻譯內容，
                        必須確保翻譯後的品質符合原文的意思。
                        一旦符合所有審核要求，你只需回覆「approve」即可，如果未通過審查，請回覆「reject」。",
                        Name = "TranslateReviewer",
                        Kernel = _kernel
                    };
        return TranslateReviewerAgent;
    }

    private ChatCompletionAgent GetManagerAgent()
    {
        ChatCompletionAgent ManagerAgent =
                    new()
                    {
                        Instructions = @"您是總編輯,負責審查文章內容,所有提交的文章內容必須符合'TranslateReviewer'以及'Reviewer'的認可
                        ，才能同意出版，您需要綜合他們的建議，一旦都完成，你只需要回覆「approve」即可，如果未通過審查，請回覆「reject」。",
                        Name = "Manager",
                        Kernel = _kernel
                    };
        return ManagerAgent;
    }

    public async Task ChatCompletionAgentAsync()
    {
        var writerAgent = GetWriterAgent();
        var reviewerAgent = GetReviewerAgent();
        var translateReviewerAgent = GetTranslateReviewerAgent();
        var managerAgent = GetManagerAgent();

        // Define the agent group chat
        AgentGroupChat chat =
            new(writerAgent, reviewerAgent, translateReviewerAgent, managerAgent)
            {
                ExecutionSettings =
                    new()
                    {
                        TerminationStrategy =
                            new ApprovalTerminationStrategy()
                            {
                                Agents = [managerAgent],
                                MaximumIterations = 5,
                                AutomaticReset = true
                            }
                    }
            };


        // Invoke chat and display messages.
        string input = @"中秋節即將到來，請創作一篇短文，用來表達中秋佳節的祝福，並且我希望你能提供中文及英文的版本，
        二個版本的內容要相同的，只是經過翻譯，不可以失去原意。";

        chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, input));

        await foreach (var content in chat.InvokeAsync())
        {
            Console.WriteLine($"# {content.Role} - {content.AuthorName ?? "*"}: '{content.Content}'");
        }
    }

    private class ApprovalTerminationStrategy : TerminationStrategy
    {
        // Terminate when the final message contains the term "approve"
        protected override Task<bool> ShouldAgentTerminateAsync(Agent agent, IReadOnlyList<ChatMessageContent> history, CancellationToken cancellationToken)
            => Task.FromResult(history[history.Count - 1].Content?.Contains("approve", StringComparison.OrdinalIgnoreCase) ?? false);
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

}


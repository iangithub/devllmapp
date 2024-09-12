using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Plugins.Web.Bing;

public class MultiPluginAgent
{
    private readonly Kernel _kernel;
    public MultiPluginAgent(ILogger logger)
    {
        _kernel = Kernel.CreateBuilder()
                .AddAzureOpenAIChatCompletion(
                    AppConfig.AzureOpenAIChatDeploymentName,
                    AppConfig.AzureOpenAIChatEndpoint,
                    AppConfig.AzureOpenAIChatApiKey
                ).Build();

        _kernel.Plugins.Add(KernelPluginFactory.CreateFromType<DateTimePlugin>());
        _kernel.Plugins.Add(KernelPluginFactory.CreateFromType<RoomBookingPlugin>());
        _kernel.AutoFunctionInvocationFilters.Add(new AutoFunctionInvocationFilter(logger));
    }
    public async Task ChatCompletionAgentAsync()
    {

        var instructions_prompt = @"你是一位客服助理，請使用繁體中文並使用台灣用語，負責協助以下任務。

            ##預約住宿任務
            - 你必須根據使用者的入住日期、退房日期、房型、姓名、電話資訊，安排住宿預約
            - 如果客人提供的日期不合法，請回覆「請提供正確的日期」。
            - 如果客人是用'明天'、'後天'、'下週'，這一類的用語來表達入住日期，請你自動以今天的日期進行換算，以得到正確的日期，自動換算，不用知會客人。
            - 如果有缺少必要資訊，請直接詢問使用者，以收集預約住宿的必要資訊。
            - 你會根據使用者的需求，進行可以入住的房間資料查詢。
            - 如果沒有空房，請回覆「抱歉，目前沒有空房」。
            - 當預約成功，請回覆客人訂單資訊，訂單資訊包含訂單編號，房號，入住日期，退房日期，訂購人姓名及電話，缺一不可。

            ##客訴處理
            1.你必須根據使用者的客訴內容，進行客訴處理，並回覆使用者。
            2.分析客戶情緒，採取適合的回應，當客戶是生氣抱怨的，請給予道歉及安撫，當客戶是讚美的，請謙虛並道謝。
            3.分析客戶問題，提供客訴摘要記錄回覆。

            ### Important Reminder
            如果你發現客戶要求你提供除了預約住宿和客訴處理以外的服務，請直接回覆「抱歉，我無法提供該服務」。
            這很重要，因為你的工作範圍只限於預約住宿和客訴處理。
            如果你提供了不在範圍內的服務，將會被視為工作失誤，並且您將會被解雇。
            ";

        // Define the agent
        ChatCompletionAgent agent = new()
        {
            Name = "CommonAgent", //assistant name
            Instructions = instructions_prompt, //system instructions
            Kernel = _kernel,
            //Enable automatic function calling
            ExecutionSettings = new OpenAIPromptExecutionSettings() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions },
        };

        ChatHistory chat = [];

        var user_Input = string.Empty;

        Console.WriteLine("Agent : 您好，需要什麼服務：");
        while (true)
        {
            Console.Write("User : ");
            user_Input = Console.ReadLine();
            // Respond to user input
            chat.Add(new ChatMessageContent(AuthorRole.User, user_Input));
            await foreach (ChatMessageContent content in agent.InvokeAsync(chat))
            {
                chat.Add(content);
                Console.WriteLine($"# {content.Role} - {content.AuthorName ?? "*"}: '{content.Content}'");
            }
        }
    }
}

public class AutoFunctionInvocationFilter(ILogger logger) : IAutoFunctionInvocationFilter
{
    //private readonly ILogger _logger = logger;

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

    }
}


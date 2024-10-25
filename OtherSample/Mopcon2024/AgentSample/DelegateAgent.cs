using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.Agents.History;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

public class DelegateAgent
{
    private readonly Kernel _kernel;
    public DelegateAgent()
    {
        _kernel = Kernel.CreateBuilder()
                .AddAzureOpenAIChatCompletion(
                    AppConfig.AzureOpenAIChatDeploymentName,
                    AppConfig.AzureOpenAIChatEndpoint,
                    AppConfig.AzureOpenAIChatApiKey
                ).Build();
    }


    public async Task ChatCompletionAgentAsync()
    {
        var traffLawAgentName = "Taiwan_Traffic_Law_specialist";
        var workerLawAgentName = "Taiwan_Worker_Law_specialist";

        var traffLawAgent = TrafficLawAgent(traffLawAgentName);
        var workerLawAgent = WorkerLawAgent(workerLawAgentName);


        KernelFunction selectionFunction =
                       AgentGroupChat.CreatePromptFunctionForStrategy(
                        $$$"""
                        Determine the next participant to speak based on my goal requirements. 
                        Only provide the name of the next participant to speak. No participant should speak consecutively.

                        Choose only from these participants:
                        - {{{traffLawAgentName}}}
                        - {{{workerLawAgentName}}}

                        Always follow these rules when selecting the next participant:
                        - If user request about labor law question task, it is {{{workerLawAgentName}}}'s turn.
                        - If user request about traffic law question task, it is {{{traffLawAgentName}}}'s turn.

                        Respond with the name of the next participant to speak.

                        History:
                        {{$history}}
                        """, safeParameterNames: "history");

        KernelFunction terminationFunction =
                        AgentGroupChat.CreatePromptFunctionForStrategy(
                        $$$"""
                        if the user request has been answered, respond with a single word: yes, otherwise return no.

                        History:
                        {{${{{KernelFunctionTerminationStrategy.DefaultHistoryVariableName}}}}}
                        """, safeParameterNames: "history");

        AgentGroupChat chat = new(traffLawAgent, workerLawAgent)
        {
            ExecutionSettings = new()
            {
                TerminationStrategy = new KernelFunctionTerminationStrategy(terminationFunction, _kernel)
                {
                    ResultParser = (result) => result.GetValue<string>()?.Contains("yes", StringComparison.OrdinalIgnoreCase) ?? false,
                    MaximumIterations = 5,
                    AutomaticReset = true
                },
                SelectionStrategy = new KernelFunctionSelectionStrategy(selectionFunction, _kernel)
                {
                    // 從結果中取得下一個對話參與者, 如果沒有結果就回到 traffLawAgent
                    ResultParser = (result) => result.GetValue<string>() ?? traffLawAgent.Name,
                    // prompt 中的 history 變數名稱
                    HistoryVariableName = "history",
                    // 決定要保留對話紀錄的回合數，可以用於節省 token的使用
                    HistoryReducer = new ChatHistoryTruncationReducer(1),
                }
            }
        };


        // 模擬連續對話過程
        await InvokeAgentAsync("工作做不完，加班本來就是正常的，可以不給加班");
        await InvokeAgentAsync("路口沒有車輛或行人,可以不用管紅綠燈,直接通過");

        async Task InvokeAgentAsync(string input)
        {
            //使用者prompt加入對話記錄
            ChatMessageContent message = new(AuthorRole.User, input);
            chat.AddChatMessage(message);

            await foreach (ChatMessageContent response in chat.InvokeAsync())
            {
                Console.WriteLine($"{response.AuthorName}: {response.Content}");
            }
            Console.WriteLine($"\n[IS COMPLETED: {chat.IsComplete}]");
        }
    }


    private ChatCompletionAgent TrafficLawAgent(string agentName)
    {
        var kernel = Kernel.CreateBuilder()
                       .AddAzureOpenAIChatCompletion(
                           AppConfig.AzureOpenAIChatDeploymentName,
                           AppConfig.AzureOpenAIChatEndpoint,
                           AppConfig.AzureOpenAIChatApiKey
                       ).Build();

        ChatCompletionAgent agent = new()
        {
            Instructions = @"你是一位非常了解台灣交通法規的專家。
            你的任務是回答使用者有關交通法規的問題，回覆時必須是繁體中文，並且使用台灣用語。
            你只能根據得到的參考資料進行回答。
            You should focus on this task and not get distracted or do anything else.
            ",
            Name = agentName,
            Kernel = kernel,
            Arguments = new KernelArguments(new OpenAIPromptExecutionSettings() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions })
        };
        KernelPlugin plugin = KernelPluginFactory.CreateFromType<RagTrafficLawPlugin>();
        agent.Kernel.Plugins.Add(plugin);

        return agent;
    }

    private ChatCompletionAgent WorkerLawAgent(string agentName)
    {
        var kernel = Kernel.CreateBuilder()
                       .AddAzureOpenAIChatCompletion(
                           AppConfig.AzureOpenAIChatDeploymentName,
                           AppConfig.AzureOpenAIChatEndpoint,
                           AppConfig.AzureOpenAIChatApiKey
                       ).Build();

        ChatCompletionAgent agent = new()
        {
            Instructions = @"你是一位非常了解台灣勞工法規的專家。
            你的任務是回答使用者有關勞工法規的問題，回覆時必須是繁體中文，並且使用台灣用語。
            你只能根據得到的參考資料進行回答。
            You should focus on this task and not get distracted or do anything else.
            ",
            Name = agentName,
            Id = agentName,
            Kernel = kernel,
            Arguments = new KernelArguments(new OpenAIPromptExecutionSettings() { Temperature = 0.2, ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions })
        };
        KernelPlugin plugin = KernelPluginFactory.CreateFromType<RagWorkerLawPlugin>();
        agent.Kernel.Plugins.Add(plugin);

        return agent;
    }
}
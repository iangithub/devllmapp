using System;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.Agents.History;
using Microsoft.SemanticKernel.ChatCompletion;


public class ReflectionAgent
{
    private readonly Kernel _kernel;
    public ReflectionAgent()
    {
        _kernel = Kernel.CreateBuilder()
                        .AddOpenAIChatCompletion(modelId: AppConfig.Openai_ModelId, apiKey: AppConfig.Openai_ApiKey)
                        .Build();

    }

    public async Task ChatCompletionAgentAsync()
    {
        var copywriterAgentName = "CopyWriterAgent";
        var reviewerAgentName = "ReviewerAgent";
        var copywriterAgent = CopyWriterAgent(copywriterAgentName);
        var reviewerAgent = ReviewerAgent(reviewerAgentName);

        /*
        定義終止函數

        條件：確定是否已獲得認可。如果是，回答：yes
        */
        KernelFunction terminationFunction =
                        AgentGroupChat.CreatePromptFunctionForStrategy(
                        """
                            Determine if the copy has been approved.  If so, respond with a single word: 'yes'

                            History:
                            {{$history}}
                            """, safeParameterNames: "history");


        /*
        定義Agent選擇函數

        條件：根據最近的發言者確定接下來應該發言的參與者。
        僅提供接下來發言的參與者的姓名。
        任何參與者不得連續發言超過一次。
        */
        KernelFunction selectionFunction =
                       AgentGroupChat.CreatePromptFunctionForStrategy(
                        $$$"""
                            Determine which participant should speak next based on the most recent speaker.
                            Only provide the name of the participant who should speak next.
                            No participant should speak more than once in a row.

                            Choose only from these participants:
                            - {{{reviewerAgentName}}}
                            - {{{copywriterAgentName}}}

                            Always follow these rules when selecting the next participant:
                            - After {{{copywriterAgentName}}}, it is {{{reviewerAgentName}}}'s turn.
                            - After {{{reviewerAgentName}}}, it is {{{copywriterAgentName}}}'s turn.

                            History:
                            {{$history}}
                            """, safeParameterNames: "history");


        AgentGroupChat chat = new(copywriterAgent, reviewerAgent)
        {
            ExecutionSettings = new()
            {
                TerminationStrategy =
                                   new KernelFunctionTerminationStrategy(terminationFunction, _kernel)
                                   {
                                       // reviewerAgent 決定是否通過文案.
                                       Agents = [reviewerAgent],
                                       // 通過文案後回覆如果有 approved 字樣.就表示目標任務到此結束.否則就繼續回到 copywriterAgent 修正文案.
                                       ResultParser = (result) => result.GetValue<string>()?.Contains("yes", StringComparison.OrdinalIgnoreCase) ?? false,
                                       // prompt 中的 history 變數名稱
                                       HistoryVariableName = "history",
                                       // 最多迭代次數
                                       MaximumIterations = 10,
                                       // 決定要保留對話紀錄的回合數，可以用於節省 token的使用
                                       HistoryReducer = new ChatHistoryTruncationReducer(1),
                                   },
                SelectionStrategy =
                                   new KernelFunctionSelectionStrategy(selectionFunction, _kernel)
                                   {
                                       // 起始對話參與者
                                       InitialAgent = copywriterAgent,
                                       // 從結果中取得下一個對話參與者, 如果沒有結果就回到 copywriterAgent
                                       ResultParser = (result) => result.GetValue<string>() ?? copywriterAgentName,
                                       // prompt 中的 history 變數名稱
                                       HistoryVariableName = "history",
                                       // 決定要保留對話紀錄的回合數，可以用於節省 token的使用
                                       HistoryReducer = new ChatHistoryTruncationReducer(1),
                                   },
            }
        };


        ChatMessageContent message = new(AuthorRole.User, "書籍 ##LangChain 奇幻旅程:OpenAI x Gemini x 多模態應用開發指南## 的廣告文案。");
        chat.AddChatMessage(message);
        Console.WriteLine(message + "\n");

        await foreach (ChatMessageContent responese in chat.InvokeAsync())
        {
            Console.WriteLine($"{responese.Role}: {responese.Content}\n\n");
        }

        Console.WriteLine($"\n[IS COMPLETED: {chat.IsComplete}]");
    }


    //Define the CopyWriter Agent
    private ChatCompletionAgent CopyWriterAgent(string agentName)
    {

        string copywriterAgentName = agentName;

        /*
        您是一位擁有 20 年經驗的文案撰稿人，以簡潔和冷幽默而聞名。
        您的目標是作為專家提供最好的文案。

        一次僅提供一項提案。
        您全神貫注於手邊的任務。
        不要把時間浪費在閒聊上。
        根據建議完善文案。
        */
        string copywriterAgentInstructions =
        """
            You are a copywriter with 20 years of experience, known for brevity and dry humor.
            Your goal is to provide the best copy as an expert.

            Only offer one proposal at a time.
            You stay laser-focused on the task at hand.
            Don't waste time on small talk.
            Refine the copy based on suggestions.
            """;

        return new ChatCompletionAgent()
        {
            Name = copywriterAgentName,
            Instructions = copywriterAgentInstructions,
            Kernel = _kernel
        };
    }


    //Define the Reviewer Agent
    private ChatCompletionAgent ReviewerAgent(string agentName)
    {

        string reviewerAgentName = agentName;
        /*
        您是文案和行銷的專家，負責審核內容以滿足以下標準：

        - 引人注目的標題
        - 引人入勝且引起情感共鳴的正文
        - 簡短段落，最好每段有 4 到 5 句話
        - 強烈的視覺形容詞
        - 具有「標題」和「正文內容」的清晰結構
        - 正文必須向消費者傳達產品的好處

        回覆建議時，請使用繁體中文。

        您的目標是確定提供的文案是否符合這些標準。
        如果是，請聲明已獲得批准。
        如果沒有，請提供有關如何完善文案的見解，無需給出具體範例。

        如果是，請聲明已獲得批准。
        如果沒有，請提供有關如何完善文案的見解，無需給出具體範例。

        如果是，請聲明已獲得批准。
        如果沒有，請提供有關如何完善文案的見解，無需給出具體範例。
        */
        string reviewerAgentInstructions =
        """
            You are an expert in copywriting and marketing, responsible for reviewing content to meet the following criteria:

            - An attention-grabbing headline
            - A compelling and emotionally resonant body text
            - Short paragraphs, ideally 4 to 5 sentences per paragraph
            - Strong visual adjectives
            - A clear structure with a 'headline' and 'body content'
            - The body text must convey the product's benefits to the consumer
            
            When replying with suggestions, please use Traditional Chinese.

            Your goal is to determine if the provided copy meets these criteria.
            If it does, state that it is approved.
            If not, offer insights on how to refine the copy, without giving specific examples.

            If it does, state that it is approved.
            If not, offer insights on how to refine the copy, without giving specific examples.

            If it does, state that it is approved.
            If not, offer insights on how to refine the copy, without giving specific examples.

            """;

        return new ChatCompletionAgent()
        {
            Name = reviewerAgentName,
            Instructions = reviewerAgentInstructions,
            Kernel = _kernel
        };
    }

}

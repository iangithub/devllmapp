using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Planning.Handlebars;
using MyConsoleApp.Plugins;

namespace IntroSample
{
    internal class Program
    {
        private const string deploy_Name = "gpt4o";
        private const string aoai_Endpoint = "https://demo23.openai.azure.com";
        private const string api_Key = "6836c79a107c49c0993d7f0d24fef10d";


        static async Task Main(string[] args)
        {
            var kernel = Kernel.CreateBuilder()
                            .AddAzureOpenAIChatCompletion(
                             deploymentName: deploy_Name,   // Azure OpenAI Deployment Name
                             endpoint: aoai_Endpoint, // Azure OpenAI Endpoint
                             apiKey: api_Key  // Azure OpenAI Key
                            ).Build();

            //Import the Plugin from Type
            kernel.Plugins.AddFromType<DataTimePlugin>();
            kernel.AutoFunctionInvocationFilters.Add(new AutoFunctionInvocationFilter());

            // Enable manual function calling
            OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };


            // Start a chat session
            while (true)
            {
                //user's message
                Console.Write("User > ");
                var userMessage = Console.ReadLine()!;

                // Invoke the kernel
                var results = await kernel.InvokePromptAsync(userMessage, new(openAIPromptExecutionSettings));

                // Print the Assistant results
                Console.WriteLine($"Assistant > {results}");
            }
            Console.ReadLine();
        }

    }

    public class AutoFunctionInvocationFilter : IAutoFunctionInvocationFilter
    {
        public async Task OnAutoFunctionInvocationAsync(AutoFunctionInvocationContext context, Func<AutoFunctionInvocationContext, Task> next)
        {
            // Example: get function information
            Console.WriteLine($"Currenct Function Name: {context.Function.Name}");

            // Example: get chat history
            var chatHistory = context.ChatHistory;

            // Example: get information about all functions which will be invoked
            var functionCalls = FunctionCallContent.GetFunctionCalls(context.ChatHistory.Last());
            foreach (var functionCall in functionCalls)
            {
                Console.WriteLine($"Function Name: {functionCall.FunctionName}");
            }

            // Example: get request sequence index
            Console.WriteLine($"Request Sequence Index: {context.RequestSequenceIndex}");

            // Example: get function sequence index
            Console.WriteLine($"Function Sequence Index: {context.FunctionSequenceIndex}");

            // Example: get total number of functions which will be called
            Console.WriteLine($"Function Count: {context.FunctionCount}");

            // Calling next filter in pipeline or function itself.
            // By skipping this call, next filters and function won't be invoked, and function call loop will proceed to the next function.
            await next(context);

            // Example: get function result
            var result = context.Result;
            Console.WriteLine($"function result => {context.Result}");

            // Example: override function result value
            //context.Result = new FunctionResult(context.Result, "Result from auto function invocation filter");

            // Example: Terminate function invocation
            //context.Terminate = true;
        }
    }

}
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using WebApplication1.Plugins;

namespace WebApplication1.Model
{
    public class DateTimeKernelClient
    {
        private readonly Kernel _kernel;

        public DateTimeKernelClient(Kernel kernel)
        {
            _kernel = kernel;
        }

        public async Task<string> DateTimeAsync(string query)
        {
            // Enable auto invocation of kernel functions
            OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            _kernel.Plugins.AddFromType<DateTimeInformation>();
            var result = await _kernel.InvokePromptAsync(query, arguments: new(openAIPromptExecutionSettings) { });

            return result.ToString();
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Chat1Controller : ControllerBase
    {
        private readonly Kernel _kernel;

        public Chat1Controller(Kernel kernel)
        {
            _kernel = kernel;
        }

        [HttpGet]
        public async Task<IActionResult> Chat(string query)
        {
            // Enable auto invocation of kernel functions
            OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            var result = await _kernel.InvokePromptAsync(query, arguments: new(openAIPromptExecutionSettings) { });
            return Ok(result.ToString());
        }
    }
}

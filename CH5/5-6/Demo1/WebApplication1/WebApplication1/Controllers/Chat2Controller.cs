using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Chat2Controller : ControllerBase
    {
        private readonly Kernel _kernel;
        private readonly KernelFunction _summarizeFun;

        public Chat2Controller(Kernel kernel)
        {

            //若kernel被DI為單體模式，而特定功能需要擴展或修改kernel，則可以使用Clone方法，產生一個新的實例，以避免影響原kernel實例
            _kernel = kernel.Clone();
            //Import the Plugin from the plugins directory.
            var pluginsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
            var plugin = _kernel.ImportPluginFromPromptDirectory(Path.Combine(pluginsDirectory, "SummarizePlugin"));
            _summarizeFun = plugin["Summarize"];
        }

        [HttpGet]
        public async Task<IActionResult> Chat(string query)
        {
            // Enable auto invocation of kernel functions
            OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            var result = (await _kernel.InvokeAsync(_summarizeFun, arguments: new() { { "user_query", query } }));
            return Ok(result.ToString());
        }
    }
}

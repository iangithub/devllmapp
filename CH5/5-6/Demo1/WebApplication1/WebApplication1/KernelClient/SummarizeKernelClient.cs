using Microsoft.SemanticKernel;

namespace WebApplication1.Model
{
    public class SummarizeKernelClient
    {
        private readonly Kernel _kernel;

        public SummarizeKernelClient(Kernel kernel)
        {
            _kernel = kernel;
        }

        public async Task<string> SummarizeAsync(string query)
        {
            //Import the Plugin from the plugins directory.
            var pluginsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
            var plugin = _kernel.ImportPluginFromPromptDirectory(Path.Combine(pluginsDirectory, "SummarizePlugin"));
            var summarizeFun = plugin["Summarize"];

           var result = (await _kernel.InvokeAsync(summarizeFun, arguments: new() { { "user_query", query } }));

            return result.ToString();
        }
    }
}

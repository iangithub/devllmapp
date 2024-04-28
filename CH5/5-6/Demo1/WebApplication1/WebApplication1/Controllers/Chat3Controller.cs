using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel;
using WebApplication1.Model;
using Microsoft.AspNetCore.Http.HttpResults;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Chat3Controller : ControllerBase
    {
        private readonly DateTimeKernelClient _dateTimeKernelClient;
        private readonly SummarizeKernelClient _summarizeKernelClient;

        public Chat3Controller(DateTimeKernelClient dateTimeKernelClient, SummarizeKernelClient summarizeKernelClient)
        {
            _dateTimeKernelClient = dateTimeKernelClient;
            _summarizeKernelClient = summarizeKernelClient;
        }


        [Route("datetime")]
        [HttpGet]
        public async Task<IActionResult> DateTimeChat(string query)
        {
            var result = await _dateTimeKernelClient.DateTimeAsync(query);
            return Ok(result.ToString());
        }

        [Route("summarize")]
        [HttpGet]
        public async Task<IActionResult> SmmarizeChat(string query)
        {
            var result = await _summarizeKernelClient.SummarizeAsync(query);

            return Ok(result.ToString());
        }
    }
}

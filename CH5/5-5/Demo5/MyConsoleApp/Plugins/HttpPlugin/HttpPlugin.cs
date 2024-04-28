using System;
using System.ComponentModel;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

public class HttpPlugin
{
    [KernelFunction, Description("Makes a POST request to a uri")]
    public async Task<string> PostAsync(
        [Description("The URI of the request")] string url,
        [Description("The body of the request")] string json)
    {
        using (HttpClient client = new HttpClient())
        {
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            string responseContent = await response.Content.ReadAsStringAsync();
            return responseContent;
        }
    }
}
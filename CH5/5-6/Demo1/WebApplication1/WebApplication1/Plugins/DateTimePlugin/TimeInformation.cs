using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace WebApplication1.Plugins
{
    public class DateTimeInformation
    {
        [KernelFunction, Description("Retrieves the current date time")]
        public string GetCurrentDateTime()
        {
            return DateTime.Now.ToString();
        }
    }
}

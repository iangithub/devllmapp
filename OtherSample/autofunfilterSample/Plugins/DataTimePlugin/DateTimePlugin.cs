using Microsoft.SemanticKernel;
using System.ComponentModel;

internal class DataTimePlugin
{
    [KernelFunction, Description("Get the current date and time")]
    public string GetCurrentDateTime()
    {
        return DateTime.Now.ToString();
    }
}
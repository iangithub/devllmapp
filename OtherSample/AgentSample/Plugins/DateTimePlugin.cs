using System.ComponentModel;
using Microsoft.SemanticKernel;

public class DateTimePlugin
{
    [KernelFunction, Description("Retrieves real world the current date time")]
    public string GetCurrentDateTime()
    {
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
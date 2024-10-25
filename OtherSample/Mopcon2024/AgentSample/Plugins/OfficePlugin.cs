using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.SemanticKernel;

public class OfficePlugin
{
    [KernelFunction, Description("Get customer information by name")]
    public string GetCustomerInfo([Description("customer name")] string customerName)
    {
        var customerInfo = new Dictionary<string, string>
            {
                { "Name", customerName },
                { "Address", "123 Main St, Anytown, USA" },
                { "Phone", "555-1234" },
                { "Email", "Lee@mail.com" }
            };
        return System.Text.Json.JsonSerializer.Serialize(customerInfo);
    }

    [KernelFunction, Description("send e-mail to customer")]
    public string SendEmail([Description("email address")] string mailTo
    , [Description("email recipient")] string name
    , [Description("email subject")] string subject
    , [Description("email content")] string content)
    {

        return $"email sent to {mailTo} with subject {subject} and content {content} successfully";
    }

    [KernelFunction, Description("Get schedule by date")]
    public string GetSchedule([Description("schedule on the date")] string date)
    {
        // Generate mock schedule data
        var random = new Random();
        var events = new List<string> { "Meeting with customer", "Team Standup", "Project Presentation", "Lunch with Client", "Code Review" };
        var times = new List<string> { "10:00 AM", "11:00 AM", "1:00 PM", "2:00 PM", "3:00 PM" };
        var locations = new List<string> { "Conference Room", "Office", "Cafeteria", "Client's Office", "Online" };

        var eventIndex = random.Next(events.Count);
        var timeIndex = random.Next(times.Count);
        var locationIndex = random.Next(locations.Count);

        var schedules = new List<Dictionary<string, string>>();

        for (int i = 0; i < 5; i++)
        {
            eventIndex = random.Next(events.Count);
            timeIndex = random.Next(times.Count);
            locationIndex = random.Next(locations.Count);

            schedules.Add(new Dictionary<string, string>
                {
                    { "Date", date },
                    { "Event", events[eventIndex] },
                    { "Time", times[timeIndex] },
                    { "Location", locations[locationIndex] }
                });
        }

        return System.Text.Json.JsonSerializer.Serialize(schedules);

    }
}

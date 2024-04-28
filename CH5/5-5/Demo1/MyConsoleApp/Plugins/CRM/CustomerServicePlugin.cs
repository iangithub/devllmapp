using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace MyConsoleApp.Plugins.CRM
{
    public class CustomerServicePlugin
    {
        private List<CustomerData> customers = new List<CustomerData>
                {
                    new CustomerData { Name = "John", Email = "john@example.com", Gender="M" },
                    new CustomerData { Name = "Jane", Email = "jane@example.com", Gender="F" },
                    new CustomerData { Name = "Mike", Email = "mike@example.com", Gender="M" }
                };

        [KernelFunction, Description("Query customer data")]
        public CustomerData QueryCustomerData(string customerName)
        {
            return customers.FirstOrDefault(c => c.Name == customerName);
        }
    }

    public class CustomerData
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
    }
}

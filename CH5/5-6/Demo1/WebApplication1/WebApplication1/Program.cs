using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.SemanticKernel;
using WebApplication1.Model;
using WebApplication1.Plugins;

namespace WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();

            string deploy_Name = "xxx";
            string aoai_Endpoint = "https://xxxx.openai.azure.com";
            string api_Key = "xxxxx";

            //builder.Services.AddSingleton<Kernel>(sp =>
            //    {
            //        var builder = Kernel.CreateBuilder();
            //        builder.Services.AddAzureOpenAIChatCompletion(deploymentName: deploy_Name, endpoint: aoai_Endpoint, apiKey: api_Key);
            //        builder.Plugins.AddFromType<DateTimeInformation>();
            //        return builder.Build();
            //    });

            //for KernelClient Sample
            builder.Services.AddScoped<Kernel>(sp =>
            {
                var builder = Kernel.CreateBuilder();
                builder.Services.AddAzureOpenAIChatCompletion(deploymentName: deploy_Name, endpoint: aoai_Endpoint, apiKey: api_Key);
                return builder.Build();
            });
            builder.Services.AddScoped<DateTimeKernelClient>();
            builder.Services.AddScoped<SummarizeKernelClient>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}

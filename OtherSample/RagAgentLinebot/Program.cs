using RagAgentLinebot.Models;
namespace RagAgentLinebot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
            builder.Services.AddScoped<MultiRagAgent>();
            builder.Services.AddScoped<RagTrafficLawPlugin>();
            builder.Services.AddScoped<RagWorkerLawPlugin>();
            builder.Services.AddLogging();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}

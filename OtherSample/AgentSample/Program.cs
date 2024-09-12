// See https://aka.ms/new-console-template for more information


using Elastic.Clients.Elasticsearch.Aggregations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddLogging(configure =>
        {
            configure.AddConsole();
            configure.SetMinimumLevel(LogLevel.Trace); // Set the minimum log level to Trace
        });
    })
    .Build();
var logger = host.Services.GetRequiredService<ILogger<Program>>();

Console.WriteLine("\n\n Hello, World Dev! \n\n");

var agent = new BasicAgent();
await agent.ChatCompletionAgentAsync();

// var agent = new MultiPluginAgent(logger);
// await agent.ChatCompletionAgentAsync();

// var agent = new MultiRagAgent(logger);
// await agent.ChatCompletionAgentAsync();

// var agent = new MultiAgent(logger);
// await agent.ChatCompletionAgentAsync();


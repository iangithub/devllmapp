// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mopcon2024.AgentSample;

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

//Simple agent that uses the OpenAI chat completion model to respond to user input.
var agent = new BasicAgent();
await agent.ChatCompletionAgentAsync();

//sequence agents use OpenAI GPT-4 model and Google AI Gemini model
// var agent = new NewsAgent();
// await agent.ChatCompletionAgentAsync();

//Reflection agents use OpenAI GPT-4 model
// var agent = new ReflectionAgent();
// await agent.ChatCompletionAgentAsync();

//Delegate Agents based on target tasks
// var agent = new DelegateAgent();
// await agent.ChatCompletionAgentAsync();

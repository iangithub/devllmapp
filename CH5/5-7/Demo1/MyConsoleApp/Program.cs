using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Planning.Handlebars;
using MyConsoleApp.Plugins;

namespace IntroSample
{
    internal class Program
    {
        private const string deploy_Name = "xxxx";
        private const string aoai_Endpoint = "https://xxxx.openai.azure.com";
        private const string api_Key = "xxxxx";


        static async Task Main(string[] args)
        {

            //await AutoFunctionCalling();
            //await HandlebarsPlannerDemo1();
            //await HandlebarsPlannerDemo2();
            await HandlebarsPlannerDemo3();
            //await HandlebarsPlannerDemo4();
            //await FunctionCallingStepwisePlannerDemo();
            Console.ReadLine();
        }

        static async Task AutoFunctionCalling()
        {
            var kernel = Kernel.CreateBuilder()
                            .AddAzureOpenAIChatCompletion(
                             deploymentName: deploy_Name,   // Azure OpenAI Deployment Name
                             endpoint: aoai_Endpoint, // Azure OpenAI Endpoint
                             apiKey: api_Key  // Azure OpenAI Key
                            ).Build();

            //Import the Plugin from Type
            kernel.Plugins.AddFromType<DataTimePlugin>();

            // Enable manual function calling
            OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };


            // Start a chat session
            while (true)
            {
                //user's message
                Console.Write("User > ");
                var userMessage = Console.ReadLine()!;

                // Invoke the kernel
                var results = await kernel.InvokePromptAsync(userMessage, new(openAIPromptExecutionSettings));

                // Print the Assistant results
                Console.WriteLine($"Assistant > {results}");
            }
        }


        /// <summary>
        /// Kernel所掛載的Plugins足以完成任務
        /// </summary>
        /// <returns></returns>
        static async Task HandlebarsPlannerDemo1()
        {
            string goal = "今天台北的天氣適合怎麼打扮啊";
            var kernel = Kernel.CreateBuilder()
                                        .AddAzureOpenAIChatCompletion(
                                         deploymentName: deploy_Name,   // Azure OpenAI Deployment Name
                                         endpoint: aoai_Endpoint, // Azure OpenAI Endpoint
                                         apiKey: api_Key  // Azure OpenAI Key
                                        ).Build();

            //Import the Plugin from Type
            kernel.Plugins.AddFromType<WeatherServicePlugin>();

            // Import the Plugin from the plugins directory.
            var pluginsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
            var plugin = kernel.ImportPluginFromPromptDirectory(Path.Combine(pluginsDirectory, "StylistPlugin"));
            KernelFunction styleFun = plugin["Style"];


            // Set the planner options
            var plannerOptions = new HandlebarsPlannerOptions()
            {
                //使用較低的Temperature和 top_p 參數值，以盡量減少Planner的幻覺
                ExecutionSettings = new OpenAIPromptExecutionSettings()
                {
                    Temperature = 0.0,
                    TopP = 0.1,
                },
            };
            plannerOptions.AllowLoops = true;

            //Create the planner
            var planner = new HandlebarsPlanner(plannerOptions);
            var plan = await planner.CreatePlanAsync(kernel, goal);

            // Execute the plan
            var result = await plan.InvokeAsync(kernel);

            PrintPlannerDetails(goal, plan, result);
        }

        /// <summary>
        /// Kernel所掛載的Plugins不足以完成任務，因為沒有掛載高鐵時刻表的Plugin以及時間的Plugin
        /// </summary>
        /// <returns></returns>
        static async Task HandlebarsPlannerDemo2()
        {
            string goal = "最近一班到台北的高鐵是幾點發車";
            var kernel = Kernel.CreateBuilder()
                                        .AddAzureOpenAIChatCompletion(
                                         deploymentName: deploy_Name,   // Azure OpenAI Deployment Name
                                         endpoint: aoai_Endpoint, // Azure OpenAI Endpoint
                                         apiKey: api_Key  // Azure OpenAI Key
                                        ).Build();

            //Import the Plugin from Type
            kernel.Plugins.AddFromType<WeatherServicePlugin>();

            // Import the Plugin from the plugins directory.
            var pluginsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
            var plugin = kernel.ImportPluginFromPromptDirectory(Path.Combine(pluginsDirectory, "StylistPlugin"));
            KernelFunction styleFun = plugin["Style"];

            try
            {
                // Set the planner options
                var plannerOptions = new HandlebarsPlannerOptions()
                {
                    //使用較低的Temperature和 top_p 參數值，以盡量減少Planner的幻覺
                    ExecutionSettings = new OpenAIPromptExecutionSettings()
                    {
                        Temperature = 0.0,
                        TopP = 0.1,
                    },
                };
                plannerOptions.AllowLoops = true;

                //Create the planner
                var planner = new HandlebarsPlanner(plannerOptions);
                var plan = await planner.CreatePlanAsync(kernel, goal);

                // Execute the plan
                var result = await plan.InvokeAsync(kernel);

                PrintPlannerDetails(goal, plan, result);
            }
            catch (KernelException ex)
            {
                Console.WriteLine($"\n{ex.Message}\n");
            }

        }

        /// <summary>
        /// LLM模型原生能力，產生書本的內容
        /// </summary>
        /// <returns></returns>
        static async Task HandlebarsPlannerDemo3()
        {
            string goal = "創作一本包含 3 個章節的書，內容是「進入校園學習」的故事，對象是7~10歲的孩童，使用 #zh-tw。";
            var kernel = Kernel.CreateBuilder()
                                        .AddAzureOpenAIChatCompletion(
                                         deploymentName: deploy_Name,   // Azure OpenAI Deployment Name
                                         endpoint: aoai_Endpoint, // Azure OpenAI Endpoint
                                         apiKey: api_Key  // Azure OpenAI Key
                                        ).Build();

            //Import the Plugin from Type
            kernel.Plugins.AddFromType<WeatherServicePlugin>();

            // Import the Plugin from the plugins directory.
            var pluginsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
            var plugin = kernel.ImportPluginFromPromptDirectory(Path.Combine(pluginsDirectory, "StylistPlugin"));
            KernelFunction styleFun = plugin["Style"];

            try
            {
                // Set the planner options
                var plannerOptions = new HandlebarsPlannerOptions()
                {
                    //使用較低的Temperature和 top_p 參數值，以盡量減少Planner的幻覺
                    ExecutionSettings = new OpenAIPromptExecutionSettings()
                    {
                        Temperature = 0.0,
                        TopP = 0.1,
                    },
                };
                plannerOptions.AllowLoops = true;

                //Create the planner
                var planner = new HandlebarsPlanner(plannerOptions);
                var plan = await planner.CreatePlanAsync(kernel, goal);

                // Execute the plan
                var result = await plan.InvokeAsync(kernel);

                PrintPlannerDetails(goal, plan, result);
            }
            catch (KernelException ex)
            {
                Console.WriteLine($"\n{ex.Message}\n");
            }

        }

        /// <summary>
        /// 使用既存的plan計劃
        /// </summary>
        /// <returns></returns>
        static async Task HandlebarsPlannerDemo4()
        {
            string goal = "今天高雄的天氣適合怎麼打扮啊";
            var kernel = Kernel.CreateBuilder()
                                        .AddAzureOpenAIChatCompletion(
                                         deploymentName: deploy_Name,   // Azure OpenAI Deployment Name
                                         endpoint: aoai_Endpoint, // Azure OpenAI Endpoint
                                         apiKey: api_Key  // Azure OpenAI Key
                                        ).Build();

            //Import the Plugin from Type
            kernel.Plugins.AddFromType<WeatherServicePlugin>();

            // Import the Plugin from the plugins directory.
            var pluginsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
            var plugin = kernel.ImportPluginFromPromptDirectory(Path.Combine(pluginsDirectory, "StylistPlugin"));
            KernelFunction styleFun = plugin["Style"];


            //使用既存的plan計劃
            string serializedPlan = await File.ReadAllTextAsync("plan.txt");
            HandlebarsPlan plan = new HandlebarsPlan(serializedPlan);

            // Execute the plan
            var result = await plan.InvokeAsync(kernel);

            PrintPlannerDetails(goal, plan, result);
        }

        static async Task FunctionCallingStepwisePlannerDemo()
        {
            var goal = @"台北的天氣如何? 我要去台北應該怎麼穿才適合? ";

            var kernel = Kernel.CreateBuilder()
                            .AddAzureOpenAIChatCompletion(
                            deploymentName: deploy_Name,   // Azure OpenAI Deployment Name
                            endpoint: aoai_Endpoint, // Azure OpenAI Endpoint
                            apiKey: api_Key  // Azure OpenAI Key
                            ).Build();

            //Import the Plugin from Type
            kernel.Plugins.AddFromType<DataTimePlugin>();
            kernel.Plugins.AddFromType<WeatherServicePlugin>();

            // Import the Plugin from the plugins directory.
            var pluginsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
            var plugin = kernel.ImportPluginFromPromptDirectory(Path.Combine(pluginsDirectory, "StylistPlugin"));
            KernelFunction styleFun = plugin["Style"];

            var planner = new FunctionCallingStepwisePlanner(
                new FunctionCallingStepwisePlannerOptions
                {
                    MaxIterations = 10,
                    MaxTokens = 4000,
                });


            var result = await planner.ExecuteAsync(kernel, goal);
            Console.WriteLine($"Q: {goal}\nA: {result.FinalAnswer} \n\n");

            // You can uncomment the line below to see the planner's process for completing the request.
            Console.WriteLine($"Chat history:\n{System.Text.Json.JsonSerializer.Serialize(result.ChatHistory, new System.Text.Json.JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            })}");
        }

        static void PrintPlannerDetails(string goal, HandlebarsPlan plan, string result)
        {
            Console.WriteLine($"目標: {goal}");
            Console.WriteLine($"原始計劃: \n {plan} \n");
            Console.WriteLine($"結果: \n {result} \n");

            // Print the prompt template
            if (plan.Prompt is not null)
            {
                Console.WriteLine("======== CreatePlan Prompt ========\n");
                Console.WriteLine(plan.Prompt);
            }
        }

    }
}
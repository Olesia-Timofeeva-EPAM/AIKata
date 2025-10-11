// See https://aka.ms/new-console-template for more information
using LLMService;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

var apiKey = config["Dial:ApiKey"];
var endpoint = config["Dial:Endpoint"];
var model = config["Dial:Model"];

ILlmService llmService = new LlmService(apiKey, endpoint, model);

// Example call
var transcript = "Let's finalize our Q4 roadmap next Monday, and assign marketing leads for new campaigns.";
var summary = await llmService.SummarizeAsync(transcript);

Console.WriteLine("=== Meeting Summary ===");
Console.WriteLine(summary);

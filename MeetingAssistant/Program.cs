using ASRService;
using LLMService;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

var apiKey = config["Dial:ApiKey"];
var endpoint = config["Dial:Endpoint"];
var model = config["Dial:Model"];

var speechKey = config["Speech:SubscriptionKey"];
var speechRegion = config["Speech:Region"];

var asrService = new AsrService(speechKey, speechRegion);
ILlmService llmService = new LlmService(apiKey, endpoint, model);

var transcript = await asrService.TranscribeFromMicAsync();
var summary = await llmService.SummarizeAsync(transcript);

Console.WriteLine("=== Meeting Summary ===");
Console.WriteLine(summary);

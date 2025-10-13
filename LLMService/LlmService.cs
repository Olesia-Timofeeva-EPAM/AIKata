using System.Net;
using System.Text;
using System.Text.Json;

namespace LLMService
{
    public class LlmService : ILlmService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _endpoint;
        private readonly string _model;
        private const string _apiVersion = "2024-08-01-preview";

        public LlmService(string apiKey, string endpoint, string model)
        {
            ServicePointManager.ServerCertificateValidationCallback = (s, c, ch, e) => true;
            ServicePointManager.CheckCertificateRevocationList = false;
            _httpClient = new HttpClient();
            _apiKey = apiKey;
            _endpoint = endpoint;
            _model = model;
            _httpClient.DefaultRequestHeaders.Add("api-key", apiKey);
        }

        public async Task<string> SummarizeAsync(string transcript)
        {
            if (string.IsNullOrWhiteSpace(transcript))
            {
                return "No transcript available for summarization.";
            }
            
            var payload = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system", content = "You are an AI meeting assistant that summarizes and suggests next steps concisely." },
                    new { role = "user", content = $"Summarize this part of the meeting transcript and suggest next talking point:\n{transcript}" }
                },
                max_tokens = 900,
                temperature = 0.8
            };

            var json = JsonSerializer.Serialize(payload);

            for (int attempt = 1; attempt <= 3; attempt++)
            {
                var response = await _httpClient.PostAsync(_endpoint, new StringContent(json, Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(result);
                    return doc.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString()?.Trim() ?? "(no summary returned)";
                }

                // Handle 429 Too Many Requests
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    Console.WriteLine($"⚠️ Rate limit hit (attempt {attempt}). Waiting before retry...");
                    await Task.Delay(2000 * attempt); // exponential backoff
                    continue;
                }

                // Other errors
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API error: {response.StatusCode} - {error}");
            }

            return "Request failed after multiple retries due to rate limiting.";
        }
    }
}

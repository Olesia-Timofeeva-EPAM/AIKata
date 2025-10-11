using System.Net.Http.Headers;
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

        public LlmService(string apiKey, string endpoint = "https://api.dial.epam.com/v1/chat/completions", string model = "gpt-4o-mini")
        {
            _httpClient = new HttpClient();
            _apiKey = apiKey;
            _endpoint = endpoint;
            _model = model;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
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
                max_tokens = 300,
                temperature = 0.7
            };

            var json = JsonSerializer.Serialize(payload);
            var response = await _httpClient.PostAsync(_endpoint, new StringContent(json, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(result);
            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return content?.Trim() ?? "(no summary returned)";
        }
    }
}

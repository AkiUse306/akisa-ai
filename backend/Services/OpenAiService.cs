using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AkisaAi.Api.Models;
using Microsoft.Extensions.Configuration;

namespace AkisaAi.Api.Services;

public sealed class OpenAiService
{
    private readonly string? _apiKey;
    private readonly HttpClient _httpClient;

    public OpenAiService(IConfiguration configuration)
    {
        _apiKey = configuration["OpenAI:ApiKey"];
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (!string.IsNullOrEmpty(_apiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        }
    }

    public bool IsConfigured => !string.IsNullOrEmpty(_apiKey);

    public async Task<string> CreateChatCompletionAsync(string prompt, string model = "gpt-3.5-turbo")
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException("OpenAI API key is not configured.");
        }

        var payload = new
        {
            model,
            messages = new[]
            {
                new { role = "system", content = "You are AKISA-AI, a multi-modal AI operating system assistant." },
                new { role = "user", content = prompt }
            },
            temperature = 0.7,
            max_tokens = 800
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        var completion = document.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return completion ?? string.Empty;
    }
}

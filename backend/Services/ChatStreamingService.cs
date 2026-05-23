using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AkisaAi.Api.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AkisaAi.Api.Services;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class ChatStreamingService
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;
    private readonly ILogger<ChatStreamingService> _logger;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public ChatStreamingService(IConfiguration configuration, ILogger<ChatStreamingService> logger)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        _apiKey = configuration["OpenAI:ApiKey"];
        _httpClient = new HttpClient();
        _logger = logger;

        if (!string.IsNullOrEmpty(_apiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
        }
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsConfigured => !string.IsNullOrEmpty(_apiKey);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async IAsyncEnumerable<ChatStreamToken> StreamChatAsync(
        string prompt,
        string model = "gpt-4",
        string? systemPrompt = null,
        List<ChatMessage>? history = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        if (!IsConfigured)
        {
            yield return new ChatStreamToken { Content = "Streaming is not available without OpenAI configuration.", Delta = "Streaming is not available without OpenAI configuration." };
            yield break;
        }

        var messages = new List<Dictionary<string, string>>();

        if (!string.IsNullOrEmpty(systemPrompt))
        {
            messages.Add(new() { { "role", "system" }, { "content", systemPrompt } });
        }

        if (history != null && history.Count > 0)
        {
            foreach (var msg in history)
            {
                messages.Add(new() { { "role", msg.Role }, { "content", msg.Content } });
            }
        }

        messages.Add(new() { { "role", "user" }, { "content", prompt } });

        var payload = new
        {
            model,
            messages,
            stream = true,
            temperature = 0.7,
            max_tokens = 800
        };

        var tokens = await GetStreamTokensAsync(payload);
        foreach (var token in tokens)
        {
            yield return token;
        }
    }

    private async Task<List<ChatStreamToken>> GetStreamTokensAsync(object payload)
    {
        var tokens = new List<ChatStreamToken>();

        try
        {
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            using var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("OpenAI streaming request failed with status {StatusCode}", response.StatusCode);
                tokens.Add(new ChatStreamToken { Content = $"Streaming failed: {response.StatusCode}", Delta = $"Error {response.StatusCode}" });
                return tokens;
            }

            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line) || line == "data: [DONE]")
                    continue;

                if (line.StartsWith("data: "))
                {
                    try
                    {
                        var json = line[6..]; // Remove "data: " prefix
                        using var doc = JsonDocument.Parse(json);
                        var root = doc.RootElement;

                        if (root.TryGetProperty("choices", out var choices) &&
                            choices.GetArrayLength() > 0)
                        {
                            var choice = choices[0];
                            if (choice.TryGetProperty("delta", out var delta) &&
                                delta.TryGetProperty("content", out var deltaContent) &&
                                deltaContent.GetString() is { } content_str)
                            {
                                tokens.Add(new ChatStreamToken
                                {
                                    Content = content_str,
                                    Delta = content_str,
                                    FinishReason = choice.TryGetProperty("finish_reason", out var fr) ? fr.GetString() : null
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse streaming token");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Streaming request failed");
            tokens.Add(new ChatStreamToken { Content = $"Streaming error: {ex.Message}", Delta = "Error" });
        }

        return tokens;
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<string> GetStreamedResponseAsync(string prompt, string model = "gpt-4", string? systemPrompt = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        var fullResponse = new StringBuilder();

        try
        {
            await foreach (var token in StreamChatAsync(prompt, model, systemPrompt))
            {
                fullResponse.Append(token.Content);
            }

            return fullResponse.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get streamed response");
            return $"Error: {ex.Message}";
        }
    }
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class ChatStreamToken
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Content { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Delta { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? FinishReason { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class ChatMessage
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Role { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Content { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

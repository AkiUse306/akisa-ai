using System;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AkisaAi.Api.Services;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class VoiceService
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;
    private readonly ILogger<VoiceService> _logger;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public VoiceService(IConfiguration configuration, ILogger<VoiceService> logger)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        _apiKey = configuration["OpenAI:ApiKey"];
        _httpClient = new HttpClient();
        _logger = logger;

        if (!string.IsNullOrEmpty(_apiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        }
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsConfigured => !string.IsNullOrEmpty(_apiKey);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<VoiceResponse> TextToSpeechAsync(string text, string voice = "nova", string model = "tts-1-hd")
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        if (!IsConfigured)
        {
            return new VoiceResponse
            {
                Success = false,
                Message = "Text-to-speech is not available without OpenAI configuration.",
                Data = Array.Empty<byte>()
            };
        }

        try
        {
            var payload = new
            {
                model,
                input = text,
                voice
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://api.openai.com/v1/audio/speech", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Text-to-speech request failed with status {StatusCode}", response.StatusCode);
                return new VoiceResponse
                {
                    Success = false,
                    Message = $"TTS request failed: {response.StatusCode}",
                    Data = Array.Empty<byte>()
                };
            }

            var audioData = await response.Content.ReadAsByteArrayAsync();
            return new VoiceResponse
            {
                Success = true,
                Message = "Text converted to speech successfully.",
                Data = audioData,
                ContentType = "audio/mp3"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Text-to-speech failed");
            return new VoiceResponse
            {
                Success = false,
                Message = $"Error: {ex.Message}",
                Data = Array.Empty<byte>()
            };
        }
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<SpeechToTextResponse> SpeechToTextAsync(Stream audioStream, string language = "en")
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        if (!IsConfigured)
        {
            return new SpeechToTextResponse
            {
                Success = false,
                Text = "Speech-to-text is not available without OpenAI configuration."
            };
        }

        try
        {
            using var multipartContent = new MultipartFormDataContent();
            multipartContent.Add(new StreamContent(audioStream), "file", "audio.mp3");
            multipartContent.Add(new StringContent("whisper-1"), "model");
            multipartContent.Add(new StringContent(language), "language");

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/audio/transcriptions", multipartContent);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Speech-to-text request failed with status {StatusCode}", response.StatusCode);
                return new SpeechToTextResponse
                {
                    Success = false,
                    Text = $"STT request failed: {response.StatusCode}"
                };
            }

            using var stream = await response.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);
            var text = doc.RootElement.GetProperty("text").GetString() ?? "";

            return new SpeechToTextResponse
            {
                Success = true,
                Text = text,
                Language = language
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Speech-to-text failed");
            return new SpeechToTextResponse
            {
                Success = false,
                Text = $"Error: {ex.Message}"
            };
        }
    }
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class VoiceResponse
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool Success { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Message { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public byte[] Data { get; set; } = Array.Empty<byte>();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string ContentType { get; set; } = "audio/mp3";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class SpeechToTextResponse
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool Success { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Text { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? Language { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

using AkisaAi.Api.Data;
using AkisaAi.Api.Models;

namespace AkisaAi.Api.Services;

public sealed class ComparisonService
{
    private readonly InMemoryStore _store;
    private readonly ModelRouterService _router;
    private readonly OpenAiService _openAiService;
    private readonly ILogger<ComparisonService> _logger;

    public ComparisonService(InMemoryStore store, ModelRouterService router, OpenAiService openAiService, ILogger<ComparisonService> logger)
    {
        _store = store;
        _router = router;
        _openAiService = openAiService;
        _logger = logger;
    }

    public async Task<CompareResponse> CompareAsync(string prompt)
    {
        var sessionId = _store.CreateSession("comparison-anonymous");
        var conversation = _store.GetConversation(sessionId);
        var memory = _store.GetRecentMemory("comparison-anonymous", 5);

        var akisaResponse = await _router.RouteChatAsync(prompt, memory, conversation);
        var chatResponse = await GetChatGptResponseAsync(prompt);
        var cursorResponse = await GetCursorResponseAsync(prompt);

        return new CompareResponse
        {
            Akisa = akisaResponse,
            ChatGpt = chatResponse,
            Cursor = cursorResponse,
            SessionId = sessionId
        };
    }

    private async Task<string> GetChatGptResponseAsync(string prompt)
    {
        if (!_openAiService.IsConfigured)
        {
            return "ChatGPT simulation is unavailable because the OpenAI key is not configured. Configure OpenAI__ApiKey to enable direct comparison.";
        }

        try
        {
            return await _openAiService.CreateChatCompletionAsync(
                prompt,
                model: "gpt-4",
                systemPrompt: "You are ChatGPT, a conversational AI assistant. Answer clearly, thoughtfully, and with general-purpose reasoning.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ChatGPT comparison request failed.");
            return "ChatGPT comparison request failed. Please check your OpenAI configuration.";
        }
    }

    private async Task<string> GetCursorResponseAsync(string prompt)
    {
        if (!_openAiService.IsConfigured)
        {
            return "Cursor simulation is unavailable because the OpenAI key is not configured. Configure OpenAI__ApiKey to enable direct comparison.";
        }

        try
        {
            return await _openAiService.CreateChatCompletionAsync(
                prompt,
                model: "gpt-4",
                systemPrompt: "You are Cursor, a developer productivity assistant that focuses on code context, workflow automation, navigation, and practical engineering guidance. Provide answers that are concise and developer-focused.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cursor comparison request failed.");
            return "Cursor comparison request failed. Please check your OpenAI configuration.";
        }
    }
}

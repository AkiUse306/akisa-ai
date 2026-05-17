using AkisaAi.Api.Data;
using AkisaAi.Api.Models;

namespace AkisaAi.Api.Services;

public sealed class AiService
{
    private readonly InMemoryStore _store;
    private readonly ModelRouterService _router;
    private readonly ILogger<AiService> _logger;

    public AiService(InMemoryStore store, ModelRouterService router, ILogger<AiService> logger)
    {
        _store = store;
        _router = router;
        _logger = logger;
    }

    public async Task<ChatResponse> CreateChatResponseAsync(ChatRequest request, string userId)
    {
        var sessionId = request.SessionId ?? _store.CreateSession(userId);
        var user = _store.GetUserById(userId);
        var memory = _store.GetRecentMemory(userId, 5);
        var conversation = _store.GetConversation(sessionId);

        _store.AddConversationMessage(sessionId, "user", request.Prompt);

        var assistantMessage = await _router.RouteChatAsync(request.Prompt, memory, conversation);

        _store.AddConversationMessage(sessionId, "assistant", assistantMessage);
        _store.AddMemoryEntry(userId, "conversation", request.Prompt);

        return new ChatResponse
        {
            RequestId = Guid.NewGuid().ToString(),
            Message = assistantMessage,
            Model = request.Model ?? "dynamic-model-route",
            SessionId = sessionId
        };
    }

    public IEnumerable<AgentDescriptor> GetAvailableAgents()
    {
        return new[]
        {
            new AgentDescriptor("coding-agent", "Coding Agent", "Writes and refactors code."),
            new AgentDescriptor("research-agent", "Research Agent", "Summarizes and analyzes information."),
            new AgentDescriptor("automation-agent", "Automation Agent", "Plans workflows and executes tasks."),
            new AgentDescriptor("business-agent", "Business Agent", "Supports analytics and CRM automation."),
            new AgentDescriptor("creative-agent", "Creative Agent", "Generates ideas, stories, and visuals.")
        };
    }

    public Task<AgentExecutionResult> ExecuteAgentAsync(string agentId, string input, string userId)
    {
        var agent = GetAvailableAgents().FirstOrDefault(a => a.Id == agentId);
        if (agent is null)
        {
            return Task.FromResult(new AgentExecutionResult
            {
                AgentId = agentId,
                Output = "Agent not found.",
                Success = false
            });
        }

        var result = agentId switch
        {
            "coding-agent" => ExecuteCodingAgent(input),
            "research-agent" => ExecuteResearchAgent(input),
            "automation-agent" => ExecuteAutomationAgent(input),
            "business-agent" => ExecuteBusinessAgent(input),
            "creative-agent" => ExecuteCreativeAgent(input),
            _ => "The agent could not execute the request."
        };

        _store.AddMemoryEntry(userId, "agent", $"{agent.Name} handled: {input}");

        return Task.FromResult(new AgentExecutionResult
        {
            AgentId = agentId,
            Output = result,
            Success = true
        });
    }

    private static string GenerateAssistantText(string prompt, IReadOnlyList<MemoryEntry> memory, IReadOnlyList<ConversationMessage> conversation)
    {
        var memorySummary = memory.Any()
            ? string.Join(" \n", memory.Take(3).Select(entry => $"- {entry.Type}: {entry.Content}"))
            : "No semantic memory was available for this session.";

        var historySummary = conversation.Any()
            ? string.Join(" \n", conversation.Skip(Math.Max(0, conversation.Count - 4)).Select(message => $"[{message.Role}] {message.Text}"))
            : "No previous conversation history found.";

        return $"Prompt: {prompt}\n\nMemory:\n{memorySummary}\n\nRecent conversation:\n{historySummary}\n\nResponse:\n{ComposeResponse(prompt)}";
    }

    private static string ComposeResponse(string prompt)
    {
        return prompt.Contains("plan", StringComparison.OrdinalIgnoreCase)
            ? "I can create a multi-step plan for you. Start by confirming the key objectives and then I will break the task into discrete actions."
            : prompt.Contains("code", StringComparison.OrdinalIgnoreCase) || prompt.Contains("bug", StringComparison.OrdinalIgnoreCase)
                ? "I can help refactor your code or provide a sample implementation for the requested functionality."
                : prompt.Contains("analyze", StringComparison.OrdinalIgnoreCase) || prompt.Contains("summarize", StringComparison.OrdinalIgnoreCase)
                    ? "I will analyze your request and summarize the most important points."
                    : "I have processed your request and created a response that includes context, memory, and actionable guidance.";
    }

    private static string ExecuteCodingAgent(string input)
    {
        return $"Coding Agent is composing a code-focused plan for: {input}. Next step: analyze the logic, generate safety tests, and propose implementation details.";
    }

    private static string ExecuteResearchAgent(string input)
    {
        return $"Research Agent is summarizing and synthesizing the topic: {input}. The response includes key insights, relevant context, and suggested follow-up questions.";
    }

    private static string ExecuteAutomationAgent(string input)
    {
        return $"Automation Agent is creating the workflow plan for: {input}. It will include triggers, actions, and a validation step for continuous execution.";
    }

    private static string ExecuteBusinessAgent(string input)
    {
        return $"Business Agent is producing analytics guidance for: {input}. It will propose KPIs, customer workflows, and possible automation paths.";
    }

    private static string ExecuteCreativeAgent(string input)
    {
        return $"Creative Agent is generating creative output for: {input}. Expect idea themes, concept outlines, and inspirational prompts.";
    }
}

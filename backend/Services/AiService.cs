using AkisaAi.Api.Data;
using AkisaAi.Api.Models;

namespace AkisaAi.Api.Services;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class AiService
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
    private readonly InMemoryStore _store;
    private readonly ModelRouterService _router;
    private readonly VectorMemoryService _vectorMemory;
    private readonly ILogger<AiService> _logger;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public AiService(InMemoryStore store, ModelRouterService router, VectorMemoryService vectorMemory, ILogger<AiService> logger)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        _store = store;
        _router = router;
        _vectorMemory = vectorMemory;
        _logger = logger;
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<ChatResponse> CreateChatResponseAsync(ChatRequest request, string userId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        var sessionId = request.SessionId ?? _store.CreateSession(userId);
        var user = _store.GetUserById(userId);
        var semanticMemory = await _vectorMemory.SearchMemoryAsync(userId, request.Prompt, 5);
        var conversation = _store.GetConversation(sessionId);

        _store.AddConversationMessage(sessionId, "user", request.Prompt);
        await _vectorMemory.AddMemoryAsync(userId, request.Prompt, "user_prompt");

        var assistantMessage = await _router.RouteChatAsync(request.Prompt, semanticMemory, conversation);

        _store.AddConversationMessage(sessionId, "assistant", assistantMessage);
        await _vectorMemory.AddMemoryAsync(userId, assistantMessage, "assistant_response");

        return new ChatResponse
        {
            RequestId = Guid.NewGuid().ToString(),
            Message = assistantMessage,
            Model = request.Model ?? "dynamic-model-route",
            SessionId = sessionId
        };
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public IEnumerable<AgentDescriptor> GetAvailableAgents()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public Task<AgentExecutionResult> ExecuteAgentAsync(string agentId, string input, string userId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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

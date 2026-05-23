using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AkisaAi.Api.Data;
using AkisaAi.Api.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AkisaAi.Api.Services;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class AgentOrchestrationService
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
    private readonly OpenAiService _openAiService;
    private readonly VectorMemoryService _vectorMemoryService;
    private readonly PluginService _pluginService;
    private readonly RedisService _redisService;
    private readonly ILogger<AgentOrchestrationService> _logger;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public AgentOrchestrationService(
        OpenAiService openAiService,
        VectorMemoryService vectorMemoryService,
        PluginService pluginService,
        RedisService redisService,
        ILogger<AgentOrchestrationService> logger)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        _openAiService = openAiService;
        _vectorMemoryService = vectorMemoryService;
        _pluginService = pluginService;
        _redisService = redisService;
        _logger = logger;
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<AgentResponse> ExecuteAgentAsync(string userId, string agentType, string objective, Dictionary<string, object>? context = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        var agentId = Guid.NewGuid().ToString();
        context ??= new();

        _logger.LogInformation("Starting agent execution: {AgentType} with objective: {Objective}", agentType, objective);

        try
        {
            var systemPrompt = GenerateSystemPrompt(agentType);
            var memoryContext = await _vectorMemoryService.SearchMemoryAsync(userId, objective, 5);
            var contextSummary = string.Join("\n", memoryContext.Select(m => m.Content));

            var prompt = $@"OBJECTIVE: {objective}

AVAILABLE CONTEXT:
{contextSummary}

AGENT TYPE: {agentType}
AGENT ID: {agentId}

Please analyze the objective and context, then provide a comprehensive response that includes:
1. Analysis of the objective
2. Recommended actions or steps
3. Potential risks or considerations
4. Next steps or follow-ups";

            var response = await _openAiService.CreateChatCompletionAsync(
                prompt,
                model: "gpt-4",
                systemPrompt: systemPrompt);

            var agentResponse = new AgentResponse
            {
                AgentId = agentId,
                AgentType = agentType,
                Status = "completed",
                Response = response,
                ExecutionTimeMs = 0,
                Context = context,
                Timestamp = DateTime.UtcNow
            };

            // Cache the agent execution result
            await _redisService.SetJsonAsync($"agent:{agentId}", agentResponse, TimeSpan.FromHours(24));

            return agentResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Agent execution failed for type {AgentType}", agentType);
            return new AgentResponse
            {
                AgentId = agentId,
                AgentType = agentType,
                Status = "failed",
                Response = $"Agent execution failed: {ex.Message}",
                ExecutionTimeMs = 0,
                Context = context,
                Timestamp = DateTime.UtcNow
            };
        }
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<AgentResponse> ExecuteToolAsync(string userId, string toolId, Dictionary<string, object> toolParams)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        var agentId = Guid.NewGuid().ToString();
        _logger.LogInformation("Executing tool: {ToolId} with params: {Params}", toolId, JsonSerializer.Serialize(toolParams));

        try
        {
            var result = await _pluginService.ExecutePluginAsync(toolId, JsonSerializer.Serialize(toolParams));
            var responseText = JsonSerializer.Serialize(result);

            return new AgentResponse
            {
                AgentId = agentId,
                AgentType = "tool",
                Status = "completed",
                Response = responseText,
                ExecutionTimeMs = 0,
                Context = toolParams,
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tool execution failed for tool {ToolId}", toolId);
            return new AgentResponse
            {
                AgentId = agentId,
                AgentType = "tool",
                Status = "failed",
                Response = $"Tool execution failed: {ex.Message}",
                ExecutionTimeMs = 0,
                Context = toolParams,
                Timestamp = DateTime.UtcNow
            };
        }
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<List<AgentResponse>> GetAgentHistoryAsync(string userId, int limit = 10)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        // For now, return empty list. In production, query from database
        return await Task.FromResult(new List<AgentResponse>());
    }

    private string GenerateSystemPrompt(string agentType)
    {
        return agentType switch
        {
            "analyst" => "You are an analytical agent specialized in data analysis, pattern recognition, and insights generation. Provide detailed, evidence-based analysis.",
            "planner" => "You are a planning agent specialized in workflow design, task sequencing, and project management. Focus on practical, executable plans.",
            "researcher" => "You are a research agent specialized in information synthesis, knowledge aggregation, and comprehensive reporting. Be thorough and cite sources.",
            "creative" => "You are a creative agent specialized in ideation, brainstorming, and innovative solution generation. Think outside the box.",
            "developer" => "You are a developer agent specialized in code generation, technical architecture, and implementation guidance. Provide concrete code examples.",
            _ => "You are a general-purpose AI agent. Analyze requests comprehensively and provide helpful, accurate responses."
        };
    }
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class AgentResponse
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string AgentId { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string AgentType { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Status { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Response { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public long ExecutionTimeMs { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public Dictionary<string, object> Context { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public DateTime Timestamp { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

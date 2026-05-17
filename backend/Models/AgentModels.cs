namespace AkisaAi.Api.Models;

public sealed record AgentDescriptor(string Id, string Name, string Description);

public sealed class AgentExecutionRequest
{
    public string Input { get; set; } = string.Empty;
}

public sealed class AgentExecutionResult
{
    public string AgentId { get; set; } = string.Empty;
    public string Output { get; set; } = string.Empty;
    public bool Success { get; set; }
}

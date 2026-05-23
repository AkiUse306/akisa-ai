namespace AkisaAi.Api.Models;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class ChatRequest
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Prompt { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? Model { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? SessionId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? AgentType { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class ChatResponse
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string RequestId { get; set; } = Guid.NewGuid().ToString();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Message { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Model { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string SessionId { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class CompareRequest
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Prompt { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class CompareResponse
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Akisa { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string ChatGpt { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Cursor { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string SessionId { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class RefreshTokenRequest
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string RefreshToken { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string UserId { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class ToolExecutionRequest
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string ToolId { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public Dictionary<string, object>? Parameters { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class TextToSpeechRequest
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Text { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? Voice { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

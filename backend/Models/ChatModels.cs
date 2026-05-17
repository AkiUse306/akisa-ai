namespace AkisaAi.Api.Models;

public sealed class ChatRequest
{
    public string Prompt { get; set; } = string.Empty;
    public string? Model { get; set; }
    public string? SessionId { get; set; }
}

public sealed class ChatResponse
{
    public string RequestId { get; set; } = Guid.NewGuid().ToString();
    public string Message { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
}

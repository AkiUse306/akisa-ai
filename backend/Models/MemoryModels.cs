namespace AkisaAi.Api.Models;

public sealed record MemoryEntry(string Id, string UserId, string Type, string Content, DateTime CreatedAt);

public sealed record ConversationMessage(string Role, string Text, DateTime Timestamp);

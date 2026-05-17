using System.Collections.Concurrent;
using AkisaAi.Api.Models;

namespace AkisaAi.Api.Data;

public sealed class InMemoryStore
{
    private readonly ConcurrentDictionary<string, User> _users = new();
    private readonly ConcurrentDictionary<string, string> _sessions = new();
    private readonly ConcurrentDictionary<string, List<ConversationMessage>> _conversations = new();
    private readonly ConcurrentDictionary<string, List<MemoryEntry>> _memories = new();

    public bool RegisterUser(RegisterRequest request, out User user)
    {
        var normalized = request.Username.Trim().ToLowerInvariant();
        if (_users.Values.Any(u => u.Username.Equals(normalized, StringComparison.OrdinalIgnoreCase)))
        {
            user = default!;
            return false;
        }

        user = new User
        {
            Username = normalized,
            DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? request.Username : request.DisplayName.Trim(),
            PasswordHash = HashPassword(request.Password),
        };

        return _users.TryAdd(user.Id, user);
    }

    public bool ValidateCredentials(LoginRequest request, out User user)
    {
        user = _users.Values.FirstOrDefault(u => u.Username.Equals(request.Username.Trim(), StringComparison.OrdinalIgnoreCase))!;
        return user is not null && VerifyPassword(request.Password, user.PasswordHash);
    }

    public string CreateSession(string userId)
    {
        var sessionId = Guid.NewGuid().ToString();
        _sessions[sessionId] = userId;
        return sessionId;
    }

    public bool TryGetUserIdForSession(string sessionId, out string? userId)
    {
        return _sessions.TryGetValue(sessionId, out userId);
    }

    public string? GetUserIdByName(string username)
    {
        return _users.Values.FirstOrDefault(u => u.Username.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase))?.Id;
    }

    public User? GetUserById(string userId)
    {
        return _users.TryGetValue(userId, out var user) ? user : null;
    }

    public void AddConversationMessage(string sessionId, string role, string text)
    {
        var message = new ConversationMessage(role, text, DateTime.UtcNow);
        var conversation = _conversations.GetOrAdd(sessionId, _ => new List<ConversationMessage>());
        lock (conversation)
        {
            conversation.Add(message);
            if (conversation.Count > 50)
            {
                conversation.RemoveRange(0, conversation.Count - 50);
            }
        }
    }

    public IReadOnlyList<ConversationMessage> GetConversation(string sessionId)
    {
        return _conversations.TryGetValue(sessionId, out var conversation)
            ? conversation.AsReadOnly()
            : Array.Empty<ConversationMessage>();
    }

    public void AddMemoryEntry(string userId, string type, string content)
    {
        var entry = new MemoryEntry(Guid.NewGuid().ToString(), userId, type, content, DateTime.UtcNow);
        var memoryList = _memories.GetOrAdd(userId, _ => new List<MemoryEntry>());
        lock (memoryList)
        {
            memoryList.Add(entry);
            if (memoryList.Count > 200)
            {
                memoryList.RemoveRange(0, memoryList.Count - 200);
            }
        }
    }

    public IReadOnlyList<MemoryEntry> GetRecentMemory(string userId, int count = 10)
    {
        return _memories.TryGetValue(userId, out var memory)
            ? memory.OrderByDescending(entry => entry.CreatedAt).Take(count).ToList().AsReadOnly()
            : Array.Empty<MemoryEntry>();
    }

    private static string HashPassword(string password)
    {
        // Simple hash for local development; replace with a secure algorithm in production.
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(password);
        return Convert.ToHexString(sha256.ComputeHash(bytes));
    }

    private static bool VerifyPassword(string providedPassword, string storedHash)
    {
        var candidate = HashPassword(providedPassword);
        return string.Equals(candidate, storedHash, StringComparison.OrdinalIgnoreCase);
    }
}

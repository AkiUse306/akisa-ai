using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using AkisaAi.Api.Models;

namespace AkisaAi.Api.Data;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class InMemoryStore
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
    private readonly ConcurrentDictionary<string, User> _users = new();
    private readonly ConcurrentDictionary<string, string> _sessions = new();
    private readonly ConcurrentDictionary<string, List<ConversationMessage>> _conversations = new();
    private readonly ConcurrentDictionary<string, List<MemoryEntry>> _memories = new();
    private readonly string _storagePath;
    private readonly object _storageLock = new();

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public InMemoryStore()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        var dataFolder = Path.Combine(Directory.GetCurrentDirectory(), "data");
        Directory.CreateDirectory(dataFolder);
        _storagePath = Path.Combine(dataFolder, "store.json");
        LoadState();
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool RegisterUser(RegisterRequest request, out User user)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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

        var added = _users.TryAdd(user.Id, user);
        if (added) SaveState();
        return added;
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool ValidateCredentials(LoginRequest request, out User user)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        user = _users.Values.FirstOrDefault(u => u.Username.Equals(request.Username.Trim(), StringComparison.OrdinalIgnoreCase))!;
        return user is not null && VerifyPassword(request.Password, user.PasswordHash);
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string CreateSession(string userId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        var sessionId = Guid.NewGuid().ToString();
        _sessions[sessionId] = userId;
        SaveState();
        return sessionId;
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool TryGetUserIdForSession(string sessionId, out string? userId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        return _sessions.TryGetValue(sessionId, out userId);
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? GetUserIdByName(string username)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        return _users.Values.FirstOrDefault(u => u.Username.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase))?.Id;
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public User? GetUserById(string userId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        return _users.TryGetValue(userId, out var user) ? user : null;
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public void AddConversationMessage(string sessionId, string role, string text)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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

        SaveState();
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public IReadOnlyList<ConversationMessage> GetConversation(string sessionId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        return _conversations.TryGetValue(sessionId, out var conversation)
            ? conversation.AsReadOnly()
            : Array.Empty<ConversationMessage>();
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public void AddMemoryEntry(string userId, string type, string content)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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

        SaveState();
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public IReadOnlyList<MemoryEntry> GetRecentMemory(string userId, int count = 10)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        return _memories.TryGetValue(userId, out var memory)
            ? memory.OrderByDescending(entry => entry.CreatedAt).Take(count).ToList().AsReadOnly()
            : Array.Empty<MemoryEntry>();
    }

    private void SaveState()
    {
        lock (_storageLock)
        {
            var state = new StoreState(
                _users.Values.ToList(),
                _sessions.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                _conversations.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                _memories.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };

            File.WriteAllText(_storagePath, JsonSerializer.Serialize(state, options));
        }
    }

    private void LoadState()
    {
        if (!File.Exists(_storagePath))
        {
            return;
        }

        try
        {
            var json = File.ReadAllText(_storagePath);
            var state = JsonSerializer.Deserialize<StoreState>(json);
            if (state is null)
            {
                return;
            }

            foreach (var user in state.Users)
            {
                _users[user.Id] = user;
            }

            foreach (var kvp in state.Sessions)
            {
                _sessions[kvp.Key] = kvp.Value;
            }

            foreach (var kvp in state.Conversations)
            {
                _conversations[kvp.Key] = kvp.Value;
            }

            foreach (var kvp in state.Memories)
            {
                _memories[kvp.Key] = kvp.Value;
            }
        }
        catch
        {
            // If the store file is corrupted, continue with an in-memory fallback.
        }
    }

    private static string HashPassword(string password)
    {
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

internal sealed record StoreState(
    List<User> Users,
    Dictionary<string, string> Sessions,
    Dictionary<string, List<ConversationMessage>> Conversations,
    Dictionary<string, List<MemoryEntry>> Memories);

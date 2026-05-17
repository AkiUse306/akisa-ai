using System.Linq;
using System.Security.Cryptography;
using System.Text;
using AkisaAi.Api.Data;
using AkisaAi.Api.Models;

namespace AkisaAi.Api.Services;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class VectorMemoryService
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
    private readonly InMemoryStore _store;
    private readonly OpenAiService _openAiService;
    private readonly ILogger<VectorMemoryService> _logger;
    private readonly List<VectorMemoryEntry> _vectorEntries = new();
    private readonly object _lock = new();

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public VectorMemoryService(InMemoryStore store, OpenAiService openAiService, ILogger<VectorMemoryService> logger)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        _store = store;
        _openAiService = openAiService;
        _logger = logger;
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task AddMemoryAsync(string userId, string content, string type)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        var vector = await CreateVectorAsync(content);
        var entry = new VectorMemoryEntry(Guid.NewGuid().ToString(), userId, content, vector, DateTime.UtcNow, type);

        lock (_lock)
        {
            _vectorEntries.Add(entry);
        }

        _logger.LogDebug("Added semantic memory entry for user {UserId} with type {Type}", userId, type);
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<IReadOnlyList<MemoryEntry>> SearchMemoryAsync(string userId, string query, int top = 5)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        var queryVector = await CreateVectorAsync(query);
        List<VectorMemoryEntry> candidates;

        lock (_lock)
        {
            candidates = _vectorEntries.Where(e => e.UserId == userId).ToList();
        }

        var scored = candidates
            .Select(entry => new
            {
                Score = CosineSimilarity(queryVector, entry.Vector),
                Entry = entry
            })
            .OrderByDescending(x => x.Score)
            .Take(top)
            .Where(x => x.Score > 0)
            .Select(x => new MemoryEntry(x.Entry.Id, x.Entry.UserId, x.Entry.Type, x.Entry.Content, x.Entry.CreatedAt))
            .ToList()
            .AsReadOnly();

        if (!scored.Any())
        {
            return _store.GetRecentMemory(userId, top);
        }

        return scored;
    }

    private async Task<double[]> CreateVectorAsync(string text)
    {
        if (_openAiService.IsConfigured)
        {
            try
            {
                return await _openAiService.CreateEmbeddingAsync(text);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "OpenAI embeddings failed, falling back to local semantic vectors.");
            }
        }

        return CreateLocalVector(text);
    }

    private static double[] CreateLocalVector(string text)
    {
        var normalized = new string(text
            .ToLowerInvariant()
            .Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))
            .ToArray());

        var tokens = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var vector = new double[64];

        foreach (var token in tokens)
        {
            var hash = MurmurHash3(token);
            var index = Math.Abs(hash % vector.Length);
            vector[index] += 1.0;
        }

        var magnitude = Math.Sqrt(vector.Sum(value => value * value));
        if (magnitude > 0)
        {
            for (var i = 0; i < vector.Length; i++)
            {
                vector[i] /= magnitude;
            }
        }

        return vector;
    }

    private static double CosineSimilarity(double[] a, double[] b)
    {
        if (a.Length != b.Length)
        {
            return 0.0;
        }

        var dot = 0.0;
        var magA = 0.0;
        var magB = 0.0;

        for (var i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            magA += a[i] * a[i];
            magB += b[i] * b[i];
        }

        if (magA == 0 || magB == 0)
        {
            return 0.0;
        }

        return dot / (Math.Sqrt(magA) * Math.Sqrt(magB));
    }

    private static int MurmurHash3(string value)
    {
        var data = Encoding.UTF8.GetBytes(value);
        using var hash = SHA256.Create();
        var bytes = hash.ComputeHash(data);
        return BitConverter.ToInt32(bytes, 0);
    }

    private sealed record VectorMemoryEntry(string Id, string UserId, string Content, double[] Vector, DateTime CreatedAt, string Type);
}

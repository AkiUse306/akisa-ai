using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace AkisaAi.VectorMemory;

public sealed class VectorMemoryStore
{
    private readonly ConcurrentDictionary<string, float[]> _embeddings = new();
    private readonly ConcurrentDictionary<string, string> _metadata = new();

    public void AddEmbedding(string id, float[] vector, string metadata)
    {
        _embeddings[id] = Normalize(vector);
        _metadata[id] = metadata;
    }

    public IReadOnlyDictionary<string, float[]> GetAllEmbeddings() => _embeddings;

    public string? GetMetadata(string id) => _metadata.TryGetValue(id, out var value) ? value : null;

    public IReadOnlyList<(string Id, string Metadata, float Score)> Search(float[] query, int limit = 5)
    {
        var normalizedQuery = Normalize(query);
        return _embeddings
            .Select(pair => (pair.Key, Score: CosineSimilarity(normalizedQuery, pair.Value)))
            .OrderByDescending(entry => entry.Score)
            .Take(limit)
            .Select(entry => (entry.Key, GetMetadata(entry.Key) ?? string.Empty, entry.Score))
            .ToList();
    }

    private static float[] Normalize(float[] vector)
    {
        var length = MathF.Sqrt(vector.Sum(value => value * value));
        if (length < 1e-9f)
        {
            return vector;
        }

        return vector.Select(value => value / length).ToArray();
    }

    private static float CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length != b.Length)
        {
            return 0f;
        }

        var dot = 0f;
        for (var i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
        }

        return dot;
    }
}

using System;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace AkisaAi.Api.Services;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class RedisService
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<RedisService> _logger;
    private readonly bool _isAvailable;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public RedisService(IConfiguration configuration, ILogger<RedisService> logger)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        _logger = logger;
        _redis = null;
        var redisUrl = configuration["Redis:Url"] ?? "localhost:6379";

        try
        {
            var options = ConfigurationOptions.Parse(redisUrl);
            options.AbortOnConnectFail = false;
            options.ConnectTimeout = 5000;
            _redis = ConnectionMultiplexer.Connect(options);
            _isAvailable = _redis.IsConnected;

            if (_isAvailable)
            {
                _logger.LogInformation("Redis connected successfully at {RedisUrl}", redisUrl);
            }
            else
            {
                _logger.LogWarning("Redis is not available at {RedisUrl}. Using fallback mode.", redisUrl);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to connect to Redis at {RedisUrl}. Using fallback mode.", redisUrl);
            _isAvailable = false;
        }
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsAvailable => _isAvailable && _redis?.IsConnected == true;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<bool> SetAsync(string key, string value, TimeSpan? expiry = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        if (!IsAvailable) return false;

        try
        {
            var db = _redis.GetDatabase();
            await db.StringSetAsync(key, value, expiry);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set Redis key {Key}", key);
            return false;
        }
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<string?> GetAsync(string key)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        if (!IsAvailable) return null;

        try
        {
            var db = _redis.GetDatabase();
            var value = await db.StringGetAsync(key);
            return value.IsNull ? null : value.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get Redis key {Key}", key);
            return null;
        }
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<bool> DeleteAsync(string key)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        if (!IsAvailable) return false;

        try
        {
            var db = _redis.GetDatabase();
            return await db.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete Redis key {Key}", key);
            return false;
        }
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<bool> ExistsAsync(string key)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        if (!IsAvailable) return false;

        try
        {
            var db = _redis.GetDatabase();
            return await db.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check Redis key {Key}", key);
            return false;
        }
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<T?> GetJsonAsync<T>(string key)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        var json = await GetAsync(key);
        if (string.IsNullOrEmpty(json)) return default;

        try
        {
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize Redis JSON value for key {Key}", key);
            return default;
        }
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<bool> SetJsonAsync<T>(string key, T value, TimeSpan? expiry = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            return await SetAsync(key, json, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to serialize and set Redis JSON value for key {Key}", key);
            return false;
        }
    }
}

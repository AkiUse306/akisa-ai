using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using AkisaAi.Api.Data;
using AkisaAi.Api.Models;
using AkisaAi.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var envFile = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envFile))
{
    foreach (var rawLine in File.ReadAllLines(envFile))
    {
        var line = rawLine.Trim();
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
        {
            continue;
        }

        var separatorIndex = line.IndexOf('=');
        if (separatorIndex <= 0)
        {
            continue;
        }

        var envKey = line[..separatorIndex].Trim();
        var value = line[(separatorIndex + 1)..].Trim();
        var configKey = envKey.Replace("__", ":");
        if (!builder.Configuration.AsEnumerable().Any(pair => pair.Key == configKey))
        {
            builder.Configuration[configKey] = value;
        }
    }
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalDev", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .WithOrigins("http://localhost:3000", "http://127.0.0.1:3000");
    });
});

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

builder.Services.AddSingleton<InMemoryStore>();
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddScoped<OpenAiService>();
builder.Services.AddScoped<ModelRouterService>();
builder.Services.AddScoped<VectorMemoryService>();
builder.Services.AddScoped<PluginService>();
builder.Services.AddScoped<ComparisonService>();
builder.Services.AddScoped<AiService>();
builder.Services.AddScoped<VisionService>();

var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "akisa-ai-development-super-secret-key";
var key = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateLifetime = true
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors("LocalDev");
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "AKISA-AI backend running" }));

app.MapPost("/api/auth/register", (RegisterRequest request, InMemoryStore store, JwtTokenService tokenService) =>
{
    if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest(new { message = "Username and password are required." });
    }

    if (!store.RegisterUser(request, out var user))
    {
        return Results.Conflict(new { message = "Username already exists." });
    }

    var accessToken = tokenService.CreateJwtToken(user);
    var sessionId = store.CreateSession(user.Id);
    return Results.Ok(new TokenResponse(accessToken, string.Empty, user.Id, sessionId));
});

app.MapPost("/api/auth/login", (LoginRequest request, InMemoryStore store, JwtTokenService tokenService) =>
{
    if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest(new { message = "Username and password are required." });
    }

    if (!store.ValidateCredentials(request, out var user))
    {
        return Results.Unauthorized();
    }

    var accessToken = tokenService.CreateJwtToken(user);
    var sessionId = store.CreateSession(user.Id);
    return Results.Ok(new TokenResponse(accessToken, string.Empty, user.Id, sessionId));
});

app.MapGet("/api/auth/me", (ClaimsPrincipal user, InMemoryStore store) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId))
    {
        return Results.Unauthorized();
    }

    var profile = store.GetUserById(userId);
    return profile is null ? Results.NotFound() : Results.Ok(new { profile.Id, profile.Username, profile.DisplayName, profile.Role, profile.CreatedAt });
}).RequireAuthorization();

app.MapPost("/api/chat", async (ChatRequest request, ClaimsPrincipal user, AiService aiService) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId))
    {
        return Results.Unauthorized();
    }

    var response = await aiService.CreateChatResponseAsync(request, userId);
    return Results.Ok(response);
}).RequireAuthorization();

app.MapGet("/api/agents", (AiService aiService) => Results.Ok(aiService.GetAvailableAgents())).RequireAuthorization();

app.MapPost("/api/agents/{agentId}/execute", async (string agentId, AgentExecutionRequest request, ClaimsPrincipal user, AiService aiService) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId))
    {
        return Results.Unauthorized();
    }

    var result = await aiService.ExecuteAgentAsync(agentId, request.Input, userId);
    return Results.Ok(result);
}).RequireAuthorization();

app.MapGet("/api/memory/recent", (ClaimsPrincipal user, InMemoryStore store) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId))
    {
        return Results.Unauthorized();
    }

    var memory = store.GetRecentMemory(userId, 10);
    return Results.Ok(memory);
}).RequireAuthorization();

app.MapGet("/api/memory/search", async (string query, ClaimsPrincipal user, VectorMemoryService vectorMemory) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId))
    {
        return Results.Unauthorized();
    }

    var memory = await vectorMemory.SearchMemoryAsync(userId, query, 10);
    return Results.Ok(memory);
}).RequireAuthorization();

app.MapGet("/api/conversations/{sessionId}", (string sessionId, InMemoryStore store, ClaimsPrincipal user) =>
{
    if (!store.TryGetUserIdForSession(sessionId, out var userId) || user.FindFirstValue(ClaimTypes.NameIdentifier) != userId)
    {
        return Results.Unauthorized();
    }

    var conversation = store.GetConversation(sessionId);
    return Results.Ok(conversation);
}).RequireAuthorization();

app.MapGet("/api/plugins", (PluginService pluginService) => Results.Ok(pluginService.GetPlugins())).RequireAuthorization();

app.MapPost("/api/plugins/{pluginId}/execute", async (string pluginId, PluginExecutionRequest request, PluginService pluginService, ClaimsPrincipal user) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId))
    {
        return Results.Unauthorized();
    }

    var result = await pluginService.ExecutePluginAsync(pluginId, request.Input);
    return Results.Ok(result);
}).RequireAuthorization();

app.MapPost("/api/compare", async (CompareRequest request, ComparisonService comparisonService) =>
{
    if (string.IsNullOrWhiteSpace(request.Prompt))
    {
        return Results.BadRequest(new { message = "Prompt is required for comparison." });
    }

    var result = await comparisonService.CompareAsync(request.Prompt);
    return Results.Ok(result);
});

app.MapPost("/api/vision/analyze-image", async (HttpRequest request, VisionService visionService, ClaimsPrincipal user) =>
{
    if (!request.HasFormContentType || request.Form.Files.Count == 0)
    {
        return Results.BadRequest(new { message = "Image file is required." });
    }

    var file = request.Form.Files[0];
    var result = await visionService.AnalyzeImageAsync(file);
    return Results.Ok(result);
}).RequireAuthorization();

app.MapPost("/api/vision/ocr", async (HttpRequest request, VisionService visionService, ClaimsPrincipal user) =>
{
    if (!request.HasFormContentType || request.Form.Files.Count == 0)
    {
        return Results.BadRequest(new { message = "Image file is required." });
    }

    var file = request.Form.Files[0];
    var result = await visionService.ExtractTextAsync(file);
    return Results.Ok(result);
}).RequireAuthorization();

app.MapPost("/api/vision/analyze-document", async (HttpRequest request, VisionService visionService, ClaimsPrincipal user) =>
{
    if (!request.HasFormContentType || request.Form.Files.Count == 0)
    {
        return Results.BadRequest(new { message = "Document file is required." });
    }

    var file = request.Form.Files[0];
    var result = await visionService.AnalyzeDocumentAsync(file);
    return Results.Ok(result);
}).RequireAuthorization();

app.MapHub<AiHub>("/realtime/ai");

app.Run();

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class AiHub : Hub
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task SendIfReady(string message)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        await Clients.All.SendAsync("ReceiveMessage", message);
    }
}

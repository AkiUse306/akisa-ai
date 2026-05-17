using System.IO;
using System.Linq;
using System.Text.Json;
using AkisaAi.Api.Models;

namespace AkisaAi.Api.Services;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class PluginService
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
    private readonly OpenAiService _openAiService;
    private readonly ILogger<PluginService> _logger;
    private readonly IReadOnlyList<PluginManifest> _plugins;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public PluginService(OpenAiService openAiService, ILogger<PluginService> logger)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        _openAiService = openAiService;
        _logger = logger;
        _plugins = LoadPlugins();
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public IReadOnlyList<PluginManifest> GetPlugins() => _plugins;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<PluginExecutionResult> ExecutePluginAsync(string pluginId, string input)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        var manifest = _plugins.FirstOrDefault(plugin => plugin.Id.Equals(pluginId, StringComparison.OrdinalIgnoreCase));
        if (manifest is null)
        {
            return new PluginExecutionResult
            {
                PluginId = pluginId,
                Output = "Plugin not found.",
                Success = false
            };
        }

        var action = manifest.Actions.FirstOrDefault();
        if (action is null)
        {
            return new PluginExecutionResult
            {
                PluginId = pluginId,
                Output = "No plugin action available.",
                Success = false
            };
        }

        var output = action.Id switch
        {
            "summarize-text" => await SummarizeTextAsync(input),
            _ => $"Plugin '{manifest.DisplayName}' does not support the requested action yet."
        };

        return new PluginExecutionResult
        {
            PluginId = pluginId,
            Output = output,
            Success = true
        };
    }

    private IReadOnlyList<PluginManifest> LoadPlugins()
    {
        try
        {
            var root = GetRepositoryRoot();
            var manifestPath = Path.Combine(root, "plugins", "sample-plugin.json");
            if (!File.Exists(manifestPath))
            {
                _logger.LogWarning("Plugin manifest not found at {ManifestPath}", manifestPath);
                return Array.Empty<PluginManifest>();
            }

            var fileText = File.ReadAllText(manifestPath);
            var manifest = JsonSerializer.Deserialize<PluginManifest>(fileText);
            return manifest is null ? Array.Empty<PluginManifest>() : new[] { manifest };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load plugin manifests.");
            return Array.Empty<PluginManifest>();
        }
    }

    private async Task<string> SummarizeTextAsync(string input)
    {
        if (_openAiService.IsConfigured)
        {
            try
            {
                return await _openAiService.CreateChatCompletionAsync($"Summarize the following text in a concise way:\n\n{input}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "OpenAI summarization failed, using local fallback.");
            }
        }

        return GenerateLocalSummary(input);
    }

    private static string GenerateLocalSummary(string input)
    {
        var lines = input.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var summaryLines = lines.Take(4).ToList();
        if (summaryLines.Count == 0)
        {
            return "No content was provided for summarization.";
        }

        return "Summary:\n" + string.Join(" \n", summaryLines.Select(line => $"- {line.Trim()}"));
    }

    private static string GetRepositoryRoot()
    {
        var baseDir = AppContext.BaseDirectory;
        return Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
    }
}

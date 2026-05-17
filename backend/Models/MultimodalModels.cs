namespace AkisaAi.Api.Models;

public sealed class ImageAnalysisResult
{
    public string FileName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IReadOnlyList<string> Tags { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> Objects { get; set; } = Array.Empty<string>();
}

public sealed class OcrResult
{
    public string FileName { get; set; } = string.Empty;
    public string Language { get; set; } = "en";
    public string Text { get; set; } = string.Empty;
}

public sealed class FileAnalysisResult
{
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public bool ContainsText { get; set; }
}

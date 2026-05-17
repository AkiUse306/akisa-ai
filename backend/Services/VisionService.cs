using AkisaAi.Api.Models;
using Microsoft.AspNetCore.Http;

namespace AkisaAi.Api.Services;

public sealed class VisionService
{
    private readonly ILogger<VisionService> _logger;

    public VisionService(ILogger<VisionService> logger)
    {
        _logger = logger;
    }

    public Task<ImageAnalysisResult> AnalyzeImageAsync(IFormFile image)
    {
        var tags = new[] { "AI platform", "vision", "scene", "analysis" };
        var objects = new[] { "interface", "text block", "icon" };
        var description = $"This image appears to contain an AI workspace interface with contextual elements and actionable prompts.";

        _logger.LogInformation("Analyzing image {FileName}", image.FileName);

        return Task.FromResult(new ImageAnalysisResult
        {
            FileName = image.FileName,
            Description = description,
            Tags = tags,
            Objects = objects
        });
    }

    public Task<OcrResult> ExtractTextAsync(IFormFile image)
    {
        var text = "Sample OCR output: AKISA-AI demo text, user prompt, and interface labels.";

        _logger.LogInformation("Extracting text from image {FileName}", image.FileName);

        return Task.FromResult(new OcrResult
        {
            FileName = image.FileName,
            Language = "en",
            Text = text
        });
    }

    public Task<FileAnalysisResult> AnalyzeDocumentAsync(IFormFile document)
    {
        var fileType = document.ContentType;
        var summary = document.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
            ? "PDF document detected. The file contains structured content, headings, and likely technical AI platform documentation."
            : document.FileName.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
                ? "Markdown file detected. It contains documentation or specification contents suitable for AI processing."
                : "Document file detected. The format is supported for summarization and semantic extraction.";

        _logger.LogInformation("Analyzing document {FileName} of type {FileType}", document.FileName, fileType);

        return Task.FromResult(new FileAnalysisResult
        {
            FileName = document.FileName,
            FileType = fileType,
            Summary = summary,
            ContainsText = true
        });
    }
}

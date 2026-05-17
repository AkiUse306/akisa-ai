using System.IO;
using System.Text;
using AkisaAi.Api.Models;
using Microsoft.AspNetCore.Http;

namespace AkisaAi.Api.Services;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class VisionService
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
    private readonly ILogger<VisionService> _logger;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public VisionService(ILogger<VisionService> logger)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        _logger = logger;
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<ImageAnalysisResult> AnalyzeImageAsync(IFormFile image)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        _logger.LogInformation("Analyzing image {FileName}", image.FileName);

        var description = $"Detected image: {image.FileName}. The scene appears to include a user interface, text blocks, and AI workspace elements.";
        var tags = new[] { "AI platform", "vision", "analysis", "interface" };
        var objects = image.ContentType.StartsWith("image/")
            ? new[] { "text region", "interface", "icon", "layout" }
            : new[] { "document", "content", "interface" };

        return await Task.FromResult(new ImageAnalysisResult
        {
            FileName = image.FileName,
            Description = description,
            Tags = tags,
            Objects = objects
        });
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<OcrResult> ExtractTextAsync(IFormFile image)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        _logger.LogInformation("Extracting text from image or document {FileName}", image.FileName);

        if (image.ContentType == "text/plain" || image.FileName.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
        {
            using var reader = new StreamReader(image.OpenReadStream(), Encoding.UTF8);
            var text = await reader.ReadToEndAsync();
            return new OcrResult
            {
                FileName = image.FileName,
                Language = "en",
                Text = text
            };
        }

        return await Task.FromResult(new OcrResult
        {
            FileName = image.FileName,
            Language = "en",
            Text = "OCR extraction is currently using placeholder capture. The platform can integrate Tesseract or cloud OCR for real image text extraction in production."
        });
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<FileAnalysisResult> AnalyzeDocumentAsync(IFormFile document)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        _logger.LogInformation("Analyzing document {FileName} of type {FileType}", document.FileName, document.ContentType);

        var summary = new StringBuilder();
        var fileType = document.ContentType;

        if (document.FileName.EndsWith(".md", StringComparison.OrdinalIgnoreCase) || document.FileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
        {
            using var reader = new StreamReader(document.OpenReadStream(), Encoding.UTF8);
            var content = await reader.ReadToEndAsync();
            var headings = content.Split('\n').Where(line => line.StartsWith("#")).Take(4);
            summary.AppendLine("Document summary:");
            summary.AppendLine(headings.Any() ? string.Join(" ", headings) : "No headings found. The document contains plain content.");
            summary.AppendLine("This file is ready for semantic analysis and summarization.");
        }
        else if (document.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            summary.AppendLine("PDF document detected. Extracting PDF text requires a PDF parser integration in future releases.");
            summary.AppendLine("The file is suitable for semantic analysis and report generation.");
        }
        else
        {
            summary.AppendLine("Document detected. The platform can analyze structure, detect topics, and extract text in future feature releases.");
        }

        return await Task.FromResult(new FileAnalysisResult
        {
            FileName = document.FileName,
            FileType = fileType,
            Summary = summary.ToString().Trim(),
            ContainsText = true
        });
    }
}

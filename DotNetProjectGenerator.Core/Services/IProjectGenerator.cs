namespace DotNetProjectGenerator.Core.Services;

public interface IProjectGenerator
{
    Task<bool> InitializeProjectAsync(string projectName, string pattern, string testFramework, string ciPipeline, string? outputDirectory = null);
    Task<bool> CustomizeProjectAsync(string projectPath, IEnumerable<string> features);
    Task<bool> GenerateFromPdfAsync(string pdfPath, string projectName);
} 
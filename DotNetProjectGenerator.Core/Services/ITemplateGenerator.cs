using System.Threading.Tasks;

namespace DotNetProjectGenerator.Core.Services;

public interface ITemplateGenerator
{
    Task<bool> GenerateProjectAsync(string projectName, string pattern, string testFramework, string ciPipeline, string outputDirectory);
} 
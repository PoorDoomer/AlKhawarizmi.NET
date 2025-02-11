using System.Threading.Tasks;
using System.Collections.Generic;
using DotNetProjectGenerator.Core.Models;

namespace DotNetProjectGenerator.Core.Services
{
    public interface IProjectAnalyzer
    {
        Task<ProjectStructure> AnalyzeProjectAsync(string projectPath);
        Task<bool> ValidateEntityAsync(string projectPath, string entityName);
        Task<string> FindEntityLocationAsync(string projectPath, string entityName);
        Task<Dictionary<string, string>> IdentifyProjectFoldersAsync(string projectPath, string pattern);
    }
} 
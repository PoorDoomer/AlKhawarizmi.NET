using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Spectre.Console;
using DotNetProjectGenerator.Core.Models;

namespace DotNetProjectGenerator.Core.Services
{
    public class ProjectAnalyzer : IProjectAnalyzer
    {
        public async Task<ProjectStructure> AnalyzeProjectAsync(string projectPath)
        {
            var structure = new ProjectStructure
            {
                ProjectRoot = projectPath
            };

            try
            {
                // Find all .cs files
                var files = Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    var relativePath = Path.GetRelativePath(projectPath, file);
                    
                    // Identify file type
                    if (file.Contains("Controller"))
                        structure.ControllerFiles.Add(relativePath);
                    else if (file.Contains("Service"))
                        structure.ServiceFiles.Add(relativePath);
                    else if (file.Contains("Repository"))
                        structure.RepositoryFiles.Add(relativePath);
                    else if (file.Contains("Command") || file.Contains("Query"))
                        structure.CqrsFiles.Add(relativePath);
                    else if (file.Contains("Entity") || file.Contains("Model"))
                        structure.EntityFiles.Add(relativePath);
                }

                // Detect patterns
                structure.Patterns = new List<string>();
                if (structure.CqrsFiles.Any())
                    structure.Patterns.Add("CQRS");
                if (structure.ServiceFiles.Any() && structure.RepositoryFiles.Any())
                    structure.Patterns.Add("Service Repository");
                if (structure.EntityFiles.Any(f => f.Contains("Domain")))
                    structure.Patterns.Add("Clean Architecture");
                if (structure.EntityFiles.Any(f => f.Contains("Aggregate")))
                    structure.Patterns.Add("Domain-Driven Design");
                if (!structure.ControllerFiles.Any() && structure.EntityFiles.Any())
                    structure.Patterns.Add("Minimal API");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error analyzing project: {ex.Message}[/]");
            }

            return structure;
        }

        public async Task<bool> ValidateEntityAsync(string projectPath, string entityName)
        {
            try
            {
                var modelFiles = Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories)
                    .Where(f => f.Contains("Models") || f.Contains("Entities") || f.Contains("Domain"));

                foreach (var file in modelFiles)
                {
                    var content = await File.ReadAllTextAsync(file);
                    if (content.Contains($"class {entityName}"))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error validating entity: {ex.Message}[/]");
            }

            return false;
        }

        public async Task<string> FindEntityLocationAsync(string projectPath, string entityName)
        {
            try
            {
                var files = Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    var content = await File.ReadAllTextAsync(file);
                    if (content.Contains($"class {entityName}"))
                    {
                        return file;
                    }
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error finding entity: {ex.Message}[/]");
            }

            return string.Empty;
        }

        public async Task<Dictionary<string, string>> IdentifyProjectFoldersAsync(string projectPath, string pattern)
        {
            var folders = new Dictionary<string, string>();
            var possibleFolders = new List<string>();

            try
            {
                // Get all directories
                var directories = Directory.GetDirectories(projectPath, "*", SearchOption.AllDirectories);

                switch (pattern.ToLower())
                {
                    case "cqrs":
                        possibleFolders.AddRange(new[] { 
                            "Commands", "Queries", "Handlers", "Models", "Controllers" 
                        });
                        break;

                    case "service repository":
                        possibleFolders.AddRange(new[] { 
                            "Services", "Repositories", "Models", "Controllers", "Interfaces" 
                        });
                        break;

                    case "clean architecture":
                        possibleFolders.AddRange(new[] { 
                            "Domain", "Application", "Infrastructure", "Presentation",
                            "Entities", "Interfaces", "Services", "Controllers" 
                        });
                        break;

                    case "minimal api":
                        possibleFolders.AddRange(new[] { 
                            "Endpoints", "Models", "Services" 
                        });
                        break;

                    case "domain-driven design":
                        possibleFolders.AddRange(new[] { 
                            "Domain", "Application", "Infrastructure", "Api",
                            "Aggregates", "ValueObjects", "Repositories", "Services" 
                        });
                        break;
                }

                // For each required folder type
                foreach (var folderType in possibleFolders)
                {
                    var matchingDirs = directories
                        .Where(d => Path.GetFileName(d).Contains(folderType, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (matchingDirs.Count == 0)
                    {
                        // No matching directory found
                        folders[folderType] = string.Empty;
                    }
                    else if (matchingDirs.Count == 1)
                    {
                        // One matching directory found
                        folders[folderType] = matchingDirs[0];
                    }
                    else
                    {
                        // Multiple matching directories found - take the first one
                        folders[folderType] = matchingDirs[0];
                    }
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error identifying project folders: {ex.Message}[/]");
            }

            return folders;
        }
    }
} 
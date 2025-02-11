using System;
using System.Text;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Spectre.Console;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using DotNetProjectGenerator.Core.Models;

namespace DotNetProjectGenerator.Core.Services
{
    public interface IBestPracticesGenerator
    {
        Task GenerateDocumentationAsync(string projectPath);
    }

    public class BestPracticesGenerator : IBestPracticesGenerator
    {
        private readonly ITemplateGenerator _templateGenerator;

        public BestPracticesGenerator(ITemplateGenerator templateGenerator)
        {
            _templateGenerator = templateGenerator;
        }

        public async Task GenerateDocumentationAsync(string projectPath)
        {
            var documentation = new StringBuilder();
            documentation.AppendLine("# Best Practices Documentation 📚\n");

            // Analyze project structure
            await AnalyzeProjectStructure(projectPath, documentation);

            // Analyze code quality
            await AnalyzeCodeQuality(projectPath, documentation);

            // Generate recommendations
            GenerateRecommendations(documentation);

            // Save documentation
            var docPath = Path.Combine(projectPath, "docs", "BestPractices.md");
            await File.WriteAllTextAsync(docPath, documentation.ToString());
        }

        private async Task AnalyzeProjectStructure(string projectPath, StringBuilder documentation)
        {
            documentation.AppendLine("## Project Structure Analysis 🏗️\n");

            // Check layer separation
            var layers = new[] { "Domain", "Application", "Infrastructure", "WebApi" };
            foreach (var layer in layers)
            {
                var layerPath = Path.Combine(projectPath, layer);
                if (Directory.Exists(layerPath))
                {
                    documentation.AppendLine($"### {layer} Layer ✓");
                    documentation.AppendLine("```");
                    await AddDirectoryStructure(layerPath, documentation, "  ");
                    documentation.AppendLine("```\n");
                }
            }
        }

        private async Task AnalyzeCodeQuality(string projectPath, StringBuilder documentation)
        {
            documentation.AppendLine("## Code Quality Analysis 🔍\n");

            var metrics = new Dictionary<string, int>
            {
                { "Total Files", 0 },
                { "Classes with XML Documentation", 0 },
                { "Public APIs", 0 },
                { "Test Files", 0 }
            };

            foreach (var file in Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories))
            {
                metrics["Total Files"]++;

                var content = await File.ReadAllTextAsync(file);
                var tree = CSharpSyntaxTree.ParseText(content);
                var root = await tree.GetRootAsync();

                // Count documented classes
                if (root.DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .Any(c => c.HasLeadingTrivia && 
                             c.GetLeadingTrivia()
                              .Any(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))))
                {
                    metrics["Classes with XML Documentation"]++;
                }

                // Count public APIs
                metrics["Public APIs"] += root.DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Count(m => m.Modifiers.Any(mod => mod.IsKind(SyntaxKind.PublicKeyword)));

                // Count test files
                if (file.Contains(".Tests.") || file.EndsWith("Tests.cs"))
                {
                    metrics["Test Files"]++;
                }
            }

            documentation.AppendLine("### Metrics\n");
            foreach (var metric in metrics)
            {
                documentation.AppendLine($"- **{metric.Key}**: {metric.Value}");
            }
            documentation.AppendLine();
        }

        private void GenerateRecommendations(StringBuilder documentation)
        {
            documentation.AppendLine("## Recommendations 💡\n");

            documentation.AppendLine("### Architecture\n");
            documentation.AppendLine("1. ✅ **Dependency Injection**");
            documentation.AppendLine("   - Use constructor injection for required dependencies");
            documentation.AppendLine("   - Register services with appropriate lifetimes\n");

            documentation.AppendLine("2. ✅ **SOLID Principles**");
            documentation.AppendLine("   - Single Responsibility: Each class should have one reason to change");
            documentation.AppendLine("   - Open/Closed: Open for extension, closed for modification");
            documentation.AppendLine("   - Liskov Substitution: Derived classes must be substitutable");
            documentation.AppendLine("   - Interface Segregation: Keep interfaces focused and cohesive");
            documentation.AppendLine("   - Dependency Inversion: Depend on abstractions\n");

            documentation.AppendLine("### Security\n");
            documentation.AppendLine("1. 🔒 **Authentication & Authorization**");
            documentation.AppendLine("   - Use JWT tokens with appropriate expiration");
            documentation.AppendLine("   - Implement role-based access control");
            documentation.AppendLine("   - Secure sensitive endpoints\n");

            documentation.AppendLine("2. 🛡️ **Data Protection**");
            documentation.AppendLine("   - Use HTTPS everywhere");
            documentation.AppendLine("   - Implement input validation");
            documentation.AppendLine("   - Protect against XSS and CSRF\n");

            documentation.AppendLine("### Performance\n");
            documentation.AppendLine("1. ⚡ **Caching**");
            documentation.AppendLine("   - Use response caching for static content");
            documentation.AppendLine("   - Implement distributed caching for scalability");
            documentation.AppendLine("   - Cache expensive computations\n");

            documentation.AppendLine("2. 📈 **Database**");
            documentation.AppendLine("   - Use async/await consistently");
            documentation.AppendLine("   - Implement proper indexing");
            documentation.AppendLine("   - Use efficient queries\n");

            documentation.AppendLine("### Testing\n");
            documentation.AppendLine("1. 🧪 **Unit Tests**");
            documentation.AppendLine("   - Test business logic thoroughly");
            documentation.AppendLine("   - Use mocking appropriately");
            documentation.AppendLine("   - Follow AAA pattern\n");

            documentation.AppendLine("2. 🔄 **Integration Tests**");
            documentation.AppendLine("   - Test critical paths");
            documentation.AppendLine("   - Use in-memory database when possible");
            documentation.AppendLine("   - Test external service integration\n");
        }

        private async Task AddDirectoryStructure(string path, StringBuilder sb, string indent)
        {
            foreach (var dir in Directory.GetDirectories(path))
            {
                sb.AppendLine($"{indent}{Path.GetFileName(dir)}/");
                await AddDirectoryStructure(dir, sb, indent + "  ");
            }

            foreach (var file in Directory.GetFiles(path))
            {
                sb.AppendLine($"{indent}{Path.GetFileName(file)}");
            }
        }
    }
} 
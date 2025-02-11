using CommandLine;

namespace DotNetProjectGenerator.Core.Models;

[Verb("init", HelpText = "Initialize a new .NET project with clean architecture.")]
public class InitOptions
{
    [Option('n', "name", Required = true, HelpText = "The name of the project to create.")]
    public string ProjectName { get; set; } = string.Empty;

    [Option('o', "output", Required = false, HelpText = "Output directory for the project (optional)")]
    public string? OutputDirectory { get; set; }

    [Option('p', "pattern", Required = false, Default = "clean", HelpText = "The architectural pattern to use (clean, ddd, cqrs).")]
    public string Pattern { get; set; } = "clean";

    [Option('t', "test", Required = false, Default = "xunit", HelpText = "The testing framework to use (xunit, nunit, mstest).")]
    public string TestFramework { get; set; } = "xunit";

    [Option('c', "ci", Required = false, Default = "github", HelpText = "The CI/CD pipeline to generate (github, azure, gitlab).")]
    public string CiPipeline { get; set; } = "github";
}

[Verb("customize", HelpText = "Add or modify features in an existing project.")]
public class CustomizeOptions
{
    [Option('p', "path", Required = true, HelpText = "The path to the project to customize.")]
    public string ProjectPath { get; set; } = string.Empty;

    [Option('f', "features", Required = false, HelpText = "Comma-separated list of features to add (logging,caching,auth).")]
    public string Features { get; set; } = string.Empty;
}

[Verb("from-pdf", HelpText = "Generate a project structure from a PDF specification.")]
public class PdfOptions
{
    [Option('f', "file", Required = true, HelpText = "The path to the PDF specification file.")]
    public string PdfPath { get; set; } = string.Empty;

    [Option('n', "name", Required = true, HelpText = "The name of the project to create.")]
    public string ProjectName { get; set; } = string.Empty;
} 
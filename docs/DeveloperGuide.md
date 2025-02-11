# AlKhawarizmi.NET Developer Guide 🚀

Welcome to the AlKhawarizmi.NET developer guide! This document will help you understand the project structure and how to extend it with new features.

## Table of Contents
- [Project Structure](#project-structure)
- [Core Concepts](#core-concepts)
- [Adding New Features](#adding-new-features)
- [Template System](#template-system)
- [UI/UX Customization](#uiux-customization)
- [Best Practices](#best-practices)

## Project Structure

```
AlKhawarizmi.NET/
├── src/
│   ├── DotNetProjectGenerator.Core/        # Core functionality
│   │   ├── Models/                         # Data models
│   │   ├── Services/                       # Business logic
│   │   └── Templates/                      # Project templates
│   └── DotNetProjectGenerator.Cli/         # CLI interface
├── tests/                                  # Test projects
└── docs/                                   # Documentation
```

### Key Components

1. **ProjectGenerator**: Main service that orchestrates project creation
2. **TemplateGenerator**: Handles template-based file generation
3. **CLI Interface**: User interaction layer using Spectre.Console

## Core Concepts

### Project Generation Flow

1. **User Input**: Collect project configuration
2. **Solution Creation**: Initialize .NET solution
3. **Project Creation**: Create project structure based on architecture pattern
4. **Template Application**: Generate code from templates
5. **Customization**: Apply selected features

### Architecture Patterns

Currently supported patterns:
- Clean Architecture
- Domain-Driven Design (DDD)
- Command Query Responsibility Segregation (CQRS)

## Adding New Features

### 1. Adding a New Architecture Pattern

1. Create template files in `Templates/{PatternName}/`:
```
Templates/NewPattern/
├── Domain/
├── Application/
├── Infrastructure/
└── Api/
```

2. Add pattern-specific templates
3. Implement generation method in `TemplateGenerator`:

```csharp
public async Task GenerateNewPatternAsync(string projectName, string outputPath)
{
    var replacements = new Dictionary<string, string>
    {
        { "ProjectName", projectName }
    };

    // Generate files from templates
    await GenerateFromTemplateAsync(
        "NewPattern/Domain/Entity.cs.template",
        Path.Combine(outputPath, $"{projectName}.Domain/Entity.cs"),
        replacements);
    
    // Add more template generations...
}
```

4. Add pattern to `ProjectGenerator`:

```csharp
case "newpattern":
    await CreateNewPatternProjectsAsync(projectName);
    break;
```

### 2. Adding a New Feature

1. Create feature templates in `Templates/CrossCutting/{FeatureName}/`
2. Implement feature addition method in `ProjectGenerator`:

```csharp
private async Task AddNewFeatureAsync(string projectPath)
{
    var projectName = Path.GetFileName(projectPath);
    var featureDir = Path.Combine(projectPath, "Infrastructure/NewFeature");
    Directory.CreateDirectory(featureDir);

    // Generate feature files
    var content = await File.ReadAllTextAsync("Templates/CrossCutting/NewFeature/Feature.cs.template");
    content = content.Replace("{ProjectName}", projectName);
    await File.WriteAllTextAsync(Path.Combine(featureDir, "Feature.cs"), content);

    // Add required NuGet packages
    await ExecuteCommandAsync("dotnet", $"add {projectPath}/Infrastructure/{projectName}.Infrastructure.csproj package RequiredPackage");
}
```

3. Add feature to the customization options

### 3. Adding UI Elements

The project uses Spectre.Console for rich terminal UI. To add new UI elements:

```csharp
// Add custom spinners
private readonly string[] _customSpinners = { "🌸", "✨", "💫" };

// Create custom panels
var panel = new Panel($"[pink1]Your Content[/]")
{
    Border = BoxBorder.Rounded,
    Padding = new Padding(1, 1),
};

// Add progress bars
await AnsiConsole.Progress()
    .AutoClear(false)
    .Columns(new ProgressColumn[]
    {
        new SpinnerColumn(_customSpinners),
        new TaskDescriptionColumn(),
        new ProgressBarColumn(),
        new PercentageColumn()
    })
    .StartAsync(async ctx => { /* ... */ });
```

## Template System

### Template Structure

Templates use a simple replacement system:
- Use `{PlaceholderName}` in templates
- Define replacements in Dictionary
- Support for:
  - Project name
  - Namespace
  - Custom values

Example template:
```csharp
namespace {ProjectName}.Domain
{
    public class {EntityName} : Entity
    {
        // Properties
    }
}
```

### Creating New Templates

1. Create `.template` file in appropriate directory
2. Use placeholders for dynamic content
3. Register template in generator
4. Add replacements dictionary

## UI/UX Customization

### Colors and Styles

```csharp
// Define custom colors
var customStyle = Style.Parse("pink1 bold");

// Create styled rule
var rule = new Rule("[pink1]Title[/]");
rule.Style = customStyle;

// Use markup
AnsiConsole.MarkupLine("[pink1]Kawaii Message[/] ✨");
```

### Animations

```csharp
// Custom spinners
private readonly string[] _spinners = { "🌸", "💫", "✨" };

// Animated status
await AnsiConsole.Status()
    .StartAsync("Working... ✨", async ctx =>
    {
        ctx.Spinner(Spinner.Known.Star);
        ctx.SpinnerStyle(Style.Parse("pink1"));
        await Task.Delay(1000);
    });
```

## Best Practices

1. **Template Organization**
   - Keep templates modular
   - Use consistent naming
   - Document placeholder usage

2. **Error Handling**
   - Provide clear error messages
   - Add appropriate logging
   - Handle edge cases

3. **Code Generation**
   - Follow .NET conventions
   - Maintain consistent style
   - Add comments in generated code

4. **UI/UX**
   - Keep interface responsive
   - Provide clear progress indication
   - Use consistent styling

5. **Testing**
   - Add unit tests for new features
   - Test template generation
   - Verify error handling

## Contributing

1. Fork the repository
2. Create feature branch
3. Add your feature/template
4. Add tests
5. Submit pull request

## Need Help?

- Check existing templates for examples
- Review test cases
- Open an issue for questions
- Join our community discussions 
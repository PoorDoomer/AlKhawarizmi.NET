using System.Diagnostics;
using Spectre.Console;
using System.Reflection;

namespace DotNetProjectGenerator.Core.Services;

public class ProjectGenerator : IProjectGenerator
{
    private readonly TemplateGenerator _templateGenerator;
    private readonly string[] _kawaiiFaces = { "(｡♥‿♥｡)", "(◕‿◕✿)", "(ﾉ◕ヮ◕)ﾉ*:･ﾟ✧", "(★^O^★)", "(◠‿◠)", "ʕ•ᴥ•ʔ" };
    private readonly Spinner _kawaiiSpinner;
    private readonly string[] _spinnerFrames = new[] { "🌸", "🎀", "💫", "✨", "🌟", "⭐" };
    private readonly string _templatesPath;

    public ProjectGenerator()
    {
        _templateGenerator = new TemplateGenerator();
        // Create a custom spinner with kawaii characters
        _kawaiiSpinner = Spinner.Known.Star;

        // Initialize templates path
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
        _templatesPath = Path.Combine(assemblyDirectory!, "Templates");

        if (!Directory.Exists(_templatesPath))
        {
            AnsiConsole.MarkupLine($"[red]Warning: Templates directory not found at {_templatesPath}[/]");
            throw new DirectoryNotFoundException($"Templates directory not found at {_templatesPath}");
        }
    }

    private string GetTemplatePath(string relativePath)
    {
        var fullPath = Path.Combine(_templatesPath, relativePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Template file not found: {fullPath}");
        }
        return fullPath;
    }

    private async Task<string> ReadTemplateAsync(string relativePath)
    {
        var fullPath = GetTemplatePath(relativePath);
        return await File.ReadAllTextAsync(fullPath);
    }

    private string GetRandomKawaiiEmoji() => _kawaiiFaces[Random.Shared.Next(_kawaiiFaces.Length)];

    private string GetRandomSpinnerFrame() => _spinnerFrames[Random.Shared.Next(_spinnerFrames.Length)];

    public async Task<bool> InitializeProjectAsync(
        string projectName, 
        string pattern, 
        string testFramework, 
        string ciPipeline,
        string? outputDirectory = null)
    {
        try
        {
            // Show welcome banner
            var rule = new Rule($"[pink1]Welcome to AlKhawarizmi.NET {GetRandomKawaiiEmoji()}[/]");
            rule.Style = Style.Parse("pink1");
            AnsiConsole.Write(rule);
            
            // Resolve and validate the project directory
            string projectDirectory;
            if (!string.IsNullOrWhiteSpace(outputDirectory))
            {
                // Convert relative path to absolute if necessary
                projectDirectory = Path.GetFullPath(Path.Combine(outputDirectory, projectName));
                
                // Create the full directory path if it doesn't exist
                Directory.CreateDirectory(Path.GetDirectoryName(projectDirectory)!);
            }
            else
            {
                projectDirectory = Path.Combine(Directory.GetCurrentDirectory(), projectName);
            }

            // Show project info in a panel
            var panel = new Panel($"""
                [pink1]Project Name:[/] {projectName}
                [pink1]Location:[/] {projectDirectory}
                [pink1]Architecture:[/] {pattern}
                [pink1]Test Framework:[/] {testFramework}
                [pink1]CI/CD Pipeline:[/] {ciPipeline}
                """)
            {
                Border = BoxBorder.Rounded,
                Padding = new Padding(1, 1),
            };
            panel.Header = new PanelHeader("Project Configuration ✨");
            AnsiConsole.Write(panel);

            // Use a single progress display for all operations
            await AnsiConsole.Progress()
                .AutoClear(false)
                .Columns(new ProgressColumn[]
                {
                    new SpinnerColumn(Spinner.Known.Star).Style(Style.Parse("pink1")),
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new PercentageColumn()
                })
                .StartAsync(async ctx =>
                {
                    // Create project directory
                    var dirTask = ctx.AddTask($"[pink1]Creating project directory {GetRandomSpinnerFrame()}[/]");
                    Directory.CreateDirectory(projectDirectory);
                    Directory.SetCurrentDirectory(projectDirectory); // Change to project directory
                    await Task.Delay(500); // Add a small delay for visual effect
                    dirTask.Value = 100;

                    // Store the absolute paths we'll need throughout the process
                    var absoluteProjectDir = Directory.GetCurrentDirectory(); // Get the absolute path after changing directory
                    var solutionFile = Path.Combine(absoluteProjectDir, $"{projectName}.sln");

                    // Create solution
                    var solutionTask = ctx.AddTask($"[pink1]Creating solution {GetRandomSpinnerFrame()}[/]");
                    await ExecuteCommandAsync("dotnet", $"new sln -n {projectName}");
                    solutionTask.Value = 100;

                    // Create projects based on pattern
                    var projectsTask = ctx.AddTask($"[pink1]Creating {pattern} architecture projects {GetRandomSpinnerFrame()}[/]");
                    switch (pattern.ToLower())
                    {
                        case "clean":
                            await CreateCleanArchitectureProjectsAsync(projectName, solutionFile);
                            break;
                        case "ddd":
                            await CreateDddProjectsAsync(projectName, solutionFile);
                            break;
                        case "cqrs":
                            await CreateCqrsProjectsAsync(projectName, solutionFile);
                            break;
                        default:
                            throw new ArgumentException($"Unsupported pattern: {pattern}");
                    }
                    projectsTask.Value = 100;

                    // Create test project
                    var testTask = ctx.AddTask($"[pink1]Setting up {testFramework} test project {GetRandomSpinnerFrame()}[/]");
                    await CreateTestProjectAsync(projectName, testFramework, solutionFile);
                    testTask.Value = 100;

                    // Set up CI/CD pipeline
                    var ciTask = ctx.AddTask($"[pink1]Configuring {ciPipeline} CI/CD pipeline {GetRandomSpinnerFrame()}[/]");
                    await CreateCiPipelineAsync(projectName, ciPipeline);
                    ciTask.Value = 100;
                });

            // Show success message with the absolute path
            var successPanel = new Panel(
                $"[green]Your project has been created successfully! {GetRandomKawaiiEmoji()}[/]\n" +
                $"[grey]Location: {projectDirectory}[/]")
            {
                Border = BoxBorder.Double,
                Padding = new Padding(1, 1),
            };
            successPanel.Header = new PanelHeader("[pink1]✨ Project Created ✨[/]");
            AnsiConsole.Write(successPanel);

            return true;
        }
        catch (Exception ex)
        {
            var errorPanel = new Panel(
                $"[red]Error during project initialization:[/]\n{ex.Message}")
            {
                Border = BoxBorder.Heavy,
                BorderStyle = Style.Parse("red"),
            };
            AnsiConsole.Write(errorPanel);
            AnsiConsole.WriteException(ex);
            return false;
        }
    }

    public async Task<bool> CustomizeProjectAsync(string projectPath, IEnumerable<string> features)
    {
        try
        {
            foreach (var feature in features)
            {
                switch (feature.ToLower())
                {
                    case "logging":
                        await AddLoggingAsync(projectPath);
                        break;
                    case "caching":
                        await AddCachingAsync(projectPath);
                        break;
                    case "auth":
                        await AddAuthenticationAsync(projectPath);
                        break;
                    case "errorhandling":
                        await AddErrorHandlingAsync(projectPath);
                        break;
                    default:
                        throw new ArgumentException($"Unsupported feature: {feature}");
                }
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> GenerateFromPdfAsync(string pdfPath, string projectName)
    {
        try
        {
            // TODO: Implement PDF analysis and schema generation
            await Task.Delay(100); // Placeholder
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private async Task CreateCleanArchitectureProjectsAsync(string projectName, string solutionFile)
    {
        try
        {
            AnsiConsole.MarkupLine("[yellow]Creating Clean Architecture projects...[/]");

            // Store the absolute paths
            var projectDir = Directory.GetCurrentDirectory();
            var absoluteSolutionPath = Path.GetFullPath(solutionFile);

            // Create projects
            AnsiConsole.MarkupLine("[grey]Creating Domain project...[/]");
            await ExecuteCommandAsync("dotnet", $"new classlib -n {projectName}.Domain -o \"{Path.Combine(projectDir, $"{projectName}.Domain")}\"");
            
            AnsiConsole.MarkupLine("[grey]Creating Application project...[/]");
            await ExecuteCommandAsync("dotnet", $"new classlib -n {projectName}.Application -o \"{Path.Combine(projectDir, $"{projectName}.Application")}\"");
            
            AnsiConsole.MarkupLine("[grey]Creating Infrastructure project...[/]");
            await ExecuteCommandAsync("dotnet", $"new classlib -n {projectName}.Infrastructure -o \"{Path.Combine(projectDir, $"{projectName}.Infrastructure")}\"");
            
            AnsiConsole.MarkupLine("[grey]Creating API project...[/]");
            await ExecuteCommandAsync("dotnet", $"new webapi -n {projectName}.Api -o \"{Path.Combine(projectDir, $"{projectName}.Api")}\"");

            // Add to solution using absolute paths
            AnsiConsole.MarkupLine("[grey]Adding projects to solution...[/]");
            var domainProjPath = Path.Combine(projectDir, $"{projectName}.Domain", $"{projectName}.Domain.csproj");
            var applicationProjPath = Path.Combine(projectDir, $"{projectName}.Application", $"{projectName}.Application.csproj");
            var infrastructureProjPath = Path.Combine(projectDir, $"{projectName}.Infrastructure", $"{projectName}.Infrastructure.csproj");
            var apiProjPath = Path.Combine(projectDir, $"{projectName}.Api", $"{projectName}.Api.csproj");

            await ExecuteCommandAsync("dotnet", $"sln \"{absoluteSolutionPath}\" add \"{domainProjPath}\"");
            await ExecuteCommandAsync("dotnet", $"sln \"{absoluteSolutionPath}\" add \"{applicationProjPath}\"");
            await ExecuteCommandAsync("dotnet", $"sln \"{absoluteSolutionPath}\" add \"{infrastructureProjPath}\"");
            await ExecuteCommandAsync("dotnet", $"sln \"{absoluteSolutionPath}\" add \"{apiProjPath}\"");

            // Add project references using absolute paths
            AnsiConsole.MarkupLine("[grey]Setting up project references...[/]");
            await ExecuteCommandAsync("dotnet", $"add \"{applicationProjPath}\" reference \"{domainProjPath}\"");
            await ExecuteCommandAsync("dotnet", $"add \"{infrastructureProjPath}\" reference \"{applicationProjPath}\"");
            await ExecuteCommandAsync("dotnet", $"add \"{apiProjPath}\" reference \"{infrastructureProjPath}\"");

            // Generate template files
            AnsiConsole.MarkupLine("[grey]Generating template files...[/]");
            await _templateGenerator.GenerateCleanArchitectureAsync(projectName, projectDir);

            // Add required NuGet packages
            AnsiConsole.MarkupLine("[grey]Installing NuGet packages...[/]");
            await ExecuteCommandAsync("dotnet", $"add \"{infrastructureProjPath}\" package Microsoft.EntityFrameworkCore.SqlServer");
            await ExecuteCommandAsync("dotnet", $"add \"{apiProjPath}\" package Swashbuckle.AspNetCore");

            AnsiConsole.MarkupLine("[green]Clean Architecture projects created successfully![/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error creating Clean Architecture projects: {ex.Message}[/]");
            throw;
        }
    }

    private async Task CreateDddProjectsAsync(string projectName, string solutionFile)
    {
        // Create projects
        await ExecuteCommandAsync("dotnet", $"new classlib -n {projectName}.Domain");
        await ExecuteCommandAsync("dotnet", $"new classlib -n {projectName}.Application");
        await ExecuteCommandAsync("dotnet", $"new classlib -n {projectName}.Infrastructure");
        await ExecuteCommandAsync("dotnet", $"new webapi -n {projectName}.Api");

        // Add to solution
        await ExecuteCommandAsync("dotnet", $"sln \"{solutionFile}\" add {projectName}.Domain/{projectName}.Domain.csproj");
        await ExecuteCommandAsync("dotnet", $"sln \"{solutionFile}\" add {projectName}.Application/{projectName}.Application.csproj");
        await ExecuteCommandAsync("dotnet", $"sln \"{solutionFile}\" add {projectName}.Infrastructure/{projectName}.Infrastructure.csproj");
        await ExecuteCommandAsync("dotnet", $"sln \"{solutionFile}\" add {projectName}.Api/{projectName}.Api.csproj");

        // Add project references
        await ExecuteCommandAsync("dotnet", $"add {projectName}.Application/{projectName}.Application.csproj reference {projectName}.Domain/{projectName}.Domain.csproj");
        await ExecuteCommandAsync("dotnet", $"add {projectName}.Infrastructure/{projectName}.Infrastructure.csproj reference {projectName}.Application/{projectName}.Application.csproj");
        await ExecuteCommandAsync("dotnet", $"add {projectName}.Api/{projectName}.Api.csproj reference {projectName}.Infrastructure/{projectName}.Infrastructure.csproj");

        // Generate template files
        await _templateGenerator.GenerateDddArchitectureAsync(projectName, ".");

        // Add required NuGet packages
        await ExecuteCommandAsync("dotnet", $"add {projectName}.Domain/{projectName}.Domain.csproj package MediatR");
        await ExecuteCommandAsync("dotnet", $"add {projectName}.Infrastructure/{projectName}.Infrastructure.csproj package Microsoft.EntityFrameworkCore.SqlServer");
        await ExecuteCommandAsync("dotnet", $"add {projectName}.Infrastructure/{projectName}.Infrastructure.csproj package MediatR");
        await ExecuteCommandAsync("dotnet", $"add {projectName}.Infrastructure/{projectName}.Infrastructure.csproj package MediatR.Extensions.Microsoft.DependencyInjection");
        await ExecuteCommandAsync("dotnet", $"add {projectName}.Api/{projectName}.Api.csproj package Swashbuckle.AspNetCore");
    }

    private async Task CreateCqrsProjectsAsync(string projectName, string solutionFile)
    {
        // Create projects
        await ExecuteCommandAsync("dotnet", $"new classlib -n {projectName}.Domain");
        await ExecuteCommandAsync("dotnet", $"new classlib -n {projectName}.Application");
        await ExecuteCommandAsync("dotnet", $"new classlib -n {projectName}.Infrastructure");
        await ExecuteCommandAsync("dotnet", $"new webapi -n {projectName}.Api");

        // Add to solution
        await ExecuteCommandAsync("dotnet", $"sln \"{solutionFile}\" add {projectName}.Domain/{projectName}.Domain.csproj");
        await ExecuteCommandAsync("dotnet", $"sln \"{solutionFile}\" add {projectName}.Application/{projectName}.Application.csproj");
        await ExecuteCommandAsync("dotnet", $"sln \"{solutionFile}\" add {projectName}.Infrastructure/{projectName}.Infrastructure.csproj");
        await ExecuteCommandAsync("dotnet", $"sln \"{solutionFile}\" add {projectName}.Api/{projectName}.Api.csproj");

        // Add project references
        await ExecuteCommandAsync("dotnet", $"add {projectName}.Application/{projectName}.Application.csproj reference {projectName}.Domain/{projectName}.Domain.csproj");
        await ExecuteCommandAsync("dotnet", $"add {projectName}.Infrastructure/{projectName}.Infrastructure.csproj reference {projectName}.Application/{projectName}.Application.csproj");
        await ExecuteCommandAsync("dotnet", $"add {projectName}.Api/{projectName}.Api.csproj reference {projectName}.Infrastructure/{projectName}.Infrastructure.csproj");

        // Generate template files
        await _templateGenerator.GenerateCqrsArchitectureAsync(projectName, ".");

        // Add required NuGet packages
        await ExecuteCommandAsync("dotnet", $"add {projectName}.Domain/{projectName}.Domain.csproj package MediatR");
        await ExecuteCommandAsync("dotnet", $"add {projectName}.Application/{projectName}.Application.csproj package MediatR");
        await ExecuteCommandAsync("dotnet", $"add {projectName}.Application/{projectName}.Application.csproj package MediatR.Extensions.Microsoft.DependencyInjection");
        await ExecuteCommandAsync("dotnet", $"add {projectName}.Infrastructure/{projectName}.Infrastructure.csproj package Microsoft.EntityFrameworkCore.SqlServer");
        await ExecuteCommandAsync("dotnet", $"add {projectName}.Api/{projectName}.Api.csproj package Swashbuckle.AspNetCore");
    }

    private async Task CreateTestProjectAsync(string projectName, string framework, string solutionFile)
    {
        var template = framework.ToLower() switch
        {
            "xunit" => "xunit",
            "nunit" => "nunit",
            "mstest" => "mstest",
            _ => throw new ArgumentException($"Unsupported test framework: {framework}")
        };

        var projectDir = Directory.GetCurrentDirectory();
        var absoluteSolutionPath = Path.GetFullPath(solutionFile);
        var testsProjPath = Path.Combine(projectDir, $"{projectName}.Tests");
        var testsProjFile = Path.Combine(testsProjPath, $"{projectName}.Tests.csproj");
        var domainProjPath = Path.Combine(projectDir, $"{projectName}.Domain", $"{projectName}.Domain.csproj");
        var applicationProjPath = Path.Combine(projectDir, $"{projectName}.Application", $"{projectName}.Application.csproj");

        await ExecuteCommandAsync("dotnet", $"new {template} -n {projectName}.Tests -o \"{testsProjPath}\"");
        await ExecuteCommandAsync("dotnet", $"sln \"{absoluteSolutionPath}\" add \"{testsProjFile}\"");
        await ExecuteCommandAsync("dotnet", $"add \"{testsProjFile}\" reference \"{domainProjPath}\"");
        await ExecuteCommandAsync("dotnet", $"add \"{testsProjFile}\" reference \"{applicationProjPath}\"");
    }

    private async Task CreateCiPipelineAsync(string projectName, string pipeline)
    {
        var workflowsDir = ".github/workflows";
        var azurePipelinesFile = "azure-pipelines.yml";
        var gitlabCiFile = ".gitlab-ci.yml";

        try
        {
            switch (pipeline.ToLower())
            {
                case "github":
                    Directory.CreateDirectory(workflowsDir);
                    var githubWorkflowContent = await ReadTemplateAsync("CI/github-workflow.yml.template");
                    await File.WriteAllTextAsync(Path.Combine(workflowsDir, "build.yml"), githubWorkflowContent);
                    break;

                case "azure":
                    var azurePipelineContent = await ReadTemplateAsync("CI/azure-pipelines.yml.template");
                    await File.WriteAllTextAsync(azurePipelinesFile, azurePipelineContent);
                    break;

                case "gitlab":
                    var gitlabCiContent = await ReadTemplateAsync("CI/gitlab-ci.yml.template");
                    await File.WriteAllTextAsync(gitlabCiFile, gitlabCiContent);
                    break;

                default:
                    throw new ArgumentException($"Unsupported CI pipeline: {pipeline}");
            }

            // Generate Docker and Kubernetes configurations
            await GenerateDockerConfigurationAsync(projectName);
            await GenerateKubernetesManifestsAsync(projectName);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error creating CI/CD pipeline: {ex.Message}[/]");
            throw;
        }
    }

    private async Task GenerateDockerConfigurationAsync(string projectName)
    {
        try
        {
            var dockerfileContent = await ReadTemplateAsync("Docker/Dockerfile.template");
            dockerfileContent = dockerfileContent.Replace("{ProjectName}", projectName);
            await File.WriteAllTextAsync("Dockerfile", dockerfileContent);

            var dockerComposeContent = await ReadTemplateAsync("Docker/docker-compose.yml.template");
            await File.WriteAllTextAsync("docker-compose.yml", dockerComposeContent);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error generating Docker configuration: {ex.Message}[/]");
            throw;
        }
    }

    private async Task GenerateKubernetesManifestsAsync(string projectName)
    {
        try
        {
            Directory.CreateDirectory("kubernetes");
            var deploymentContent = await ReadTemplateAsync("Kubernetes/deployment.yml.template");
            deploymentContent = deploymentContent.Replace("{ProjectName}", projectName);
            await File.WriteAllTextAsync("kubernetes/deployment.yml", deploymentContent);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error generating Kubernetes manifests: {ex.Message}[/]");
            throw;
        }
    }

    private async Task AddLoggingAsync(string projectPath)
    {
        try
        {
            var loggingContent = await ReadTemplateAsync("CrossCutting/Logging/LoggingExtensions.cs.template");
            loggingContent = loggingContent.Replace("{ProjectName}", Path.GetFileName(projectPath));
            
            var targetDir = Path.Combine(projectPath, "Infrastructure/Logging");
            Directory.CreateDirectory(targetDir);
            await File.WriteAllTextAsync(Path.Combine(targetDir, "LoggingExtensions.cs"), loggingContent);

            // Add required NuGet packages
            var projectName = Path.GetFileName(projectPath);
            await ExecuteCommandAsync("dotnet", $"add {projectPath}/Infrastructure/{projectName}.Infrastructure.csproj package Serilog");
            await ExecuteCommandAsync("dotnet", $"add {projectPath}/Infrastructure/{projectName}.Infrastructure.csproj package Serilog.Sinks.Console");
            await ExecuteCommandAsync("dotnet", $"add {projectPath}/Infrastructure/{projectName}.Infrastructure.csproj package Serilog.Sinks.File");
            await ExecuteCommandAsync("dotnet", $"add {projectPath}/Infrastructure/{projectName}.Infrastructure.csproj package Serilog.Sinks.Elasticsearch");
            await ExecuteCommandAsync("dotnet", $"add {projectPath}/Infrastructure/{projectName}.Infrastructure.csproj package Serilog.Enrichers.Environment");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error adding logging feature: {ex.Message}[/]");
            throw;
        }
    }

    private async Task AddCachingAsync(string projectPath)
    {
        var projectName = Path.GetFileName(projectPath);
        var cacheDir = Path.Combine(projectPath, "Infrastructure/Caching");
        var servicesDir = Path.Combine(cacheDir, "Services");
        Directory.CreateDirectory(servicesDir);

        // Generate caching files
        var files = new Dictionary<string, string>
        {
            { "CachingExtensions.cs", "Templates/CrossCutting/Caching/CachingExtensions.cs.template" },
            { "Services/ICacheService.cs", "Templates/CrossCutting/Caching/Services/ICacheService.cs.template" },
            { "Services/RedisCacheService.cs", "Templates/CrossCutting/Caching/Services/RedisCacheService.cs.template" }
        };

        foreach (var file in files)
        {
            var content = await File.ReadAllTextAsync(file.Value);
            content = content.Replace("{ProjectName}", projectName);
            await File.WriteAllTextAsync(Path.Combine(cacheDir, file.Key), content);
        }

        // Add required NuGet packages
        await ExecuteCommandAsync("dotnet", $"add {projectPath}/Infrastructure/{projectName}.Infrastructure.csproj package Microsoft.Extensions.Caching.StackExchangeRedis");
    }

    private async Task AddAuthenticationAsync(string projectPath)
    {
        var projectName = Path.GetFileName(projectPath);
        var authDir = Path.Combine(projectPath, "Infrastructure/Authentication");
        var servicesDir = Path.Combine(authDir, "Services");
        Directory.CreateDirectory(servicesDir);

        // Generate authentication files
        var files = new Dictionary<string, string>
        {
            { "AuthenticationExtensions.cs", "Templates/CrossCutting/Authentication/AuthenticationExtensions.cs.template" },
            { "Services/JwtService.cs", "Templates/CrossCutting/Authentication/Services/JwtService.cs.template" }
        };

        foreach (var file in files)
        {
            var content = await File.ReadAllTextAsync(file.Value);
            content = content.Replace("{ProjectName}", projectName);
            await File.WriteAllTextAsync(Path.Combine(authDir, file.Key), content);
        }

        // Add required NuGet packages
        await ExecuteCommandAsync("dotnet", $"add {projectPath}/Infrastructure/{projectName}.Infrastructure.csproj package Microsoft.AspNetCore.Authentication.JwtBearer");
    }

    private async Task AddErrorHandlingAsync(string projectPath)
    {
        var projectName = Path.GetFileName(projectPath);
        var errorHandlingDir = Path.Combine(projectPath, "Infrastructure/ErrorHandling");
        var middlewareDir = Path.Combine(errorHandlingDir, "Middleware");
        var modelsDir = Path.Combine(errorHandlingDir, "Models");
        var exceptionsDir = Path.Combine(errorHandlingDir, "Exceptions");

        Directory.CreateDirectory(middlewareDir);
        Directory.CreateDirectory(modelsDir);
        Directory.CreateDirectory(exceptionsDir);

        // Generate error handling files
        var files = new Dictionary<string, string>
        {
            { "ErrorHandlingExtensions.cs", "Templates/CrossCutting/ErrorHandling/ErrorHandlingExtensions.cs.template" },
            { "Middleware/ExceptionHandlingMiddleware.cs", "Templates/CrossCutting/ErrorHandling/Middleware/ExceptionHandlingMiddleware.cs.template" },
            { "Models/ErrorResponse.cs", "Templates/CrossCutting/ErrorHandling/Models/ErrorResponse.cs.template" },
            { "Exceptions/ApplicationException.cs", "Templates/CrossCutting/ErrorHandling/Exceptions/ApplicationException.cs.template" }
        };

        foreach (var file in files)
        {
            var content = await File.ReadAllTextAsync(file.Value);
            content = content.Replace("{ProjectName}", projectName);
            await File.WriteAllTextAsync(Path.Combine(errorHandlingDir, file.Key), content);
        }
    }

    private async Task<int> ExecuteCommandAsync(string command, string arguments)
    {
        try
        {
            AnsiConsole.MarkupLine($"[grey]Executing: {command} {arguments}[/]");
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Directory.GetCurrentDirectory() // Ensure working directory is set
                }
            };

            process.Start();

            // Read output and error streams asynchronously
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (!string.IsNullOrEmpty(output))
                AnsiConsole.MarkupLine($"[grey]Output: {output}[/]");
            
            if (!string.IsNullOrEmpty(error))
                AnsiConsole.MarkupLine($"[red]Error: {error}[/]");

            if (process.ExitCode != 0)
                throw new Exception($"Command failed with exit code {process.ExitCode}. Error: {error}");

            return process.ExitCode;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to execute command: {command} {arguments}[/]");
            throw;
        }
    }
} 
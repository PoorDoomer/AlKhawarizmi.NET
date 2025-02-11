using System.Reflection;
using Spectre.Console;
using DotNetProjectGenerator.Core.Models;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DotNetProjectGenerator.Core.Services;

public class TemplateGenerator : ITemplateGenerator
{
    private readonly string _templatesPath;

    public TemplateGenerator()
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
        _templatesPath = Path.Combine(assemblyDirectory!, "Templates");
        
        AnsiConsole.MarkupLine($"[grey]Templates path: {_templatesPath}[/]");
        if (!Directory.Exists(_templatesPath))
        {
            AnsiConsole.MarkupLine($"[red]Warning: Templates directory not found at {_templatesPath}[/]");
            // List the contents of the assembly directory to help debug
            if (Directory.Exists(assemblyDirectory))
            {
                AnsiConsole.MarkupLine("[grey]Assembly directory contents:[/]");
                foreach (var item in Directory.GetFileSystemEntries(assemblyDirectory!))
                {
                    AnsiConsole.MarkupLine($"[grey]  {Path.GetFileName(item)}[/]");
                }
            }
        }
    }

    public async Task GenerateFromTemplateAsync(string templatePath, string outputPath, Dictionary<string, string> replacements)
    {
        try
        {
            AnsiConsole.MarkupLine($"[grey]Generating from template: {templatePath}[/]");
            AnsiConsole.MarkupLine($"[grey]Output path: {outputPath}[/]");

            // Ensure the output directory exists
            var outputDirectory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDirectory))
            {
                AnsiConsole.MarkupLine($"[grey]Creating directory: {outputDirectory}[/]");
                Directory.CreateDirectory(outputDirectory);
            }

            // Read the template
            var templateFullPath = Path.Combine(_templatesPath, templatePath);
            AnsiConsole.MarkupLine($"[grey]Reading template from: {templateFullPath}[/]");
            
            if (!File.Exists(templateFullPath))
            {
                AnsiConsole.MarkupLine($"[red]Template file not found: {templateFullPath}[/]");
                // List available templates in the directory
                var templateDir = Path.GetDirectoryName(templateFullPath);
                if (Directory.Exists(templateDir))
                {
                    AnsiConsole.MarkupLine($"[grey]Available files in {templateDir}:[/]");
                    foreach (var file in Directory.GetFiles(templateDir, "*.*", SearchOption.AllDirectories))
                    {
                        AnsiConsole.MarkupLine($"[grey]  {Path.GetRelativePath(templateDir, file)}[/]");
                    }
                }
                throw new FileNotFoundException($"Template file not found: {templateFullPath}");
            }

            var templateContent = await File.ReadAllTextAsync(templateFullPath);

            // Apply replacements
            AnsiConsole.MarkupLine("[grey]Applying template replacements...[/]");
            var outputContent = replacements.Aggregate(templateContent,
                (current, replacement) =>
                {
                    AnsiConsole.MarkupLine($"[grey]  Replacing {{{replacement.Key}}} with {replacement.Value}[/]");
                    return current.Replace($"{{{replacement.Key}}}", replacement.Value);
                });

            // Write the output file
            AnsiConsole.MarkupLine($"[grey]Writing output to: {outputPath}[/]");
            await File.WriteAllTextAsync(outputPath, outputContent);
            AnsiConsole.MarkupLine($"[green]Successfully generated: {outputPath}[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error generating template {templatePath}:[/]");
            AnsiConsole.WriteException(ex);
            throw;
        }
    }

    public async Task GenerateCleanArchitectureAsync(string projectName, string outputPath)
    {
        var replacements = new Dictionary<string, string>
        {
            { "ProjectName", projectName }
        };

        // Domain Layer
        await GenerateFromTemplateAsync(
            "CleanArchitecture/Domain/Entity.cs.template",
            Path.Combine(outputPath, $"{projectName}.Domain/Common/Entity.cs"),
            replacements);

        await GenerateFromTemplateAsync(
            "CleanArchitecture/Domain/ValueObject.cs.template",
            Path.Combine(outputPath, $"{projectName}.Domain/Common/ValueObject.cs"),
            replacements);

        await GenerateFromTemplateAsync(
            "CleanArchitecture/Domain/IRepository.cs.template",
            Path.Combine(outputPath, $"{projectName}.Domain/Interfaces/IRepository.cs"),
            replacements);

        // Application Layer
        await GenerateFromTemplateAsync(
            "CleanArchitecture/Application/IService.cs.template",
            Path.Combine(outputPath, $"{projectName}.Application/Interfaces/IService.cs"),
            replacements);

        await GenerateFromTemplateAsync(
            "CleanArchitecture/Application/DependencyInjection.cs.template",
            Path.Combine(outputPath, $"{projectName}.Application/DependencyInjection.cs"),
            replacements);

        // Infrastructure Layer
        await GenerateFromTemplateAsync(
            "CleanArchitecture/Infrastructure/Repository.cs.template",
            Path.Combine(outputPath, $"{projectName}.Infrastructure/Persistence/Repository.cs"),
            replacements);

        await GenerateFromTemplateAsync(
            "CleanArchitecture/Infrastructure/ApplicationDbContext.cs.template",
            Path.Combine(outputPath, $"{projectName}.Infrastructure/Persistence/ApplicationDbContext.cs"),
            replacements);

        await GenerateFromTemplateAsync(
            "CleanArchitecture/Infrastructure/DependencyInjection.cs.template",
            Path.Combine(outputPath, $"{projectName}.Infrastructure/DependencyInjection.cs"),
            replacements);

        // API Layer
        await GenerateFromTemplateAsync(
            "CleanArchitecture/Api/BaseController.cs.template",
            Path.Combine(outputPath, $"{projectName}.Api/Controllers/BaseController.cs"),
            replacements);

        await GenerateFromTemplateAsync(
            "CleanArchitecture/Api/Program.cs.template",
            Path.Combine(outputPath, $"{projectName}.Api/Program.cs"),
            replacements);

        await GenerateFromTemplateAsync(
            "CleanArchitecture/Api/appsettings.json.template",
            Path.Combine(outputPath, $"{projectName}.Api/appsettings.json"),
            replacements);
    }

    public async Task GenerateDddArchitectureAsync(string projectName, string outputPath)
    {
        var replacements = new Dictionary<string, string>
        {
            { "ProjectName", projectName }
        };

        // Domain Layer
        await GenerateFromTemplateAsync(
            "DDD/Domain/Entity.cs.template",
            Path.Combine(outputPath, $"{projectName}.Domain/Common/Entity.cs"),
            replacements);

        await GenerateFromTemplateAsync(
            "DDD/Domain/ValueObject.cs.template",
            Path.Combine(outputPath, $"{projectName}.Domain/Common/ValueObject.cs"),
            replacements);

        await GenerateFromTemplateAsync(
            "DDD/Domain/AggregateRoot.cs.template",
            Path.Combine(outputPath, $"{projectName}.Domain/Common/AggregateRoot.cs"),
            replacements);

        await GenerateFromTemplateAsync(
            "DDD/Domain/DomainEvent.cs.template",
            Path.Combine(outputPath, $"{projectName}.Domain/Common/DomainEvent.cs"),
            replacements);

        await GenerateFromTemplateAsync(
            "DDD/Domain/IAggregateRepository.cs.template",
            Path.Combine(outputPath, $"{projectName}.Domain/Common/IAggregateRepository.cs"),
            replacements);

        await GenerateFromTemplateAsync(
            "DDD/Domain/IDomainEventHandler.cs.template",
            Path.Combine(outputPath, $"{projectName}.Domain/Common/IDomainEventHandler.cs"),
            replacements);

        // Infrastructure Layer
        await GenerateFromTemplateAsync(
            "DDD/Infrastructure/AggregateRepository.cs.template",
            Path.Combine(outputPath, $"{projectName}.Infrastructure/Persistence/AggregateRepository.cs"),
            replacements);

        await GenerateFromTemplateAsync(
            "DDD/Infrastructure/DomainEventDispatcher.cs.template",
            Path.Combine(outputPath, $"{projectName}.Infrastructure/EventDispatcher/DomainEventDispatcher.cs"),
            replacements);

        await GenerateFromTemplateAsync(
            "DDD/Infrastructure/DependencyInjection.cs.template",
            Path.Combine(outputPath, $"{projectName}.Infrastructure/DependencyInjection.cs"),
            replacements);

        // Reuse Clean Architecture templates for API Layer
        await GenerateFromTemplateAsync(
            "CleanArchitecture/Api/Program.cs.template",
            Path.Combine(outputPath, $"{projectName}.Api/Program.cs"),
            replacements);

        await GenerateFromTemplateAsync(
            "CleanArchitecture/Api/appsettings.json.template",
            Path.Combine(outputPath, $"{projectName}.Api/appsettings.json"),
            replacements);
    }

    public async Task GenerateCqrsArchitectureAsync(string projectName, string outputPath)
    {
        var replacements = new Dictionary<string, string>
        {
            { "ProjectName", projectName }
        };

        // Application Layer - CQRS Base Interfaces
        await GenerateFromTemplateAsync(
            "CQRS/Application/ICommand.cs.template",
            Path.Combine(outputPath, $"{projectName}.Application/Common/Commands/ICommand.cs"),
            replacements);

        await GenerateFromTemplateAsync(
            "CQRS/Application/IQuery.cs.template",
            Path.Combine(outputPath, $"{projectName}.Application/Common/Queries/IQuery.cs"),
            replacements);

        await GenerateFromTemplateAsync(
            "CQRS/Application/ICommandHandler.cs.template",
            Path.Combine(outputPath, $"{projectName}.Application/Common/Commands/ICommandHandler.cs"),
            replacements);

        await GenerateFromTemplateAsync(
            "CQRS/Application/IQueryHandler.cs.template",
            Path.Combine(outputPath, $"{projectName}.Application/Common/Queries/IQueryHandler.cs"),
            replacements);

        // Example Implementation
        await GenerateFromTemplateAsync(
            "CQRS/Application/ExampleCommand.cs.template",
            Path.Combine(outputPath, $"{projectName}.Application/Features/Example/ExampleCommand.cs"),
            replacements);

        await GenerateFromTemplateAsync(
            "CQRS/Application/ExampleQuery.cs.template",
            Path.Combine(outputPath, $"{projectName}.Application/Features/Example/ExampleQuery.cs"),
            replacements);

        // Reuse Clean Architecture templates for API Layer
        await GenerateFromTemplateAsync(
            "CleanArchitecture/Api/Program.cs.template",
            Path.Combine(outputPath, $"{projectName}.Api/Program.cs"),
            replacements);

        await GenerateFromTemplateAsync(
            "CleanArchitecture/Api/appsettings.json.template",
            Path.Combine(outputPath, $"{projectName}.Api/appsettings.json"),
            replacements);
    }

    public async Task GenerateControllerAction(string projectDir, EndpointDetails details)
    {
        var controllerDir = Path.Combine(projectDir, "Controllers");
        Directory.CreateDirectory(controllerDir);

        var controllerContent = GenerateControllerContent(details);
        var controllerPath = Path.Combine(controllerDir, $"{GetControllerName(details)}Controller.cs");
        await File.WriteAllTextAsync(controllerPath, controllerContent);
    }

    public async Task GenerateDto(string projectDir, EndpointDetails details)
    {
        if (string.IsNullOrEmpty(details.DtoName))
            return;

        var dtoDir = Path.Combine(projectDir, "Models", "DTOs");
        Directory.CreateDirectory(dtoDir);

        var dtoContent = GenerateDtoContent(details);
        var dtoPath = Path.Combine(dtoDir, $"{details.DtoName}.cs");
        await File.WriteAllTextAsync(dtoPath, dtoContent);
    }

    public async Task GenerateValidator(string projectDir, EndpointDetails details)
    {
        if (!details.RequiresValidation)
            return;

        var validatorDir = Path.Combine(projectDir, "Validators");
        Directory.CreateDirectory(validatorDir);

        var validatorContent = GenerateValidatorContent(details);
        var validatorPath = Path.Combine(validatorDir, $"{GetValidatorName(details)}.cs");
        await File.WriteAllTextAsync(validatorPath, validatorContent);
    }

    public async Task GenerateEndpointTests(string projectDir, EndpointDetails details)
    {
        var testDir = Path.Combine(projectDir, "Tests");
        Directory.CreateDirectory(testDir);

        var testContent = GenerateTestContent(details);
        var testPath = Path.Combine(testDir, $"{GetControllerName(details)}Tests.cs");
        await File.WriteAllTextAsync(testPath, testContent);
    }

    public async Task UpdateApiDocumentation(string projectDir, EndpointDetails details)
    {
        var docsDir = Path.Combine(projectDir, "docs");
        Directory.CreateDirectory(docsDir);

        var docPath = Path.Combine(docsDir, "api.md");
        var docContent = GenerateApiDocContent(details);

        // Append to existing documentation or create new
        if (File.Exists(docPath))
        {
            await File.AppendAllTextAsync(docPath, docContent);
        }
        else
        {
            await File.WriteAllTextAsync(docPath, docContent);
        }
    }

    private string GenerateControllerContent(EndpointDetails details)
    {
        var template = @"using Microsoft.AspNetCore.Mvc;
{0}

namespace {1}.Controllers
{{
    [ApiController]
    [Route(""{2}"")]
    {3}
    public class {4}Controller : ControllerBase
    {{
        {5}
        public {6} {7}({8})
        {{
            {9}
        }}
    }}
}}";

        var usings = details.RequiresValidation ? "using FluentValidation;" : "";
        var auth = details.RequiresAuthorization ? "[Authorize]" : "";
        var cache = details.RequiresCaching ? "[ResponseCache(Duration = 60)]" : "";
        var controllerName = GetControllerName(details);
        var methodName = GetMethodName(details);
        var returnType = GetReturnType(details);
        var parameters = GetMethodParameters(details);
        var methodBody = GetMethodBody(details);

        return string.Format(template,
            usings,
            "YourNamespace",
            details.Route,
            $"{auth}\n    {cache}",
            controllerName,
            methodName,
            returnType,
            parameters,
            methodBody);
    }

    private string GenerateDtoContent(EndpointDetails details)
    {
        return $@"namespace YourNamespace.Models.DTOs
{{
    public class {details.DtoName}
    {{
        // TODO: Add DTO properties
    }}
}}";
    }

    private string GenerateValidatorContent(EndpointDetails details)
    {
        return $@"using FluentValidation;
using YourNamespace.Models.DTOs;

namespace YourNamespace.Validators
{{
    public class {GetValidatorName(details)} : AbstractValidator<{details.DtoName}>
    {{
        public {GetValidatorName(details)}()
        {{
            // TODO: Add validation rules
        }}
    }}
}}";
    }

    private string GenerateTestContent(EndpointDetails details)
    {
        return $@"using Xunit;
using YourNamespace.Controllers;
using YourNamespace.Models.DTOs;

namespace YourNamespace.Tests
{{
    public class {GetControllerName(details)}Tests
    {{
        [Fact]
        public async Task {GetMethodName(details)}_ReturnsExpectedResult()
        {{
            // Arrange
            
            // Act
            
            // Assert
        }}
    }}
}}";
    }

    private string GenerateApiDocContent(EndpointDetails details)
    {
        return $@"
## {details.HttpMethod} {details.Route}

{(details.RequiresAuthorization ? "**Requires Authentication**" : "No authentication required")}

### Request
- Method: {details.HttpMethod}
- Endpoint: `{details.Route}`
{(details.RequiresValidation ? "- Validation: Yes" : "")}
{(details.RequiresCaching ? "- Caching: Enabled (60 seconds)" : "")}

### Response
- Type: {details.ResponseType}
{(string.IsNullOrEmpty(details.DtoName) ? "" : $"- DTO: {details.DtoName}")}

---
";
    }

    private string GetControllerName(EndpointDetails details)
    {
        var segments = details.Route.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return segments.Length > 0 ? 
            char.ToUpper(segments[^1][0]) + segments[^1][1..] : 
            "Default";
    }

    private string GetMethodName(EndpointDetails details)
    {
        var action = details.Route.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? "Default";
        return $"{details.HttpMethod}{char.ToUpper(action[0])}{action[1..]}";
    }

    private string GetReturnType(EndpointDetails details)
    {
        return details.ResponseType switch
        {
            "void" => "IActionResult",
            "ActionResult" => "ActionResult",
            "IEnumerable<T>" => $"ActionResult<IEnumerable<{details.DtoName}>>",
            "Single Entity" => $"ActionResult<{details.DtoName}>",
            "Custom DTO" => $"ActionResult<{details.DtoName}>",
            _ => "IActionResult"
        };
    }

    private string GetMethodParameters(EndpointDetails details)
    {
        return details.HttpMethod switch
        {
            "POST" or "PUT" => $"[FromBody] {details.DtoName} request",
            "GET" => "int id",
            "DELETE" => "int id",
            _ => ""
        };
    }

    private string GetMethodBody(EndpointDetails details)
    {
        return details.HttpMethod switch
        {
            "GET" => "return Ok();",
            "POST" => "return CreatedAtAction(nameof(GetById), new { id = 1 }, request);",
            "PUT" => "return NoContent();",
            "DELETE" => "return NoContent();",
            _ => "return Ok();"
        };
    }

    private string GetValidatorName(EndpointDetails details)
    {
        return $"{details.DtoName}Validator";
    }

    // CRUD Entity Generation
    public async Task GenerateEntity(string projectDir, CrudDetails details)
    {
        var replacements = new Dictionary<string, string>
        {
            { "EntityName", details.EntityName },
            { "Properties", GenerateEntityProperties(details.Properties) }
        };

        var templatePath = Path.Combine("Entity", "Entity.cs.template");
        var outputPath = Path.Combine(projectDir, "Domain", "Entities", $"{details.EntityName}.cs");
        await GenerateFromTemplateAsync(templatePath, outputPath, replacements);
    }

    // CQRS Pattern Generation
    public async Task GenerateCommands(string projectDir, CrudDetails details)
    {
        await GenerateCreateCommand(projectDir, details);
        await GenerateUpdateCommand(projectDir, details);
        await GenerateDeleteCommand(projectDir, details);
    }

    public async Task GenerateQueries(string projectDir, CrudDetails details)
    {
        await GenerateGetQuery(projectDir, details);
        await GenerateGetAllQuery(projectDir, details);
    }

    public async Task GenerateHandlers(string projectDir, CrudDetails details)
    {
        var replacements = new Dictionary<string, string>
        {
            { "EntityName", details.EntityName },
            { "Properties", GenerateEntityProperties(details.Properties) }
        };

        // Generate command handlers
        await GenerateFromTemplateAsync(
            "Cqrs/Handlers/CreateEntityCommandHandler.cs.template",
            Path.Combine(projectDir, "Application", "Handlers", $"Create{details.EntityName}CommandHandler.cs"),
            replacements);

        await GenerateFromTemplateAsync(
            "Cqrs/Handlers/UpdateEntityCommandHandler.cs.template",
            Path.Combine(projectDir, "Application", "Handlers", $"Update{details.EntityName}CommandHandler.cs"),
            replacements);

        await GenerateFromTemplateAsync(
            "Cqrs/Handlers/DeleteEntityCommandHandler.cs.template",
            Path.Combine(projectDir, "Application", "Handlers", $"Delete{details.EntityName}CommandHandler.cs"),
            replacements);

        // Generate query handlers
        await GenerateFromTemplateAsync(
            "Cqrs/Handlers/GetEntityQueryHandler.cs.template",
            Path.Combine(projectDir, "Application", "Handlers", $"Get{details.EntityName}QueryHandler.cs"),
            replacements);

        await GenerateFromTemplateAsync(
            "Cqrs/Handlers/GetAllEntitiesQueryHandler.cs.template",
            Path.Combine(projectDir, "Application", "Handlers", $"GetAll{details.EntityName}sQueryHandler.cs"),
            replacements);
    }

    public async Task GenerateCqrsTests(string projectDir, CrudDetails details)
    {
        var replacements = new Dictionary<string, string>
        {
            { "EntityName", details.EntityName },
            { "Properties", GenerateTestProperties(details.Properties) }
        };

        // Generate command tests
        await GenerateFromTemplateAsync(
            "Cqrs/Tests/Commands/CreateEntityCommandTests.cs.template",
            Path.Combine(projectDir, "Tests", "Commands", $"Create{details.EntityName}CommandTests.cs"),
            replacements);

        await GenerateFromTemplateAsync(
            "Cqrs/Tests/Commands/UpdateEntityCommandTests.cs.template",
            Path.Combine(projectDir, "Tests", "Commands", $"Update{details.EntityName}CommandTests.cs"),
            replacements);

        await GenerateFromTemplateAsync(
            "Cqrs/Tests/Commands/DeleteEntityCommandTests.cs.template",
            Path.Combine(projectDir, "Tests", "Commands", $"Delete{details.EntityName}CommandTests.cs"),
            replacements);

        // Generate query tests
        await GenerateFromTemplateAsync(
            "Cqrs/Tests/Queries/GetEntityQueryTests.cs.template",
            Path.Combine(projectDir, "Tests", "Queries", $"Get{details.EntityName}QueryTests.cs"),
            replacements);

        await GenerateFromTemplateAsync(
            "Cqrs/Tests/Queries/GetAllEntitiesQueryTests.cs.template",
            Path.Combine(projectDir, "Tests", "Queries", $"GetAll{details.EntityName}sQueryTests.cs"),
            replacements);
    }

    // Service Repository Pattern Generation
    public async Task GenerateInterfaces(string projectDir, CrudDetails details)
    {
        await GenerateServiceInterface(projectDir, details);
        await GenerateRepositoryInterface(projectDir, details);
    }

    public async Task GenerateRepository(string projectDir, CrudDetails details)
    {
        var replacements = new Dictionary<string, string>
        {
            { "EntityName", details.EntityName },
            { "Properties", GenerateRepositoryMethods(details) }
        };

        await GenerateFromTemplateAsync(
            "ServiceRepository/Repositories/EntityRepository.cs.template",
            Path.Combine(projectDir, "Infrastructure", "Repositories", $"{details.EntityName}Repository.cs"),
            replacements);
    }

    public async Task GenerateService(string projectDir, CrudDetails details)
    {
        var replacements = new Dictionary<string, string>
        {
            { "EntityName", details.EntityName },
            { "Properties", GenerateServiceMethods(details) }
        };

        await GenerateFromTemplateAsync(
            "ServiceRepository/Services/EntityService.cs.template",
            Path.Combine(projectDir, "Services", $"{details.EntityName}Service.cs"),
            replacements);
    }

    public async Task GenerateController(string projectDir, CrudDetails details)
    {
        var replacements = new Dictionary<string, string>
        {
            { "EntityName", details.EntityName },
            { "Properties", GenerateControllerMethods(details) }
        };

        await GenerateFromTemplateAsync(
            "ServiceRepository/Controllers/EntityController.cs.template",
            Path.Combine(projectDir, "Controllers", $"{details.EntityName}Controller.cs"),
            replacements);
    }

    public async Task GenerateServiceTests(string projectDir, CrudDetails details)
    {
        var replacements = new Dictionary<string, string>
        {
            { "EntityName", details.EntityName },
            { "Properties", GenerateTestProperties(details.Properties) }
        };

        await GenerateFromTemplateAsync(
            "ServiceRepository/Tests/EntityServiceTests.cs.template",
            Path.Combine(projectDir, "Tests", $"{details.EntityName}ServiceTests.cs"),
            replacements);
    }

    // Clean Architecture Pattern Generation
    public async Task GenerateDomainLayer(string projectDir, CrudDetails details)
    {
        var replacements = new Dictionary<string, string>
        {
            { "EntityName", details.EntityName },
            { "Properties", GenerateEntityProperties(details.Properties) }
        };

        await GenerateFromTemplateAsync(
            "CleanArchitecture/Domain/Entity.cs.template",
            Path.Combine(projectDir, $"{details.EntityName}.Domain/Entities/{details.EntityName}.cs"),
            replacements);

        await GenerateFromTemplateAsync(
            "CleanArchitecture/Domain/IRepository.cs.template",
            Path.Combine(projectDir, $"{details.EntityName}.Domain/Interfaces/I{details.EntityName}Repository.cs"),
            replacements);
    }

    public async Task GenerateApplicationLayer(string projectDir, CrudDetails details)
    {
        var replacements = new Dictionary<string, string>
        {
            { "EntityName", details.EntityName },
            { "Properties", GenerateEntityProperties(details.Properties) }
        };

        // Generate DTOs
        await GenerateFromTemplateAsync(
            "CleanArchitecture/Application/Dtos/EntityDto.cs.template",
            Path.Combine(projectDir, $"{details.EntityName}.Application/Dtos/{details.EntityName}Dto.cs"),
            replacements);

        // Generate Interfaces
        await GenerateFromTemplateAsync(
            "CleanArchitecture/Application/Interfaces/IEntityService.cs.template",
            Path.Combine(projectDir, $"{details.EntityName}.Application/Interfaces/I{details.EntityName}Service.cs"),
            replacements);

        // Generate Services
        await GenerateFromTemplateAsync(
            "CleanArchitecture/Application/Services/EntityService.cs.template",
            Path.Combine(projectDir, $"{details.EntityName}.Application/Services/{details.EntityName}Service.cs"),
            replacements);
    }

    public async Task GenerateInfrastructureLayer(string projectDir, CrudDetails details)
    {
        var replacements = new Dictionary<string, string>
        {
            { "EntityName", details.EntityName },
            { "Properties", GenerateEntityProperties(details.Properties) }
        };

        // Generate Repository Implementation
        await GenerateFromTemplateAsync(
            "CleanArchitecture/Infrastructure/Repositories/EntityRepository.cs.template",
            Path.Combine(projectDir, $"{details.EntityName}.Infrastructure/Repositories/{details.EntityName}Repository.cs"),
            replacements);

        // Generate DbContext
        await GenerateFromTemplateAsync(
            "CleanArchitecture/Infrastructure/Data/ApplicationDbContext.cs.template",
            Path.Combine(projectDir, $"{details.EntityName}.Infrastructure/Data/ApplicationDbContext.cs"),
            replacements);
    }

    public async Task GeneratePresentationLayer(string projectDir, CrudDetails details)
    {
        var replacements = new Dictionary<string, string>
        {
            { "EntityName", details.EntityName },
            { "Properties", GenerateEntityProperties(details.Properties) }
        };

        // Generate API Controllers
        await GenerateFromTemplateAsync(
            "CleanArchitecture/Presentation/Controllers/EntityController.cs.template",
            Path.Combine(projectDir, $"{details.EntityName}.Api/Controllers/{details.EntityName}Controller.cs"),
            replacements);
    }

    public async Task GenerateCleanArchitectureTests(string projectDir, CrudDetails details)
    {
        var replacements = new Dictionary<string, string>
        {
            { "EntityName", details.EntityName },
            { "Properties", GenerateTestProperties(details.Properties) }
        };

        // Generate Unit Tests
        await GenerateFromTemplateAsync(
            "CleanArchitecture/Tests/Domain/EntityTests.cs.template",
            Path.Combine(projectDir, $"{details.EntityName}.Tests/Domain/{details.EntityName}Tests.cs"),
            replacements);

        await GenerateFromTemplateAsync(
            "CleanArchitecture/Tests/Application/EntityServiceTests.cs.template",
            Path.Combine(projectDir, $"{details.EntityName}.Tests/Application/{details.EntityName}ServiceTests.cs"),
            replacements);

        await GenerateFromTemplateAsync(
            "CleanArchitecture/Tests/Infrastructure/EntityRepositoryTests.cs.template",
            Path.Combine(projectDir, $"{details.EntityName}.Tests/Infrastructure/{details.EntityName}RepositoryTests.cs"),
            replacements);
    }

    // Minimal API Pattern Generation
    public async Task GenerateMinimalApiEndpoints(string projectDir, CrudDetails details)
    {
        var replacements = new Dictionary<string, string>
        {
            { "EntityName", details.EntityName },
            { "Properties", GenerateEntityProperties(details.Properties) }
        };

        await GenerateFromTemplateAsync(
            "MinimalApi/Endpoints/EntityEndpoints.cs.template",
            Path.Combine(projectDir, "Endpoints", $"{details.EntityName}Endpoints.cs"),
            replacements);
    }

    public async Task GenerateMinimalApiTests(string projectDir, CrudDetails details)
    {
        var replacements = new Dictionary<string, string>
        {
            { "EntityName", details.EntityName },
            { "Properties", GenerateTestProperties(details.Properties) }
        };

        await GenerateFromTemplateAsync(
            "MinimalApi/Tests/EntityEndpointsTests.cs.template",
            Path.Combine(projectDir, "Tests", $"{details.EntityName}EndpointsTests.cs"),
            replacements);
    }

    // DDD Pattern Generation
    public async Task GenerateAggregateRoot(string projectDir, CrudDetails details)
    {
        var replacements = new Dictionary<string, string>
        {
            { "EntityName", details.EntityName },
            { "Properties", GenerateEntityProperties(details.Properties) }
        };

        await GenerateFromTemplateAsync(
            "DDD/Domain/AggregateRoots/Entity.cs.template",
            Path.Combine(projectDir, "Domain", "AggregateRoots", $"{details.EntityName}.cs"),
            replacements);
    }

    public async Task GenerateValueObjects(string projectDir, CrudDetails details)
    {
        foreach (var property in details.Properties.Where(p => p.Type == "ValueObject"))
        {
            var replacements = new Dictionary<string, string>
            {
                { "ValueObjectName", property.Name },
                { "Properties", GenerateValueObjectProperties(property) }
            };

            await GenerateFromTemplateAsync(
                "DDD/Domain/ValueObjects/ValueObject.cs.template",
                Path.Combine(projectDir, "Domain", "ValueObjects", $"{property.Name}.cs"),
                replacements);
        }
    }

    public async Task GenerateDddRepository(string projectDir, CrudDetails details)
    {
        var replacements = new Dictionary<string, string>
        {
            { "EntityName", details.EntityName },
            { "Properties", GenerateRepositoryMethods(details) }
        };

        await GenerateFromTemplateAsync(
            "DDD/Infrastructure/Repositories/EntityRepository.cs.template",
            Path.Combine(projectDir, "Infrastructure", "Repositories", $"{details.EntityName}Repository.cs"),
            replacements);
    }

    public async Task GenerateApplicationService(string projectDir, CrudDetails details)
    {
        var replacements = new Dictionary<string, string>
        {
            { "EntityName", details.EntityName },
            { "Properties", GenerateServiceMethods(details) }
        };

        await GenerateFromTemplateAsync(
            "DDD/Application/Services/EntityApplicationService.cs.template",
            Path.Combine(projectDir, "Application", "Services", $"{details.EntityName}ApplicationService.cs"),
            replacements);
    }

    public async Task GenerateDddTests(string projectDir, CrudDetails details)
    {
        var replacements = new Dictionary<string, string>
        {
            { "EntityName", details.EntityName },
            { "Properties", GenerateTestProperties(details.Properties) }
        };

        // Generate Domain Tests
        await GenerateFromTemplateAsync(
            "DDD/Tests/Domain/EntityTests.cs.template",
            Path.Combine(projectDir, "Tests", "Domain", $"{details.EntityName}Tests.cs"),
            replacements);

        // Generate Application Tests
        await GenerateFromTemplateAsync(
            "DDD/Tests/Application/EntityApplicationServiceTests.cs.template",
            Path.Combine(projectDir, "Tests", "Application", $"{details.EntityName}ApplicationServiceTests.cs"),
            replacements);
    }

    // Helper methods
    private string GenerateEntityProperties(List<EntityProperty> properties)
    {
        var sb = new StringBuilder();
        foreach (var prop in properties)
        {
            if (!string.IsNullOrEmpty(prop.Description))
            {
                sb.AppendLine($"        /// <summary>");
                sb.AppendLine($"        /// {prop.Description}");
                sb.AppendLine($"        /// </summary>");
            }

            foreach (var attribute in prop.Attributes)
            {
                sb.AppendLine($"        {attribute}");
            }

            var nullableSymbol = prop.IsRequired ? "" : "?";
            sb.AppendLine($"        public {prop.Type}{nullableSymbol} {prop.Name} {{ get; set; }}");
            sb.AppendLine();
        }
        return sb.ToString();
    }

    private string GenerateTestProperties(List<EntityProperty> properties)
    {
        var sb = new StringBuilder();
        foreach (var prop in properties)
        {
            var defaultValue = GetDefaultValue(prop.Type);
            sb.AppendLine($"                {prop.Name} = {defaultValue},");
        }
        return sb.ToString();
    }

    private string GetDefaultValue(string type)
    {
        return type switch
        {
            "string" => "\"Test Value\"",
            "int" => "1",
            "decimal" => "1.0m",
            "bool" => "true",
            "DateTime" => "DateTime.UtcNow",
            "Guid" => "Guid.NewGuid()",
            "long" => "1L",
            "float" => "1.0f",
            "double" => "1.0d",
            _ => "null"
        };
    }

    private string GenerateRepositoryMethods(CrudDetails details)
    {
        // Implementation will be added in next update
        return string.Empty;
    }

    private string GenerateServiceMethods(CrudDetails details)
    {
        // Implementation will be added in next update
        return string.Empty;
    }

    private string GenerateControllerMethods(CrudDetails details)
    {
        // Implementation will be added in next update
        return string.Empty;
    }

    private async Task GenerateCreateCommand(string projectDir, CrudDetails details)
    {
        var replacements = new Dictionary<string, string>
        {
            { "EntityName", details.EntityName },
            { "Properties", GenerateCommandProperties(details.Properties) },
            { "Validations", GenerateValidationRules(details.Properties) },
            { "Mappings", GenerateEntityMappings(details.Properties) }
        };

        var templatePath = Path.Combine("Cqrs", "Commands", "CreateEntityCommand.cs.template");
        var outputPath = Path.Combine(projectDir, "Application", "Commands", $"Create{details.EntityName}Command.cs");
        await GenerateFromTemplateAsync(templatePath, outputPath, replacements);
    }

    private async Task GenerateUpdateCommand(string projectDir, CrudDetails details)
    {
        var replacements = new Dictionary<string, string>
        {
            { "EntityName", details.EntityName },
            { "Properties", GenerateCommandProperties(details.Properties) },
            { "Validations", GenerateValidationRules(details.Properties) },
            { "Mappings", GenerateEntityMappings(details.Properties) }
        };

        var templatePath = Path.Combine("Cqrs", "Commands", "UpdateEntityCommand.cs.template");
        var outputPath = Path.Combine(projectDir, "Application", "Commands", $"Update{details.EntityName}Command.cs");
        await GenerateFromTemplateAsync(templatePath, outputPath, replacements);
    }

    private async Task GenerateDeleteCommand(string projectDir, CrudDetails details)
    {
        var replacements = new Dictionary<string, string>
        {
            { "EntityName", details.EntityName },
            { "SoftDelete", details.Options.ImplementSoftDelete ? 
                "            entity.IsDeleted = true;\n            _repository.Update(entity);" :
                "            await _repository.DeleteAsync(entity, cancellationToken);" }
        };

        var templatePath = Path.Combine("Cqrs", "Commands", "DeleteEntityCommand.cs.template");
        var outputPath = Path.Combine(projectDir, "Application", "Commands", $"Delete{details.EntityName}Command.cs");
        await GenerateFromTemplateAsync(templatePath, outputPath, replacements);
    }

    private async Task GenerateGetQuery(string projectDir, CrudDetails details)
    {
        var replacements = new Dictionary<string, string>
        {
            { "EntityName", details.EntityName },
            { "CacheService", details.Options.ImplementCaching ? 
                "        private readonly ICacheService _cacheService;" : "" },
            { "CacheServiceParam", details.Options.ImplementCaching ? 
                ",\n            ICacheService cacheService" : "" },
            { "CacheServiceAssignment", details.Options.ImplementCaching ? 
                "            _cacheService = cacheService;" : "" },
            { "CacheCheck", GenerateCacheCheck(details) },
            { "CacheSet", GenerateCacheSet(details) }
        };

        var templatePath = Path.Combine("Cqrs", "Queries", "GetEntityQuery.cs.template");
        var outputPath = Path.Combine(projectDir, "Application", "Queries", $"Get{details.EntityName}Query.cs");
        await GenerateFromTemplateAsync(templatePath, outputPath, replacements);
    }

    private async Task GenerateGetAllQuery(string projectDir, CrudDetails details)
    {
        var replacements = new Dictionary<string, string>
        {
            { "EntityName", details.EntityName },
            { "SearchConditions", GenerateSearchConditions(details.Properties) },
            { "CacheService", details.Options.ImplementCaching ? 
                "        private readonly ICacheService _cacheService;" : "" },
            { "CacheServiceParam", details.Options.ImplementCaching ? 
                ",\n            ICacheService cacheService" : "" },
            { "CacheServiceAssignment", details.Options.ImplementCaching ? 
                "            _cacheService = cacheService;" : "" },
            { "CacheCheck", GenerateCacheCheck(details) },
            { "CacheSet", GenerateCacheSet(details) }
        };

        var templatePath = Path.Combine("Cqrs", "Queries", "GetAllEntitiesQuery.cs.template");
        var outputPath = Path.Combine(projectDir, "Application", "Queries", $"GetAll{details.EntityName}sQuery.cs");
        await GenerateFromTemplateAsync(templatePath, outputPath, replacements);
    }

    private async Task GenerateServiceInterface(string projectDir, CrudDetails details)
    {
        var replacements = new Dictionary<string, string>
        {
            { "EntityName", details.EntityName },
            { "AdditionalMethods", "" }
        };

        var templatePath = Path.Combine("ServiceRepository", "Interfaces", "IEntityService.cs.template");
        var outputPath = Path.Combine(projectDir, "Interfaces", $"I{details.EntityName}Service.cs");
        await GenerateFromTemplateAsync(templatePath, outputPath, replacements);
    }

    private async Task GenerateRepositoryInterface(string projectDir, CrudDetails details)
    {
        var replacements = new Dictionary<string, string>
        {
            { "EntityName", details.EntityName },
            { "AdditionalMethods", "" }
        };

        var templatePath = Path.Combine("ServiceRepository", "Interfaces", "IEntityRepository.cs.template");
        var outputPath = Path.Combine(projectDir, "Interfaces", $"I{details.EntityName}Repository.cs");
        await GenerateFromTemplateAsync(templatePath, outputPath, replacements);
    }

    private string GenerateCommandProperties(List<EntityProperty> properties)
    {
        var sb = new StringBuilder();
        foreach (var prop in properties.Where(p => !p.IsKey))
        {
            if (!string.IsNullOrEmpty(prop.Description))
            {
                sb.AppendLine($"        /// <summary>");
                sb.AppendLine($"        /// {prop.Description}");
                sb.AppendLine($"        /// </summary>");
            }

            var nullableSymbol = prop.IsRequired ? "" : "?";
            sb.AppendLine($"        public {prop.Type}{nullableSymbol} {prop.Name} {{ get; set; }}");
        }
        return sb.ToString();
    }

    private string GenerateValidationRules(List<EntityProperty> properties)
    {
        var sb = new StringBuilder();
        foreach (var prop in properties.Where(p => !p.IsKey))
        {
            if (prop.IsRequired)
            {
                sb.AppendLine($"            RuleFor(x => x.{prop.Name}).NotEmpty();");
            }

            switch (prop.Type.ToLower())
            {
                case "string":
                    sb.AppendLine($"            RuleFor(x => x.{prop.Name})");
                    sb.AppendLine($"                .MaximumLength(200)");
                    if (prop.IsRequired)
                        sb.AppendLine($"                .NotEmpty()");
                    sb.AppendLine($"                .When(x => x.{prop.Name} != null);");
                    break;
                case "int":
                case "long":
                    sb.AppendLine($"            RuleFor(x => x.{prop.Name})");
                    sb.AppendLine($"                .GreaterThanOrEqualTo(0)");
                    if (prop.IsRequired)
                        sb.AppendLine($"                .NotEmpty()");
                    sb.AppendLine($"                .When(x => x.{prop.Name} != null);");
                    break;
                case "decimal":
                case "double":
                case "float":
                    sb.AppendLine($"            RuleFor(x => x.{prop.Name})");
                    sb.AppendLine($"                .GreaterThanOrEqualTo(0)");
                    if (prop.IsRequired)
                        sb.AppendLine($"                .NotEmpty()");
                    sb.AppendLine($"                .When(x => x.{prop.Name} != null);");
                    break;
                case "datetime":
                    if (prop.IsRequired)
                    {
                        sb.AppendLine($"            RuleFor(x => x.{prop.Name})");
                        sb.AppendLine($"                .NotEmpty();");
                    }
                    break;
                case "guid":
                    if (prop.IsRequired)
                    {
                        sb.AppendLine($"            RuleFor(x => x.{prop.Name})");
                        sb.AppendLine($"                .NotEmpty();");
                    }
                    break;
            }
        }
        return sb.ToString();
    }

    private string GenerateEntityMappings(List<EntityProperty> properties)
    {
        var sb = new StringBuilder();
        foreach (var prop in properties.Where(p => !p.IsKey))
        {
            sb.AppendLine($"                {prop.Name} = request.{prop.Name},");
        }
        return sb.ToString();
    }

    private string GenerateSearchConditions(List<EntityProperty> properties)
    {
        var conditions = new List<string>();
        foreach (var prop in properties.Where(p => p.Type == "string"))
        {
            conditions.Add($"x.{prop.Name}.Contains(request.SearchTerm)");
        }

        return conditions.Count > 0 
            ? string.Join(" || ", conditions)
            : "true";
    }

    private string GenerateCacheCheck(CrudDetails details)
    {
        if (!details.Options.ImplementCaching)
            return string.Empty;

        return @"            // Check cache first
            var cacheKey = $""{details.EntityName}:{request.Id}"";
            var cachedResult = await _cacheService.GetAsync<{details.EntityName}Dto>(cacheKey);
            if (cachedResult != null)
                return cachedResult;
";
    }

    private string GenerateCacheSet(CrudDetails details)
    {
        if (!details.Options.ImplementCaching)
            return string.Empty;

        return @"            // Cache the result
            await _cacheService.SetAsync($""{details.EntityName}:{request.Id}"", dto, TimeSpan.FromMinutes(10));
";
    }

    private string GenerateValueObjectProperties(EntityProperty property)
    {
        var sb = new StringBuilder();
        if (property.ValueObjectProperties != null)
        {
            foreach (var voProp in property.ValueObjectProperties)
            {
                sb.AppendLine($"        public {voProp.Value} {voProp.Key} {{ get; private set; }}");
            }
        }
        return sb.ToString();
    }

    public async Task<bool> GenerateCrudOperationsAsync(string entityName, string pattern, string projectPath, string entityLocation)
    {
        try
        {
            // Determine which templates to use based on the pattern
            var templatePath = Path.Combine(_templatesPath, pattern.ToLower().Replace(" ", "_"));
            if (!Directory.Exists(templatePath))
            {
                AnsiConsole.MarkupLine($"[red]Error: Template path not found: {templatePath}[/]");
                return false;
            }

            // Generate files based on pattern
            switch (pattern.ToLower())
            {
                case "cqrs":
                    await GenerateCqrsFiles(entityName, projectPath, entityLocation, templatePath);
                    break;
                case "service repository":
                    await GenerateServiceRepositoryFiles(entityName, projectPath, entityLocation, templatePath);
                    break;
                case "clean architecture":
                    await GenerateCleanArchitectureFiles(entityName, projectPath, entityLocation, templatePath);
                    break;
                case "minimal api":
                    await GenerateMinimalApiFiles(entityName, projectPath, entityLocation, templatePath);
                    break;
                case "domain-driven design":
                    await GenerateDddFiles(entityName, projectPath, entityLocation, templatePath);
                    break;
                default:
                    AnsiConsole.MarkupLine($"[red]Error: Unsupported pattern: {pattern}[/]");
                    return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error generating CRUD operations: {ex.Message}[/]");
            return false;
        }
    }

    private async Task GenerateCqrsFiles(string entityName, string projectPath, string entityLocation, string templatePath)
    {
        // Generate Commands
        await GenerateFromTemplate(
            Path.Combine(templatePath, "Commands", "CreateEntityCommand.cs.template"),
            Path.Combine(projectPath, "Commands", $"Create{entityName}Command.cs"),
            new { EntityName = entityName });

        await GenerateFromTemplate(
            Path.Combine(templatePath, "Commands", "UpdateEntityCommand.cs.template"),
            Path.Combine(projectPath, "Commands", $"Update{entityName}Command.cs"),
            new { EntityName = entityName });

        await GenerateFromTemplate(
            Path.Combine(templatePath, "Commands", "DeleteEntityCommand.cs.template"),
            Path.Combine(projectPath, "Commands", $"Delete{entityName}Command.cs"),
            new { EntityName = entityName });

        // Generate Queries
        await GenerateFromTemplate(
            Path.Combine(templatePath, "Queries", "GetEntityQuery.cs.template"),
            Path.Combine(projectPath, "Queries", $"Get{entityName}Query.cs"),
            new { EntityName = entityName });

        await GenerateFromTemplate(
            Path.Combine(templatePath, "Queries", "GetAllEntitiesQuery.cs.template"),
            Path.Combine(projectPath, "Queries", $"GetAll{entityName}sQuery.cs"),
            new { EntityName = entityName });
    }

    private async Task GenerateServiceRepositoryFiles(string entityName, string projectPath, string entityLocation, string templatePath)
    {
        // Generate Interface
        await GenerateFromTemplate(
            Path.Combine(templatePath, "Interfaces", "IEntityRepository.cs.template"),
            Path.Combine(projectPath, "Interfaces", $"I{entityName}Repository.cs"),
            new { EntityName = entityName });

        await GenerateFromTemplate(
            Path.Combine(templatePath, "Interfaces", "IEntityService.cs.template"),
            Path.Combine(projectPath, "Interfaces", $"I{entityName}Service.cs"),
            new { EntityName = entityName });

        // Generate Implementation
        await GenerateFromTemplate(
            Path.Combine(templatePath, "Repositories", "EntityRepository.cs.template"),
            Path.Combine(projectPath, "Repositories", $"{entityName}Repository.cs"),
            new { EntityName = entityName });

        await GenerateFromTemplate(
            Path.Combine(templatePath, "Services", "EntityService.cs.template"),
            Path.Combine(projectPath, "Services", $"{entityName}Service.cs"),
            new { EntityName = entityName });

        // Generate Controller
        await GenerateFromTemplate(
            Path.Combine(templatePath, "Controllers", "EntityController.cs.template"),
            Path.Combine(projectPath, "Controllers", $"{entityName}Controller.cs"),
            new { EntityName = entityName });
    }

    private async Task GenerateCleanArchitectureFiles(string entityName, string projectPath, string entityLocation, string templatePath)
    {
        // Generate Domain Layer
        await GenerateFromTemplate(
            Path.Combine(templatePath, "Domain", "Entity.cs.template"),
            Path.Combine(projectPath, "Domain", "Entities", $"{entityName}.cs"),
            new { EntityName = entityName });

        // Generate Application Layer
        await GenerateFromTemplate(
            Path.Combine(templatePath, "Application", "EntityDto.cs.template"),
            Path.Combine(projectPath, "Application", "DTOs", $"{entityName}Dto.cs"),
            new { EntityName = entityName });

        await GenerateFromTemplate(
            Path.Combine(templatePath, "Application", "IEntityRepository.cs.template"),
            Path.Combine(projectPath, "Application", "Interfaces", $"I{entityName}Repository.cs"),
            new { EntityName = entityName });

        // Generate Infrastructure Layer
        await GenerateFromTemplate(
            Path.Combine(templatePath, "Infrastructure", "EntityRepository.cs.template"),
            Path.Combine(projectPath, "Infrastructure", "Repositories", $"{entityName}Repository.cs"),
            new { EntityName = entityName });

        // Generate API Layer
        await GenerateFromTemplate(
            Path.Combine(templatePath, "Api", "EntityController.cs.template"),
            Path.Combine(projectPath, "Api", "Controllers", $"{entityName}Controller.cs"),
            new { EntityName = entityName });
    }

    private async Task GenerateMinimalApiFiles(string entityName, string projectPath, string entityLocation, string templatePath)
    {
        await GenerateFromTemplate(
            Path.Combine(templatePath, "Endpoints", "EntityEndpoints.cs.template"),
            Path.Combine(projectPath, "Endpoints", $"{entityName}Endpoints.cs"),
            new { EntityName = entityName });

        await GenerateFromTemplate(
            Path.Combine(templatePath, "Models", "EntityDto.cs.template"),
            Path.Combine(projectPath, "Models", $"{entityName}Dto.cs"),
            new { EntityName = entityName });
    }

    private async Task GenerateDddFiles(string entityName, string projectPath, string entityLocation, string templatePath)
    {
        // Generate Domain Layer
        await GenerateFromTemplate(
            Path.Combine(templatePath, "Domain", "Entity.cs.template"),
            Path.Combine(projectPath, "Domain", $"{entityName}Aggregate", $"{entityName}.cs"),
            new { EntityName = entityName });

        await GenerateFromTemplate(
            Path.Combine(templatePath, "Domain", "IEntityRepository.cs.template"),
            Path.Combine(projectPath, "Domain", $"{entityName}Aggregate", $"I{entityName}Repository.cs"),
            new { EntityName = entityName });

        // Generate Infrastructure Layer
        await GenerateFromTemplate(
            Path.Combine(templatePath, "Infrastructure", "EntityRepository.cs.template"),
            Path.Combine(projectPath, "Infrastructure", "Repositories", $"{entityName}Repository.cs"),
            new { EntityName = entityName });

        // Generate Application Layer
        await GenerateFromTemplate(
            Path.Combine(templatePath, "Application", "EntityService.cs.template"),
            Path.Combine(projectPath, "Application", "Services", $"{entityName}Service.cs"),
            new { EntityName = entityName });

        // Generate API Layer
        await GenerateFromTemplate(
            Path.Combine(templatePath, "Api", "EntityController.cs.template"),
            Path.Combine(projectPath, "Api", "Controllers", $"{entityName}Controller.cs"),
            new { EntityName = entityName });
    }

    private async Task GenerateFromTemplate(string templatePath, string outputPath, object model)
    {
        try
        {
            if (!File.Exists(templatePath))
            {
                AnsiConsole.MarkupLine($"[red]Template not found: {templatePath}[/]");
                return;
            }

            var template = await File.ReadAllTextAsync(templatePath);
            var output = RenderTemplate(template, model);

            var outputDir = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir!);
            }

            await File.WriteAllTextAsync(outputPath, output);
            AnsiConsole.MarkupLine($"[green]Generated: {outputPath}[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error generating file from template: {ex.Message}[/]");
        }
    }

    private string RenderTemplate(string template, object model)
    {
        // Simple template rendering - replace placeholders with values
        var output = template;
        foreach (var prop in model.GetType().GetProperties())
        {
            var value = prop.GetValue(model)?.ToString() ?? string.Empty;
            output = output.Replace($"{{{prop.Name}}}", value);
        }
        return output;
    }

    public Task<bool> GenerateProjectAsync(string projectName, string pattern, string testFramework, string ciPipeline, string outputDirectory)
    {
        throw new NotImplementedException();
    }

    public Task<bool> GenerateEndpointAsync(string name, string method, string entity, string controllerPath, string entityPath, string pattern, string projectPath, string route, string responseType)
    {
        throw new NotImplementedException();
    }
} 
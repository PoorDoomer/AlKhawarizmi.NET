using DotNetProjectGenerator.Core.Models;
using DotNetProjectGenerator.Core.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using DotNetProjectGenerator.Cli.Infrastructure;
using Microsoft.Extensions.DependencyInjection;


namespace DotNetProjectGenerator.Cli
{
    public class Program
    {
        private static readonly IProjectGenerator _projectGenerator = new ProjectGenerator();

        // A few Kawaii ASCII faces to randomly display
        private static readonly string[] _kawaiiFaces =
        {
            "(≧◡≦)", "(ﾉ◕ヮ◕)ﾉ*:･ﾟ✧", "(✿◠‿◠)", "(づ｡◕‿‿◕)づ", "(╯°□°）╯︵ ┻━┻", "(｡♥‿♥｡)",
            "(UwU)", "(｡◕‿◕｡)", "(* ^ ω ^)", "(☆▽☆)"
        };

        public static int Main(string[] args)
        {
            var services = new ServiceCollection();

            // Register services
            services.AddSingleton<ITemplateGenerator, TemplateGenerator>();
            services.AddSingleton<IProjectAnalyzer, ProjectAnalyzer>();

            var registrar = new TypeRegistrar(services);
            var app = new CommandApp(registrar);

            // If no arguments were provided, go fully interactive by default
            if (args == null || args.Length == 0)
            {
                // Start the interactive experience
                ShowAnimatedIntro();
                return ShowInteractiveMenu(app).GetAwaiter().GetResult();
            }
            else
            {
                // If user supplied commands/options, run them directly
                return app.Run(args);
            }
        }

        /// <summary>
        /// Displays an animated intro: a fake "loading" spinner + a big ASCII banner.
        /// </summary>
        private static void ShowAnimatedIntro()
        {
            // Show a quick fake loading spinner
            AnsiConsole.Status()
                .Spinner(Spinner.Known.Aesthetic)
                .SpinnerStyle(Style.Parse("magenta dim"))
                .Start($"[pink1]Booting AlKhawarizmi.NET {GetRandomKawaiiFace()}...[/]", ctx =>
                {
                    Thread.Sleep(1500);
                });

            // Now display a big ASCII banner via Figlet
            AnsiConsole.Write(
                new FigletText("AlKhawarizmi.NET")
                    .Centered()
                    .Color(Color.Pink1));

            // A little extra flourish
            AnsiConsole.MarkupLine($"[yellow italic]The Kawaii .NET Project Generator ~ {GetRandomKawaiiFace()}[/]");
            AnsiConsole.WriteLine();
        }

        /// <summary>
        /// Shows an interactive menu with Kawaii styling.
        /// </summary>
        private static async Task<int> ShowInteractiveMenu(CommandApp app)
        {
            // Add a fun rule below the banner
            var welcomeRule = new Rule($"[pink1]Welcome, Senpai![/] {GetRandomKawaiiFace()}");
            welcomeRule.Style = Style.Parse("pink1");
            AnsiConsole.Write(welcomeRule);

            // Menu loop
            while (true)
            {
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"[green]What would you like to do, dear programmer {GetRandomKawaiiFace()}?[/]")
                        .HighlightStyle(new Style(foreground: Color.HotPink))
                        .PageSize(10)
                        .AddChoices(new[]
                        {
                            "Create New Project",
                            "Generate Project from PDF",
                            "Exit"
                        }));

                switch (choice)
                {
                    case "Create New Project":
                        await PromptCreateNewProject();
                        break;

                    case "Generate Project from PDF":
                        await PromptGenerateProjectFromPdf();
                        break;

                    case "Exit":
                        // Sweet goodbye
                        AnsiConsole.MarkupLine($"[magenta]\nArigato for using AlKhawarizmi.NET! Sayonara~ {GetRandomKawaiiFace()}[/]");
                        return 0;
                }

                // Ask if user wants to do something else
                AnsiConsole.WriteLine();
                if (!AnsiConsole.Confirm($"[pink1]Would you like to do another operation {GetRandomKawaiiFace()}?[/]"))
                {
                    AnsiConsole.MarkupLine($"[magenta]\nArigato! See you next time! {GetRandomKawaiiFace()}[/]");
                    return 0;
                }

                // Clear screen and show the intro again for flair
                AnsiConsole.Clear();
                ShowAnimatedIntro();
            }
        }

        private static async Task PromptCreateNewProject()
        {
            var projectName = AnsiConsole.Ask<string>($"Enter the [green]project name[/] {GetRandomKawaiiFace()}:");

            // Add optional output directory prompt
            var outputDir = AnsiConsole.Prompt(
                new TextPrompt<string>($"Enter the [green]output directory[/] (optional, press Enter for current directory) {GetRandomKawaiiFace()}:")
                    .AllowEmpty());

            var pattern = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"Select the [green]architecture pattern[/] {GetRandomKawaiiFace()}:")
                    .AddChoices("clean", "ddd", "cqrs"));

            var testFramework = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"Select the [green]test framework[/] {GetRandomKawaiiFace()}:")
                    .AddChoices("xunit", "nunit", "mstest"));

            var ciPipeline = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"Select the [green]CI pipeline[/] {GetRandomKawaiiFace()}:")
                    .AddChoices("github", "azure", "gitlab"));

            await HandleInit(new InitOptions
            {
                ProjectName = projectName,
                OutputDirectory = outputDir,
                Pattern = pattern,
                TestFramework = testFramework,
                CiPipeline = ciPipeline
            });
        }

        private static async Task PromptGenerateProjectFromPdf()
        {
            var pdfPath = AnsiConsole.Ask<string>($"Enter the [green]PDF path[/] {GetRandomKawaiiFace()}:");
            var generatedProjectName = AnsiConsole.Ask<string>($"Enter the [green]project name[/] {GetRandomKawaiiFace()}:");

            await HandlePdf(new PdfOptions
            {
                PdfPath = pdfPath,
                ProjectName = generatedProjectName
            });
        }

        private static async Task<int> HandleInit(InitOptions opts)
        {
            AnsiConsole.MarkupLine($"[green]Creating new project: {opts.ProjectName}[/] {GetRandomKawaiiFace()}");

            var rule = new Rule($"[yellow]{opts.ProjectName}[/]");
            rule.Style = Style.Parse("yellow");
            AnsiConsole.Write(rule);

            // Directly call the initializer
            var success = await _projectGenerator.InitializeProjectAsync(
                opts.ProjectName,
                opts.Pattern,
                opts.TestFramework,
                opts.CiPipeline,
                opts.OutputDirectory);

            if (success)
            {
                AnsiConsole.MarkupLine($"[green]Project created successfully![/] {GetRandomKawaiiFace()}");
                return 0;
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]Failed to create project.[/] {GetRandomKawaiiFace()}");
                return 1;
            }
        }

        private static async Task<int> HandlePdf(PdfOptions opts)
        {
            AnsiConsole.MarkupLine($"[green]Generating project from PDF: {opts.PdfPath}[/] {GetRandomKawaiiFace()}");

            var success = await AnsiConsole.Status()
                .Spinner(Spinner.Known.BouncingBar)
                .SpinnerStyle(Style.Parse("yellow dim"))
                .StartAsync($"[pink1]COMING GOOOON {GetRandomKawaiiFace()}[/]", async ctx =>
                {
                    return await _projectGenerator.GenerateFromPdfAsync(opts.PdfPath, opts.ProjectName);
                });

            if (success)
            {
                AnsiConsole.MarkupLine($"[green]Project generated successfully![/] {GetRandomKawaiiFace()}");
                return 0;
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]Failed to generate project from PDF.[/] {GetRandomKawaiiFace()}");
                return 1;
            }
        }

        /// <summary>
        /// Just picks a random kawaii face from our list.
        /// </summary>
        private static string GetRandomKawaiiFace()
        {
            var index = new Random().Next(_kawaiiFaces.Length);
            return _kawaiiFaces[index];
        }
    }
}

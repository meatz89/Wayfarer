using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Xunit;

namespace Wayfarer.Tests.Architecture;

/// <summary>
/// Tests for Code Quality, Fail-Fast, Separation, and Namespace compliance.
/// Covers categories enforced by pre-commit hook but not covered by other test files.
///
/// BEHAVIOR-ONLY TESTING (§8.21): Tests verify source code patterns.
/// </summary>
public class CodeQualityComplianceTests
{
    private readonly string _srcPath;

    public CodeQualityComplianceTests()
    {
        _srcPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "src");

        if (!Directory.Exists(_srcPath))
        {
            _srcPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "src"));
        }
    }

    // ========== FAIL-FAST PHILOSOPHY ==========

    /// <summary>
    /// Verify domain code avoids null coalescing operators that hide missing data.
    /// Fail-Fast principle: Missing data should fail loudly, not silently default.
    /// </summary>
    [Fact]
    public void SourceCode_AvoidNullCoalescing_InDomainLogic()
    {
        List<string> violations = new List<string>();
        Regex nullCoalescingPattern = new Regex(@"\?\?", RegexOptions.Compiled);

        List<string> domainDirs = new List<string> { "GameState", "Services", "Subsystems" };

        if (Directory.Exists(_srcPath))
        {
            foreach (string domainDir in domainDirs)
            {
                string fullPath = Path.Combine(_srcPath, domainDir);
                if (!Directory.Exists(fullPath)) continue;

                foreach (string file in Directory.GetFiles(fullPath, "*.cs", SearchOption.AllDirectories))
                {
                    if (file.Contains("Test") || file.Contains("obj") || file.Contains("bin")) continue;

                    string[] lines = File.ReadAllLines(file);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string line = lines[i];

                        if (nullCoalescingPattern.IsMatch(line) &&
                            !line.TrimStart().StartsWith("//") &&
                            !line.Contains("parameter") &&
                            !line.Contains("argument"))
                        {
                            string fileName = Path.GetFileName(file);
                            violations.Add($"{fileName}:{i + 1}: {line.Trim()}");
                        }
                    }
                }
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// Verify domain code avoids TryGetValue/TryParse patterns that silently fail.
    /// </summary>
    [Fact]
    public void SourceCode_AvoidTryPatterns_InDomainLogic()
    {
        List<string> violations = new List<string>();
        Regex tryPattern = new Regex(@"TryGetValue|TryParse", RegexOptions.Compiled);

        List<string> domainDirs = new List<string> { "GameState", "Services", "Subsystems" };

        if (Directory.Exists(_srcPath))
        {
            foreach (string domainDir in domainDirs)
            {
                string fullPath = Path.Combine(_srcPath, domainDir);
                if (!Directory.Exists(fullPath)) continue;

                foreach (string file in Directory.GetFiles(fullPath, "*.cs", SearchOption.AllDirectories))
                {
                    if (file.Contains("Test") || file.Contains("obj") || file.Contains("bin")) continue;
                    if (file.Contains("Parser") || file.Contains("DTO")) continue;

                    string[] lines = File.ReadAllLines(file);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string line = lines[i];

                        if (tryPattern.IsMatch(line) && !line.TrimStart().StartsWith("//"))
                        {
                            string fileName = Path.GetFileName(file);
                            violations.Add($"{fileName}:{i + 1}: {line.Trim()}");
                        }
                    }
                }
            }
        }

        Assert.Empty(violations);
    }

    // ========== BACKEND/FRONTEND SEPARATION ==========

    /// <summary>
    /// Verify backend services don't have CssClass properties.
    /// Frontend decides presentation (CSS), backend provides domain semantics.
    /// </summary>
    [Fact]
    public void SourceCode_NoCssClass_InBackendServices()
    {
        List<string> violations = new List<string>();
        Regex cssPattern = new Regex(@"CssClass|ClassName.*string", RegexOptions.Compiled);

        List<string> backendDirs = new List<string> { "Services", "Subsystems" };

        if (Directory.Exists(_srcPath))
        {
            foreach (string backendDir in backendDirs)
            {
                string fullPath = Path.Combine(_srcPath, backendDir);
                if (!Directory.Exists(fullPath)) continue;

                foreach (string file in Directory.GetFiles(fullPath, "*.cs", SearchOption.AllDirectories))
                {
                    if (file.Contains("Test") || file.Contains("obj") || file.Contains("bin")) continue;
                    if (file.Contains(".razor.cs")) continue;

                    string[] lines = File.ReadAllLines(file);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string line = lines[i];

                        if (cssPattern.IsMatch(line) && !line.TrimStart().StartsWith("//"))
                        {
                            string fileName = Path.GetFileName(file);
                            violations.Add($"{fileName}:{i + 1}: {line.Trim()}");
                        }
                    }
                }
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// Verify backend services don't have IconName properties.
    /// Frontend decides icons, backend provides domain semantics.
    /// </summary>
    [Fact]
    public void SourceCode_NoIconName_InBackendServices()
    {
        List<string> violations = new List<string>();
        Regex iconPattern = new Regex(@"IconName|public.*Icon.*\{", RegexOptions.Compiled);

        List<string> backendDirs = new List<string> { "Services", "Subsystems" };

        if (Directory.Exists(_srcPath))
        {
            foreach (string backendDir in backendDirs)
            {
                string fullPath = Path.Combine(_srcPath, backendDir);
                if (!Directory.Exists(fullPath)) continue;

                foreach (string file in Directory.GetFiles(fullPath, "*.cs", SearchOption.AllDirectories))
                {
                    if (file.Contains("Test") || file.Contains("obj") || file.Contains("bin")) continue;
                    if (file.Contains(".razor.cs")) continue;
                    if (file.Contains("Icon.cs")) continue;

                    string[] lines = File.ReadAllLines(file);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string line = lines[i];

                        if (iconPattern.IsMatch(line) && !line.TrimStart().StartsWith("//"))
                        {
                            string fileName = Path.GetFileName(file);
                            violations.Add($"{fileName}:{i + 1}: {line.Trim()}");
                        }
                    }
                }
            }
        }

        Assert.Empty(violations);
    }

    // ========== CODE QUALITY ==========

    /// <summary>
    /// Verify no TODO/FIXME comments in production code.
    /// Rule #0: No half measures - finish before committing.
    /// </summary>
    [Fact]
    public void SourceCode_NoTodoComments()
    {
        List<string> violations = new List<string>();
        Regex todoPattern = new Regex(@"\b(TODO|FIXME|HACK|XXX)\b", RegexOptions.Compiled);
        Regex exemptPattern = new Regex(@"completed|done|removed", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        if (Directory.Exists(_srcPath))
        {
            foreach (string file in Directory.GetFiles(_srcPath, "*.cs", SearchOption.AllDirectories))
            {
                if (file.Contains("Test") || file.Contains("obj") || file.Contains("bin")) continue;

                string[] lines = File.ReadAllLines(file);
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];

                    if (todoPattern.IsMatch(line) && !exemptPattern.IsMatch(line))
                    {
                        string fileName = Path.GetFileName(file);
                        violations.Add($"{fileName}:{i + 1}: {line.Trim()}");
                    }
                }
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// Verify no blocking async calls (.Wait(), .Result).
    /// Async everywhere - never block the thread.
    /// </summary>
    [Fact]
    public void SourceCode_NoBlockingAsync()
    {
        List<string> violations = new List<string>();
        Regex blockingPattern = new Regex(@"\.Wait\(\)|\.Result[;,\)]|GetAwaiter\(\)\.GetResult\(\)", RegexOptions.Compiled);

        if (Directory.Exists(_srcPath))
        {
            foreach (string file in Directory.GetFiles(_srcPath, "*.cs", SearchOption.AllDirectories))
            {
                if (file.Contains("Test") || file.Contains("obj") || file.Contains("bin")) continue;

                string[] lines = File.ReadAllLines(file);
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];

                    if (blockingPattern.IsMatch(line) && !line.TrimStart().StartsWith("//"))
                    {
                        string fileName = Path.GetFileName(file);
                        violations.Add($"{fileName}:{i + 1}: {line.Trim()}");
                    }
                }
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// Verify no extension methods in domain code.
    /// Domain Services and Entities only.
    /// </summary>
    [Fact]
    public void SourceCode_NoExtensionMethods_InDomain()
    {
        List<string> violations = new List<string>();
        Regex extensionPattern = new Regex(@"public\s+static\s+.*\bthis\s+", RegexOptions.Compiled);

        List<string> domainDirs = new List<string> { "GameState", "Services", "Subsystems", "Content" };

        if (Directory.Exists(_srcPath))
        {
            foreach (string domainDir in domainDirs)
            {
                string fullPath = Path.Combine(_srcPath, domainDir);
                if (!Directory.Exists(fullPath)) continue;

                foreach (string file in Directory.GetFiles(fullPath, "*.cs", SearchOption.AllDirectories))
                {
                    if (file.Contains("Test") || file.Contains("obj") || file.Contains("bin")) continue;

                    string[] lines = File.ReadAllLines(file);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string line = lines[i];

                        if (extensionPattern.IsMatch(line))
                        {
                            string fileName = Path.GetFileName(file);
                            violations.Add($"{fileName}:{i + 1}: {line.Trim()}");
                        }
                    }
                }
            }
        }

        Assert.Empty(violations);
    }

    // ========== NAMESPACE RULES ==========

    /// <summary>
    /// Verify domain code uses global namespace.
    /// Global Namespace Principle: No namespace declarations for domain code.
    /// </summary>
    [Fact]
    public void SourceCode_GlobalNamespace_InDomainCode()
    {
        List<string> violations = new List<string>();
        Regex namespacePattern = new Regex(@"^namespace\s+", RegexOptions.Compiled);
        Regex exemptPattern = new Regex(@"Wayfarer\.Tests|Wayfarer\.Pages", RegexOptions.Compiled);

        List<string> domainDirs = new List<string> { "GameState", "Services", "Subsystems", "Content" };

        if (Directory.Exists(_srcPath))
        {
            foreach (string domainDir in domainDirs)
            {
                string fullPath = Path.Combine(_srcPath, domainDir);
                if (!Directory.Exists(fullPath)) continue;

                foreach (string file in Directory.GetFiles(fullPath, "*.cs", SearchOption.AllDirectories))
                {
                    if (file.Contains("Test") || file.Contains("obj") || file.Contains("bin")) continue;
                    if (file.Contains(".razor.cs")) continue;

                    string[] lines = File.ReadAllLines(file);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string line = lines[i];

                        if (namespacePattern.IsMatch(line) && !exemptPattern.IsMatch(line))
                        {
                            string fileName = Path.GetFileName(file);
                            violations.Add($"{fileName}:{i + 1}: {line.Trim()}");
                        }
                    }
                }
            }
        }

        Assert.Empty(violations);
    }

    // ========== DETERMINISM ==========

    /// <summary>
    /// Verify Random is only used in Pile.cs (tactical layer).
    /// Strategic systems must be deterministic for player planning.
    /// </summary>
    [Fact]
    public void SourceCode_RandomOnlyInPile()
    {
        List<string> violations = new List<string>();
        Regex randomPattern = new Regex(@"new\s+Random|Random\(\)|\.Next\(", RegexOptions.Compiled);

        if (Directory.Exists(_srcPath))
        {
            foreach (string file in Directory.GetFiles(_srcPath, "*.cs", SearchOption.AllDirectories))
            {
                if (file.Contains("Test") || file.Contains("obj") || file.Contains("bin")) continue;
                if (file.Contains("Pile.cs")) continue;

                string[] lines = File.ReadAllLines(file);
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];

                    if (randomPattern.IsMatch(line) && !line.TrimStart().StartsWith("//"))
                    {
                        string fileName = Path.GetFileName(file);
                        violations.Add($"{fileName}:{i + 1}: {line.Trim()}");
                    }
                }
            }
        }

        Assert.Empty(violations);
    }

    // ========== ARC42 DOCUMENTATION ==========

    /// <summary>
    /// Verify arc42 documents don't contain code blocks.
    /// Arc42 describes WHAT, not HOW. Implementation details belong in code.
    /// </summary>
    [Fact]
    public void Arc42Docs_NoCodeBlocks()
    {
        List<string> violations = new List<string>();
        Regex codeBlockPattern = new Regex(@"```(csharp|c#|json)", RegexOptions.Compiled);

        string arc42Path = Path.Combine(_srcPath, "..", "arc42");

        if (Directory.Exists(arc42Path))
        {
            foreach (string file in Directory.GetFiles(arc42Path, "*.md", SearchOption.AllDirectories))
            {
                string[] lines = File.ReadAllLines(file);
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];

                    if (codeBlockPattern.IsMatch(line))
                    {
                        string fileName = Path.GetFileName(file);
                        violations.Add($"{fileName}:{i + 1}: {line.Trim()}");
                    }
                }
            }
        }

        Assert.Empty(violations);
    }

    // ========== TEST ARCHITECTURE ==========

    /// <summary>
    /// Verify tests do NOT call GameWorldInitializer directly.
    ///
    /// ARCHITECTURAL RULE:
    /// - Integration tests: Extend IntegrationTestBase (uses production path)
    /// - Unit tests: Create `new GameWorld()` manually
    /// - FORBIDDEN: Direct calls to GameWorldInitializer in test files
    ///
    /// Why: Tests must either test production code path (IntegrationTestBase)
    /// or be isolated unit tests (manual GameWorld). Calling GameWorldInitializer
    /// directly creates a third path that may diverge from production.
    /// </summary>
    [Fact]
    public void Tests_NoDirectGameWorldInitializerCalls()
    {
        List<string> violations = new List<string>();

        string testPath = Path.Combine(GetSourcePath(), "..", "Wayfarer.Tests.Project");
        testPath = Path.GetFullPath(testPath);

        if (!Directory.Exists(testPath)) return;

        foreach (string file in Directory.GetFiles(testPath, "*.cs", SearchOption.AllDirectories))
        {
            if (file.Contains("obj") || file.Contains("bin")) continue;

            string fileName = Path.GetFileName(file);

            // IntegrationTestBase is allowed to call it (that's its job)
            if (fileName == "IntegrationTestBase.cs") continue;

            // This test file is allowed to reference the pattern (for checking)
            if (fileName == "CodeQualityComplianceTests.cs") continue;

            string[] lines = File.ReadAllLines(file);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (line.Contains("GameWorldInitializer.") && !line.TrimStart().StartsWith("//"))
                {
                    violations.Add($"{fileName}:{i + 1}: {line.Trim()}");
                }
            }
        }

        Assert.True(violations.Count == 0,
            $"Tests must NOT call GameWorldInitializer directly.\n" +
            $"- Integration tests: Extend IntegrationTestBase\n" +
            $"- Unit tests: Use `new GameWorld()` manually\n\n" +
            $"Violations:\n{string.Join("\n", violations)}");
    }

    // ========== DOMAIN OBJECT PURITY ==========

    /// <summary>
    /// Verify domain entities do not have settable properties of service types.
    /// Domain objects should not hold references to DI-resolved services.
    /// Services should inject both GameWorld AND any services they need - not access services via GameWorld.
    /// </summary>
    [Fact]
    public void DomainEntities_NoSettableServiceProperties()
    {
        List<string> violations = new List<string>();

        // Service type suffixes that should NOT be settable properties on domain entities
        List<string> serviceTypeSuffixes = new List<string>
        {
            "Service", "Manager", "Facade", "Handler", "Processor", "Validator",
            "Calculator", "Builder", "Generator", "Resolver", "Executor", "Tracer",
            "Repository", "Provider", "Factory"
        };

        // Domain entity files to check
        List<string> domainEntityFiles = new List<string>
        {
            "GameWorld.cs", "Player.cs", "NPC.cs", "Location.cs", "Scene.cs",
            "Situation.cs", "Item.cs", "Route.cs", "TimeModel.cs", "TimeState.cs"
        };

        string srcPath = GetSourcePath();
        if (!Directory.Exists(srcPath)) return;

        foreach (string file in Directory.GetFiles(srcPath, "*.cs", SearchOption.AllDirectories))
        {
            string fileName = Path.GetFileName(file);
            if (!domainEntityFiles.Contains(fileName)) continue;
            if (file.Contains("obj") || file.Contains("bin")) continue;

            string[] lines = File.ReadAllLines(file);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                // Look for public settable properties: "public SomeService Foo { get; set; }"
                if (line.Contains("{ get; set; }") && line.Contains("public"))
                {
                    foreach (string suffix in serviceTypeSuffixes)
                    {
                        if (Regex.IsMatch(line, $@"\b\w*{suffix}\b.*\{{.*get;.*set;"))
                        {
                            violations.Add($"{fileName}:{i + 1}: {line.Trim()}");
                            break;
                        }
                    }
                }
            }
        }

        Assert.True(violations.Count == 0,
            $"Domain entities must not have settable service properties. Services should be injected directly:\n{string.Join("\n", violations)}");
    }

    /// <summary>
    /// Verify Program.cs does not assign services to domain objects after DI initialization.
    /// Pattern like "gameWorld.SomeService = app.Services.GetService<T>()" is forbidden.
    /// </summary>
    [Fact]
    public void ProgramCs_NoPostInitializationServiceAssignment()
    {
        List<string> violations = new List<string>();

        string srcPath = GetSourcePath();
        string programPath = Path.Combine(srcPath, "Program.cs");

        if (!File.Exists(programPath)) return;

        string[] lines = File.ReadAllLines(programPath);
        bool pastAppBuild = false;

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            // Track when we pass app.Build()
            if (line.Contains("app.Build()") || line.Contains("= builder.Build()"))
            {
                pastAppBuild = true;
            }

            // After app.Build(), any property assignment to domain objects is suspicious
            if (pastAppBuild)
            {
                // Pattern: someObject.Property = (anything from services)
                if (Regex.IsMatch(line, @"\w+\.\w+\s*=\s*") &&
                    (line.Contains("GetService") || line.Contains("GetRequiredService")))
                {
                    violations.Add($"Program.cs:{i + 1}: {line.Trim()}");
                }

                // Pattern: direct property assignment after build (e.g., initResult.GameWorld.Tracer = tracer)
                if (Regex.IsMatch(line, @"\.\w+\s*=\s*\w+;") &&
                    !line.TrimStart().StartsWith("//") &&
                    !line.Contains("Configuration") &&
                    !line.Contains("Logger"))
                {
                    // Check if assigning a variable that was obtained from services
                    string[] prevLines = lines.Skip(Math.Max(0, i - 5)).Take(5).ToArray();
                    bool isServiceVariable = prevLines.Any(pl =>
                        pl.Contains("GetService") || pl.Contains("GetRequiredService"));

                    if (isServiceVariable)
                    {
                        violations.Add($"Program.cs:{i + 1}: {line.Trim()}");
                    }
                }
            }
        }

        Assert.True(violations.Count == 0,
            $"Program.cs must not assign DI services to domain objects after app.Build(). Services should inject dependencies directly:\n{string.Join("\n", violations)}");
    }

    // ========== DI REGISTRATION CENTRALIZATION ==========

    /// <summary>
    /// Verify DI registration only happens in Program.cs or ServiceConfiguration.cs.
    /// Domain services and models should NOT contain DI registration logic.
    /// HIGHLANDER: One place for DI registration, no scattered configuration.
    /// </summary>
    [Fact]
    public void SourceCode_DIRegistration_OnlyInAllowedFiles()
    {
        List<string> violations = new List<string>();
        Regex diPattern = new Regex(@"services\.AddSingleton|services\.AddScoped|services\.AddTransient", RegexOptions.Compiled);

        List<string> allowedFileNames = new List<string> { "Program.cs", "ServiceConfiguration.cs" };

        string srcPath = GetSourcePath();
        Assert.True(Directory.Exists(srcPath), $"Source path should exist: {srcPath}");

        foreach (string file in Directory.GetFiles(srcPath, "*.cs", SearchOption.AllDirectories))
        {
            if (file.Contains("obj") || file.Contains("bin")) continue;

            string fileName = Path.GetFileName(file);

            if (allowedFileNames.Contains(fileName)) continue;

            string[] lines = File.ReadAllLines(file);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (diPattern.IsMatch(line) && !line.TrimStart().StartsWith("//"))
                {
                    violations.Add($"{fileName}:{i + 1}: {line.Trim()}");
                }
            }
        }

        Assert.True(violations.Count == 0,
            $"DI registration should ONLY be in Program.cs or ServiceConfiguration.cs:\n{string.Join("\n", violations)}");
    }

    /// <summary>
    /// Verify domain models (classes with state and constructors taking primitive parameters)
    /// are NOT registered as DI singletons. Models should be created explicitly where needed.
    /// </summary>
    [Fact]
    public void SourceCode_NoModelClasses_InDIRegistration()
    {
        List<string> violations = new List<string>();
        Regex modelRegistrationPattern = new Regex(@"AddSingleton<\w*Model>", RegexOptions.Compiled);

        string srcPath = GetSourcePath();
        Assert.True(Directory.Exists(srcPath), $"Source path should exist: {srcPath}");

        foreach (string file in Directory.GetFiles(srcPath, "*.cs", SearchOption.AllDirectories))
        {
            if (file.Contains("obj") || file.Contains("bin")) continue;

            string[] lines = File.ReadAllLines(file);
            string fileName = Path.GetFileName(file);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (modelRegistrationPattern.IsMatch(line) && !line.TrimStart().StartsWith("//"))
                {
                    violations.Add($"{fileName}:{i + 1}: {line.Trim()}");
                }
            }
        }

        Assert.True(violations.Count == 0,
            $"Domain models (classes ending with 'Model') should NOT be DI-registered. Create them explicitly:\n{string.Join("\n", violations)}");
    }

    private string GetSourcePath()
    {
        string basePath = AppDomain.CurrentDomain.BaseDirectory;
        string srcPath = Path.GetFullPath(Path.Combine(basePath, "..", "..", "..", "..", "src"));
        return srcPath;
    }

    // ========== HIGHLANDER: ONE WAY TO DO THINGS ==========

    /// <summary>
    /// HIGHLANDER: No optional parameters in public methods.
    /// Optional parameters create multiple ways to call the same method.
    /// If different inputs are needed, create a DIFFERENT method with a DIFFERENT name.
    /// One concept, one implementation, one way to call it.
    /// </summary>
    [Fact]
    public void PublicMethods_NoOptionalParameters()
    {
        Assembly assembly = typeof(GameWorld).Assembly;
        List<string> violations = new List<string>();

        foreach (Type type in assembly.GetTypes())
        {
            // Skip generated types and Blazor components
            if (type.Name.StartsWith("<")) continue;
            if (type.BaseType?.Name == "ComponentBase") continue;

            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);

            foreach (MethodInfo method in methods)
            {
                if (method.IsSpecialName) continue;

                foreach (ParameterInfo param in method.GetParameters())
                {
                    if (param.HasDefaultValue)
                    {
                        violations.Add($"{type.Name}.{method.Name}({param.Name} = {param.DefaultValue ?? "null"})");
                    }
                }
            }
        }

        Assert.True(violations.Count == 0, $"HIGHLANDER violation - optional parameters create multiple call paths:\n{string.Join("\n", violations)}");
    }
}

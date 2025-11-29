using System.IO;
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
}

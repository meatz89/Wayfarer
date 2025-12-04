using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;

namespace Wayfarer.Tests.Architecture;

/// <summary>
/// Detects methods called with null literal parameters.
///
/// ARCHITECTURAL PRINCIPLE: Null parameters are a code smell indicating:
/// 1. The method has too many responsibilities (should be split)
/// 2. The parameter is genuinely optional (use method overloads or separate methods)
/// 3. The caller doesn't have the required data (design problem)
///
/// This test identifies violations for holistic analysis and refactoring.
/// </summary>
public class NullParameterViolationTests
{
    private readonly ITestOutputHelper _output;
    private static readonly string SrcDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "src"));

    public NullParameterViolationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void DetectNullParameterUsage_GeneratesReport()
    {
        // This test generates a report of all null parameter usages
        // It does NOT fail - it's for analysis purposes

        List<NullParameterViolation> violations = new List<NullParameterViolation>();

        // Scan all C# files in src directory
        string[] csFiles = Directory.GetFiles(SrcDirectory, "*.cs", SearchOption.AllDirectories);

        // Pattern to match method calls with null as a parameter
        // Matches: methodName(anything, null) or methodName(null, anything) or methodName(null)
        Regex nullParamPattern = new Regex(@"(\w+)\s*\([^)]*\bnull\b[^)]*\)", RegexOptions.Compiled);

        foreach (string file in csFiles)
        {
            string relativePath = Path.GetRelativePath(SrcDirectory, file);
            string[] lines = File.ReadAllLines(file);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                // Skip comments
                string trimmed = line.Trim();
                if (trimmed.StartsWith("//") || trimmed.StartsWith("*") || trimmed.StartsWith("///"))
                    continue;

                // Skip null checks and null assignments
                if (trimmed.Contains("== null") || trimmed.Contains("!= null") ||
                    trimmed.Contains("?? null") || trimmed.Contains("= null;") ||
                    trimmed.Contains("is null") || trimmed.Contains("is not null"))
                    continue;

                // Find method calls with null parameters
                MatchCollection matches = nullParamPattern.Matches(line);
                foreach (Match match in matches)
                {
                    string methodCall = match.Value;
                    string methodName = match.Groups[1].Value;

                    // Skip common false positives
                    if (IsKnownSafePattern(methodName, methodCall))
                        continue;

                    violations.Add(new NullParameterViolation
                    {
                        File = relativePath,
                        Line = i + 1,
                        MethodName = methodName,
                        FullCall = methodCall.Trim(),
                        Context = trimmed.Length > 100 ? trimmed.Substring(0, 100) + "..." : trimmed
                    });
                }
            }
        }

        // Group by method name for analysis
        Dictionary<string, List<NullParameterViolation>> byMethod = violations
            .GroupBy(v => v.MethodName)
            .OrderByDescending(g => g.Count())
            .ToDictionary(g => g.Key, g => g.ToList());

        // Output report
        _output.WriteLine("=== NULL PARAMETER VIOLATION REPORT ===");
        _output.WriteLine($"Total violations: {violations.Count}");
        _output.WriteLine($"Unique methods: {byMethod.Count}");
        _output.WriteLine("");

        _output.WriteLine("=== BY METHOD (most frequent first) ===");
        foreach (KeyValuePair<string, List<NullParameterViolation>> kvp in byMethod)
        {
            _output.WriteLine($"\n{kvp.Key}: {kvp.Value.Count} occurrences");

            // Show first 3 examples
            foreach (NullParameterViolation violation in kvp.Value.Take(3))
            {
                _output.WriteLine($"  - {violation.File}:{violation.Line}");
                _output.WriteLine($"    {violation.Context}");
            }

            if (kvp.Value.Count > 3)
            {
                _output.WriteLine($"  ... and {kvp.Value.Count - 3} more");
            }
        }

        _output.WriteLine("\n=== SUGGESTED REFACTORING PRIORITIES ===");
        List<KeyValuePair<string, List<NullParameterViolation>>> topViolators = byMethod.Take(10).ToList();
        foreach (KeyValuePair<string, List<NullParameterViolation>> kvp in topViolators)
        {
            string suggestion = GetRefactoringSuggestion(kvp.Key, kvp.Value);
            _output.WriteLine($"{kvp.Key} ({kvp.Value.Count}x): {suggestion}");
        }

        // This test always passes - it's for generating reports
        Assert.True(true, "Report generated successfully");
    }

    [Fact]
    public void CountNullParameterViolations_ForTracking()
    {
        // This test counts violations for tracking progress over time
        // Add to CI to track technical debt

        int count = CountNullParameterViolations();

        _output.WriteLine($"Current null parameter violations: {count}");

        // Baseline: Record current count, reduce over time
        // Current state: 127 violations (2024-12-03)
        // Primary offenders: AddSystemMessage (73), ApplyConsequence (29), AddNarrativeMessage (9)
        int baseline = 130; // Baseline with 2% margin for minor variance

        Assert.True(count <= baseline,
            $"Null parameter violations ({count}) exceed baseline ({baseline}). " +
            $"Run DetectNullParameterUsage_GeneratesReport for details.");
    }

    private int CountNullParameterViolations()
    {
        int count = 0;
        string[] csFiles = Directory.GetFiles(SrcDirectory, "*.cs", SearchOption.AllDirectories);
        Regex nullParamPattern = new Regex(@"(\w+)\s*\([^)]*\bnull\b[^)]*\)", RegexOptions.Compiled);

        foreach (string file in csFiles)
        {
            string[] lines = File.ReadAllLines(file);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                string trimmed = line.Trim();

                if (trimmed.StartsWith("//") || trimmed.StartsWith("*") || trimmed.StartsWith("///"))
                    continue;

                if (trimmed.Contains("== null") || trimmed.Contains("!= null") ||
                    trimmed.Contains("?? null") || trimmed.Contains("= null;") ||
                    trimmed.Contains("is null") || trimmed.Contains("is not null"))
                    continue;

                MatchCollection matches = nullParamPattern.Matches(line);
                foreach (Match match in matches)
                {
                    string methodName = match.Groups[1].Value;
                    string methodCall = match.Value;

                    if (!IsKnownSafePattern(methodName, methodCall))
                    {
                        count++;
                    }
                }
            }
        }

        return count;
    }

    private bool IsKnownSafePattern(string methodName, string fullCall)
    {
        // Framework/library methods where null is idiomatic
        List<string> safePatterns = new List<string>
        {
            // .NET framework patterns
            "FirstOrDefault",
            "SingleOrDefault",
            "LastOrDefault",
            "GetValueOrDefault",
            "TryGetValue",
            "TryParse",
            "Parse",
            "Equals",
            "CompareTo",
            "Contains",
            "IndexOf",
            "Replace",
            "Split",
            "Join",
            "Concat",
            "Format",
            "ToString",
            "GetType",
            "GetHashCode",

            // Assertion/testing patterns
            "Assert",
            "Null",
            "NotNull",
            "Equal",
            "True",
            "False",

            // Constructor patterns (new X(null) for optional deps)
            "new",

            // Serialization
            "Serialize",
            "Deserialize",
            "JsonConvert",

            // Logging
            "Log",
            "Debug",
            "Info",
            "Warn",
            "Error",
            "WriteLine",

            // DI patterns
            "GetService",
            "GetRequiredService",
            "AddSingleton",
            "AddScoped",
            "AddTransient",

            // Exception constructors (message contains "null" text, not null param)
            "InvalidOperationException",
            "ArgumentException",
            "ArgumentNullException",
            "NullReferenceException",
            "Exception",

            // Result/status methods with string messages containing "null"
            "Failure",
            "Failed",
            "Success",

            // String building methods (only specific ones, not broad "Add" which catches domain methods)
            "Append",
            "AppendLine",
            "AppendFormat",
            "StringBuilder"
        };

        // Also filter if the null appears in a string literal (false positive)
        if (fullCall.Contains("\"") && fullCall.Contains("null"))
        {
            // Check if null is inside a string literal
            int quoteCount = fullCall.Count(c => c == '"');
            if (quoteCount >= 2)
            {
                // Likely a string containing "null" as text
                int firstQuote = fullCall.IndexOf('"');
                int lastQuote = fullCall.LastIndexOf('"');
                int nullIndex = fullCall.IndexOf("null");
                if (nullIndex > firstQuote && nullIndex < lastQuote)
                {
                    return true; // null is inside string literal
                }
            }
        }

        return safePatterns.Any(p => methodName.Contains(p, StringComparison.OrdinalIgnoreCase));
    }

    private string GetRefactoringSuggestion(string methodName, List<NullParameterViolation> violations)
    {
        // Analyze the pattern and suggest refactoring approach

        if (methodName.Contains("AddSystemMessage") || methodName.Contains("AddNarrativeMessage"))
        {
            return "Split into AddSystemMessage (with category) and AddSystemMessageUncategorized";
        }

        if (methodName.Contains("ApplyConsequence"))
        {
            return "Create ApplyConsequence overload or use NullObject pattern for situation";
        }

        if (methodName.Contains("Parse"))
        {
            return "Consider required GameWorld parameter or create test-specific parser";
        }

        if (methodName.Contains("Validate") || methodName.Contains("Extract"))
        {
            return "Use template values directly instead of nullable scaled values";
        }

        if (methodName.Contains("Create") || methodName.Contains("Generate"))
        {
            return "Consider builder pattern or factory method with explicit variants";
        }

        return "Analyze call sites - consider splitting method or creating specific overloads";
    }
}

public class NullParameterViolation
{
    public string File { get; set; }
    public int Line { get; set; }
    public string MethodName { get; set; }
    public string FullCall { get; set; }
    public string Context { get; set; }
}

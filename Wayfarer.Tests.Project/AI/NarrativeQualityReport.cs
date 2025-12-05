using System.Text.Json;

namespace Wayfarer.Tests.AI;

/// <summary>
/// Structured quality report for AI narrative evaluation.
/// Designed for EXPORT to be analyzed by AI agents or humans.
///
/// WORKFLOW:
/// 1. Tests run against real Ollama, producing quality reports
/// 2. Reports are exported to JSON
/// 3. JSON is sent to AI agent (Claude, etc.) for analysis
/// 4. AI agent proposes specific changes to ScenePromptBuilder.cs
/// 5. Human reviews and applies changes
///
/// This is NOT automatic - tests detect problems, humans/AI propose fixes.
/// </summary>
public class NarrativeQualityReport
{
    public string FixtureName { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string ModelName { get; set; } = "";

    // Input: What was sent to AI
    public string Prompt { get; set; } = "";

    // Context breakdown for AI agent to understand what generated the prompt
    public ContextBreakdown Context { get; set; } = new ContextBreakdown();

    // Output: What AI returned
    public string Response { get; set; } = "";
    public int ResponseLength { get; set; }
    public int ResponseWordCount { get; set; }
    public bool UsedFallback { get; set; }
    public string FallbackReason { get; set; } = "";

    // Quality assessment: What we expected vs what we got
    public QualityAssessment Quality { get; set; } = new QualityAssessment();

    // Result
    public bool Passed { get; set; }
    public List<string> FailureReasons { get; set; } = new List<string>();
}

/// <summary>
/// Breakdown of the context that generated the prompt.
/// Helps AI agent understand WHAT produced the prompt for targeted fixes.
/// </summary>
public class ContextBreakdown
{
    // Location context
    public string LocationName { get; set; } = "";
    public string LocationPurpose { get; set; } = "";
    public string LocationDescription { get; set; } = "";

    // NPC context
    public string NPCName { get; set; } = "";
    public string NPCPersonality { get; set; } = "";
    public string NPCProfession { get; set; } = "";
    public int NPCBondLevel { get; set; }

    // Route context (for travel)
    public string RouteName { get; set; } = "";
    public string RouteOrigin { get; set; } = "";
    public string RouteDestination { get; set; } = "";

    // Narrative hints
    public string Tone { get; set; } = "";
    public string Theme { get; set; } = "";
    public string HintContext { get; set; } = "";
    public string Style { get; set; } = "";

    // World state
    public string TimeBlock { get; set; } = "";
    public string Weather { get; set; } = "";
    public int Day { get; set; }

    /// <summary>
    /// Build context breakdown from ScenePromptContext.
    /// </summary>
    public static ContextBreakdown FromContext(ScenePromptContext context, NarrativeHints hints)
    {
        ContextBreakdown breakdown = new ContextBreakdown
        {
            TimeBlock = context.CurrentTimeBlock.ToString(),
            Weather = context.CurrentWeather.ToString(),
            Day = context.CurrentDay
        };

        if (context.Location != null)
        {
            breakdown.LocationName = context.Location.Name;
            breakdown.LocationPurpose = context.Location.Purpose.ToString();
            breakdown.LocationDescription = context.Location.Description ?? "";
        }

        if (context.NPC != null)
        {
            breakdown.NPCName = context.NPC.Name;
            breakdown.NPCPersonality = context.NPC.PersonalityType.ToString();
            breakdown.NPCProfession = context.NPC.Profession.ToString();
            breakdown.NPCBondLevel = context.NPCBondLevel;
        }

        if (context.Route != null)
        {
            breakdown.RouteName = context.Route.Name;
            breakdown.RouteOrigin = context.Route.OriginLocation?.Name ?? "";
            breakdown.RouteDestination = context.Route.DestinationLocation?.Name ?? "";
        }

        if (hints != null)
        {
            breakdown.Tone = hints.Tone ?? "";
            breakdown.Theme = hints.Theme ?? "";
            breakdown.HintContext = hints.Context ?? "";
            breakdown.Style = hints.Style ?? "";
        }

        return breakdown;
    }
}

/// <summary>
/// Quality assessment for AI agent analysis.
/// </summary>
public class QualityAssessment
{
    public List<string> ExpectedElements { get; set; } = new List<string>();
    public List<string> MissingElements { get; set; } = new List<string>();
    public List<string> FoundElements { get; set; } = new List<string>();

    public int ExpectedMinLength { get; set; }
    public int ExpectedMaxLength { get; set; }
    public bool LengthViolation { get; set; }
    public string LengthViolationType { get; set; } = ""; // "too_short" or "too_long"

    public string ExpectedTone { get; set; } = "";
}

/// <summary>
/// Aggregated report across multiple fixtures.
/// Designed for bulk export to AI agent for pattern analysis.
/// </summary>
public class NarrativeEvaluationSummary
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string ModelName { get; set; } = "";
    public int TotalFixtures { get; set; }
    public int PassedCount { get; set; }
    public int FailedCount { get; set; }
    public int FallbackCount { get; set; }

    public List<NarrativeQualityReport> Reports { get; set; } = new List<NarrativeQualityReport>();

    // Aggregated patterns for AI agent analysis
    public List<string> AllMissingElements { get; set; } = new List<string>();
    public List<string> AllFailureReasons { get; set; } = new List<string>();

    /// <summary>
    /// Export summary to JSON for AI agent consumption.
    /// </summary>
    public string ExportToJson()
    {
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        return JsonSerializer.Serialize(this, options);
    }

    /// <summary>
    /// Export summary to file.
    /// </summary>
    public void ExportToFile(string filePath)
    {
        string json = ExportToJson();
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// Aggregate patterns from all reports.
    /// </summary>
    public void AggregatePatterns()
    {
        AllMissingElements = Reports
            .SelectMany(r => r.Quality.MissingElements)
            .Distinct()
            .ToList();

        AllFailureReasons = Reports
            .SelectMany(r => r.FailureReasons)
            .Distinct()
            .ToList();

        FallbackCount = Reports.Count(r => r.UsedFallback);
    }
}

// NarrativeGenerationResult is defined in main project (src/Subsystems/Scene/NarrativeGenerationResult.cs)

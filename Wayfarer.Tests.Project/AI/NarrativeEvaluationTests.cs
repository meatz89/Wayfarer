using System.Text;
using Microsoft.Extensions.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace Wayfarer.Tests.AI;

/// <summary>
/// AI Narrative Evaluation Tests - Run against REAL Ollama.
/// NOT mocked - these tests evaluate actual AI output quality.
///
/// PURPOSE:
/// 1. Evaluate if AI responses meet quality criteria for defined contexts
/// 2. Identify prompts/contexts that need adjustment
/// 3. Compare different AI models (change OllamaConfiguration.Model)
/// 4. Optimize prompting strategies
///
/// ARCHITECTURE:
/// - Tests use SceneNarrativeService.GetPrompt*() to ensure test/production parity
/// - Tests use GenerateSituationNarrativeWithMetadataAsync for full tracking
/// - Results are exportable to JSON for AI agent analysis
///
/// USAGE:
/// - Requires Ollama running locally (tests skip if unavailable)
/// - Run with: dotnet test --filter "FullyQualifiedName~NarrativeEvaluationTests"
/// - Review output logs for full prompt/response pairs
/// - Export JSON reports for AI agent analysis
/// </summary>
public class NarrativeEvaluationTests : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private OllamaClient _ollamaClient = null!;
    private SceneNarrativeService _narrativeService = null!;
    private GameWorld _gameWorld = null!;
    private OllamaConfiguration _config = null!;
    private bool _ollamaAvailable;

    // Collect reports for export
    private NarrativeEvaluationSummary _evaluationSummary = null!;

    public NarrativeEvaluationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    public async Task InitializeAsync()
    {
        // Setup real Ollama client with configuration
        HttpClient httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(60); // Extended for slow PCs

        // ARCHITECTURAL GUARANTEE: Use OllamaConfiguration's compile-time defaults
        // Both tests and production use the SAME default values defined in OllamaConfiguration
        // No file loading, no configuration drift - the code IS the source of truth
        _config = new OllamaConfiguration();
        _ollamaClient = new OllamaClient(httpClient, _config);
        _gameWorld = new GameWorld();

        // Use production prompt builder through service (TEST/PRODUCTION PARITY)
        ScenePromptBuilder promptBuilder = new ScenePromptBuilder();
        _narrativeService = new SceneNarrativeService(_gameWorld, _ollamaClient, promptBuilder);

        // Initialize evaluation summary
        _evaluationSummary = new NarrativeEvaluationSummary
        {
            ModelName = _config.Model,
            Timestamp = DateTime.UtcNow
        };

        // Check if Ollama is available with warmup
        try
        {
            _ollamaAvailable = await _ollamaClient.CheckHealthAsync(CancellationToken.None);
            _output.WriteLine($"Ollama health check: {_ollamaAvailable}");
            _output.WriteLine($"Model: {_config.Model}");

            if (_ollamaAvailable)
            {
                // Warmup model to prevent 404s on cold start
                _output.WriteLine("Warming up model (may take up to 60s on first run)...");
                bool warmed = await _narrativeService.WarmupModelAsync(60);
                if (!warmed)
                {
                    _output.WriteLine("WARNING: Model warmup failed - some tests may use fallback");
                }
                else
                {
                    _output.WriteLine("Model warmup successful");
                }
            }
        }
        catch
        {
            _ollamaAvailable = false;
            _output.WriteLine("Ollama NOT available - tests will be skipped");
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    // ==================== SITUATION NARRATIVE EVALUATION ====================

    [Fact]
    public async Task Evaluate_InnkeeperLodgingNegotiation()
    {
        await EvaluateSituationNarrative(NarrativeTestFixtures.InnkeeperLodgingNegotiation());
    }

    [Fact]
    public async Task Evaluate_GuardCheckpointConfrontation()
    {
        await EvaluateSituationNarrative(NarrativeTestFixtures.GuardCheckpointConfrontation());
    }

    [Fact]
    public async Task Evaluate_MerchantInformationExchange()
    {
        await EvaluateSituationNarrative(NarrativeTestFixtures.MerchantInformationExchange());
    }

    [Fact]
    public async Task Evaluate_ScholarResearchAssistance()
    {
        await EvaluateSituationNarrative(NarrativeTestFixtures.ScholarResearchAssistance());
    }

    [Fact]
    public async Task Evaluate_ForestPathEncounter()
    {
        await EvaluateSituationNarrative(NarrativeTestFixtures.ForestPathEncounter());
    }

    // ==================== CHOICE LABEL EVALUATION ====================

    [Fact]
    public async Task Evaluate_DiplomaticInnkeeperApproach()
    {
        await EvaluateChoiceLabel(NarrativeTestFixtures.DiplomaticInnkeeperApproach());
    }

    [Fact]
    public async Task Evaluate_AuthoritativeGuardResponse()
    {
        await EvaluateChoiceLabel(NarrativeTestFixtures.AuthoritativeGuardResponse());
    }

    // ==================== BATCH EVALUATION WITH JSON EXPORT ====================

    [Fact]
    public async Task Evaluate_AllSituationFixtures_WithExport()
    {
        if (!_ollamaAvailable)
        {
            _output.WriteLine("SKIPPED: Ollama not available");
            return;
        }

        _output.WriteLine("========== BATCH SITUATION NARRATIVE EVALUATION ==========\n");

        List<NarrativeTestCase> fixtures = NarrativeTestFixtures.AllSituationFixtures();

        foreach (NarrativeTestCase fixture in fixtures)
        {
            NarrativeQualityReport report = await RunSituationEvaluationWithReport(fixture);
            _evaluationSummary.Reports.Add(report);
        }

        // Aggregate patterns
        _evaluationSummary.TotalFixtures = _evaluationSummary.Reports.Count;
        _evaluationSummary.PassedCount = _evaluationSummary.Reports.Count(r => r.Passed);
        _evaluationSummary.FailedCount = _evaluationSummary.Reports.Count(r => !r.Passed);
        _evaluationSummary.AggregatePatterns();

        // Print summary
        _output.WriteLine("\n========== EVALUATION SUMMARY ==========");
        _output.WriteLine($"Total fixtures: {_evaluationSummary.TotalFixtures}");
        _output.WriteLine($"Passed: {_evaluationSummary.PassedCount}");
        _output.WriteLine($"Failed: {_evaluationSummary.FailedCount}");
        _output.WriteLine($"Fallbacks used: {_evaluationSummary.FallbackCount}");

        foreach (NarrativeQualityReport report in _evaluationSummary.Reports)
        {
            string status = report.Passed ? "PASS" : "FAIL";
            string fallback = report.UsedFallback ? " (FALLBACK)" : "";
            _output.WriteLine($"  [{status}]{fallback} {report.FixtureName}: {string.Join("; ", report.FailureReasons)}");
        }

        // Export to JSON for AI agent analysis
        string json = _evaluationSummary.ExportToJson();
        _output.WriteLine("\n========== JSON EXPORT (for AI agent analysis) ==========");
        _output.WriteLine(json);

        // Save to file for AI agent analysis (in test project folder)
        string testProjectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        string exportPath = Path.Combine(testProjectDir, "AI", $"narrative_eval_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json");
        _evaluationSummary.ExportToFile(exportPath);
        _output.WriteLine($"\nExported to: {exportPath}");

        // Assert: All tests should pass (or be fallback-related)
        bool allPassed = _evaluationSummary.Reports.All(r => r.Passed);
        Assert.True(allPassed, "Some fixtures failed quality criteria. See JSON export for details.");
    }

    // ==================== CORE EVALUATION LOGIC ====================

    private async Task EvaluateSituationNarrative(NarrativeTestCase testCase)
    {
        if (!_ollamaAvailable)
        {
            _output.WriteLine($"SKIPPED: {testCase.Name} - Ollama not available");
            return;
        }

        _output.WriteLine($"========== {testCase.Name} ==========\n");

        NarrativeQualityReport report = await RunSituationEvaluationWithReport(testCase);

        // Log detailed results
        _output.WriteLine($"Result: {(report.Passed ? "PASS" : "FAIL")}");
        if (report.UsedFallback)
            _output.WriteLine($"FALLBACK USED: {report.FallbackReason}");
        _output.WriteLine($"Failures: {string.Join("; ", report.FailureReasons)}");

        Assert.True(report.Passed, string.Join("; ", report.FailureReasons));
    }

    private async Task<NarrativeQualityReport> RunSituationEvaluationWithReport(NarrativeTestCase testCase)
    {
        NarrativeQualityReport report = new NarrativeQualityReport
        {
            FixtureName = testCase.Name,
            ModelName = _config.Model,
            Context = ContextBreakdown.FromContext(testCase.Context, testCase.Hints)
        };

        // Log context
        LogContext(testCase);

        // Get prompt through SERVICE (TEST/PRODUCTION PARITY)
        // This ensures tests use the EXACT same prompt as production
        string prompt = _narrativeService.GetSituationPrompt(testCase.Context, testCase.Hints, testCase.Situation);
        report.Prompt = prompt;

        _output.WriteLine("--- PROMPT (from SceneNarrativeService.GetSituationPrompt) ---");
        _output.WriteLine(prompt);
        _output.WriteLine("--- END PROMPT ---\n");

        // Generate narrative through SERVICE (SAME CODE PATH AS PRODUCTION)
        NarrativeGenerationResult result = await _narrativeService.GenerateSituationNarrativeWithMetadataAsync(
            testCase.Context, testCase.Hints, testCase.Situation);

        report.Response = result.Narrative;
        report.ResponseLength = result.Narrative.Length;
        report.ResponseWordCount = result.Narrative.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        report.UsedFallback = result.UsedFallback;
        report.FallbackReason = result.FallbackReason;

        _output.WriteLine("--- AI RESPONSE ---");
        _output.WriteLine(result.Narrative);
        _output.WriteLine("--- END RESPONSE ---\n");

        if (result.UsedFallback)
        {
            _output.WriteLine($"WARNING: Fallback used - {result.FallbackReason}");
        }

        // Evaluate quality
        report.Quality = new QualityAssessment
        {
            ExpectedElements = testCase.RequiredContextMarkers,
            ExpectedMinLength = testCase.ExpectedLengthRange.Min,
            ExpectedMaxLength = testCase.ExpectedLengthRange.Max,
            ExpectedTone = testCase.ExpectedTone
        };

        // Track which valid entity names were used (for human review)
        List<string> usedEntityNames = new List<string>();
        foreach (string entityName in testCase.ValidEntityNames)
        {
            if (result.Narrative.Contains(entityName, StringComparison.OrdinalIgnoreCase))
            {
                usedEntityNames.Add(entityName);
            }
        }

        // Context marker validation: Check if N of M atmospheric markers appear
        foreach (string marker in testCase.RequiredContextMarkers)
        {
            if (result.Narrative.Contains(marker, StringComparison.OrdinalIgnoreCase))
            {
                report.Quality.FoundElements.Add(marker);
            }
            else
            {
                report.Quality.MissingElements.Add(marker);
            }
        }

        // Fail only if fewer than required context markers appear
        int foundMarkerCount = report.Quality.FoundElements.Count;
        if (testCase.RequiredMarkerCount > 0 && foundMarkerCount < testCase.RequiredMarkerCount)
        {
            report.FailureReasons.Add(
                $"Context insufficiently referenced: {foundMarkerCount}/{testCase.RequiredMarkerCount} markers found " +
                $"(available: {string.Join(", ", testCase.RequiredContextMarkers)})");
        }

        // Log entity name usage for human review (not a failure condition)
        _output.WriteLine($"Entity names used: {(usedEntityNames.Count > 0 ? string.Join(", ", usedEntityNames) : "none (pure atmosphere)")}");

        // Check length
        if (report.ResponseLength < testCase.ExpectedLengthRange.Min)
        {
            report.Quality.LengthViolation = true;
            report.Quality.LengthViolationType = "too_short";
            report.FailureReasons.Add($"Too short: {report.ResponseLength} chars (min: {testCase.ExpectedLengthRange.Min})");
        }
        if (report.ResponseLength > testCase.ExpectedLengthRange.Max)
        {
            report.Quality.LengthViolation = true;
            report.Quality.LengthViolationType = "too_long";
            report.FailureReasons.Add($"Too long: {report.ResponseLength} chars (max: {testCase.ExpectedLengthRange.Max})");
        }

        // Log quality assessment
        _output.WriteLine("--- QUALITY ASSESSMENT ---");
        _output.WriteLine($"Length: {report.ResponseLength} chars (expected: {testCase.ExpectedLengthRange.Min}-{testCase.ExpectedLengthRange.Max})");
        _output.WriteLine($"Context markers found: {report.Quality.FoundElements.Count}/{testCase.RequiredMarkerCount} required ({string.Join(", ", report.Quality.FoundElements)})");
        _output.WriteLine($"Expected tone: {testCase.ExpectedTone}");
        _output.WriteLine($"Used fallback: {report.UsedFallback}");
        _output.WriteLine($"Failures: {(report.FailureReasons.Count == 0 ? "None" : string.Join(", ", report.FailureReasons))}");
        _output.WriteLine("");

        report.Passed = report.FailureReasons.Count == 0;
        return report;
    }

    private async Task EvaluateChoiceLabel(ChoiceLabelTestCase testCase)
    {
        if (!_ollamaAvailable)
        {
            _output.WriteLine($"SKIPPED: {testCase.Name} - Ollama not available");
            return;
        }

        _output.WriteLine($"========== {testCase.Name} ==========\n");

        // Set situation description for context
        testCase.Situation.Description = "A tense moment of negotiation.";

        // Get prompt through SERVICE (TEST/PRODUCTION PARITY)
        string prompt = _narrativeService.GetChoiceLabelPrompt(
            testCase.Context, testCase.Situation, testCase.ChoiceTemplate,
            testCase.Requirement, testCase.Consequence);

        _output.WriteLine("--- PROMPT (from SceneNarrativeService.GetChoiceLabelPrompt) ---");
        _output.WriteLine(prompt);
        _output.WriteLine("--- END PROMPT ---\n");

        // Generate label through SERVICE
        string label = await _narrativeService.GenerateChoiceLabelAsync(
            testCase.Context, testCase.Situation, testCase.ChoiceTemplate,
            testCase.Requirement, testCase.Consequence);

        _output.WriteLine("--- AI RESPONSE ---");
        _output.WriteLine(label);
        _output.WriteLine("--- END RESPONSE ---\n");

        // Evaluate quality
        List<string> failures = new List<string>();

        // Check expected elements
        foreach (string element in testCase.ExpectedElements)
        {
            if (!label.Contains(element, StringComparison.OrdinalIgnoreCase))
            {
                failures.Add($"Missing expected element: '{element}'");
            }
        }

        // Check word count
        int wordCount = label.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        if (wordCount < testCase.ExpectedWordCountRange.Min)
        {
            failures.Add($"Too few words: {wordCount} (min: {testCase.ExpectedWordCountRange.Min})");
        }
        if (wordCount > testCase.ExpectedWordCountRange.Max)
        {
            failures.Add($"Too many words: {wordCount} (max: {testCase.ExpectedWordCountRange.Max})");
        }

        // Log quality assessment
        _output.WriteLine("--- QUALITY ASSESSMENT ---");
        _output.WriteLine($"Word count: {wordCount} (expected: {testCase.ExpectedWordCountRange.Min}-{testCase.ExpectedWordCountRange.Max})");
        _output.WriteLine($"Expected elements found: {testCase.ExpectedElements.Count - failures.Count(f => f.Contains("Missing"))}/{testCase.ExpectedElements.Count}");
        _output.WriteLine($"Failures: {(failures.Count == 0 ? "None" : string.Join(", ", failures))}");
        _output.WriteLine("");

        Assert.True(failures.Count == 0, string.Join("; ", failures));
    }

    private void LogContext(NarrativeTestCase testCase)
    {
        _output.WriteLine("--- CONTEXT ---");
        if (testCase.Context.Location != null)
            _output.WriteLine($"Location: {testCase.Context.Location.Name}");
        if (testCase.Context.NPC != null)
            _output.WriteLine($"NPC: {testCase.Context.NPC.Name} ({testCase.Context.NPC.PersonalityType}, {testCase.Context.NPC.Profession})");
        if (testCase.Context.Route != null)
            _output.WriteLine($"Route: {testCase.Context.Route.Name}");
        _output.WriteLine($"Time: {testCase.Context.CurrentTimeBlock}, Weather: {testCase.Context.CurrentWeather}");
        _output.WriteLine($"Hints - Tone: {testCase.Hints?.Tone}, Theme: {testCase.Hints?.Theme}");
        _output.WriteLine("");
    }
}

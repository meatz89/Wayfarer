using System.Runtime.CompilerServices;
using Xunit;

namespace Wayfarer.Tests.Services;

/// <summary>
/// Mock AI completion provider for testing batch parsing.
/// Returns predefined responses to test JSON parsing logic.
/// </summary>
public class MockAICompletionProvider : IAICompletionProvider
{
    private readonly string _response;

    public MockAICompletionProvider(string response)
    {
        _response = response;
    }

    public async IAsyncEnumerable<string> StreamCompletionAsync(
        string prompt,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.Yield(); // Yield to simulate async
        yield return _response;
    }

    public Task<bool> CheckHealthAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(true); // Mock always healthy
    }
}

/// <summary>
/// Tests for SceneNarrativeService (Pass 2 AI narrative generation).
/// Tests fallback behavior, null handling, and narrative output quality.
///
/// NOTE: OllamaClient is null in these tests, verifying graceful degradation.
/// Full AI integration tested separately with Ollama running.
/// </summary>
public class SceneNarrativeServiceTests
{
    private readonly GameWorld _gameWorld;
    private readonly ScenePromptBuilder _promptBuilder;
    private readonly SceneNarrativeService _service;

    public SceneNarrativeServiceTests()
    {
        _gameWorld = new GameWorld();
        _promptBuilder = new ScenePromptBuilder();
        // OllamaClient is null - tests fallback behavior
        _service = new SceneNarrativeService(_gameWorld, null, _promptBuilder);
    }

    // ========== FALLBACK BEHAVIOR TESTS ==========

    [Fact]
    public async Task GenerateSituationNarrativeAsync_WhenAIUnavailable_ReturnsFallbackText()
    {
        // Arrange: Context with location and NPC
        ScenePromptContext context = CreateContextWithLocationAndNPC();
        NarrativeHints hints = CreateDefaultHints();
        Situation situation = CreateTestSituation();

        // Act
        string narrative = await _service.GenerateSituationNarrativeAsync(context, hints, situation);

        // Assert: Fallback text is generated (not empty)
        Assert.NotNull(narrative);
        Assert.NotEmpty(narrative);
    }

    [Fact]
    public async Task GenerateSituationNarrativeAsync_WithLocationAndNPC_IncludesLocationName()
    {
        // Arrange
        ScenePromptContext context = CreateContextWithLocationAndNPC();
        context.Location.Name = "The Rusty Anchor Inn";
        NarrativeHints hints = CreateDefaultHints();
        Situation situation = CreateTestSituation();

        // Act
        string narrative = await _service.GenerateSituationNarrativeAsync(context, hints, situation);

        // Assert: Location name appears in narrative
        Assert.Contains("The Rusty Anchor Inn", narrative);
    }

    [Fact]
    public async Task GenerateSituationNarrativeAsync_WithLocationAndNPC_IncludesNPCName()
    {
        // Arrange
        ScenePromptContext context = CreateContextWithLocationAndNPC();
        context.NPC.Name = "Marcus the Merchant";
        NarrativeHints hints = CreateDefaultHints();
        Situation situation = CreateTestSituation();

        // Act
        string narrative = await _service.GenerateSituationNarrativeAsync(context, hints, situation);

        // Assert: NPC name appears in narrative
        Assert.Contains("Marcus the Merchant", narrative);
    }

    // ========== NULL HINTS HANDLING ==========

    [Fact]
    public async Task GenerateSituationNarrativeAsync_WithNullHints_ReturnsValidNarrative()
    {
        // Arrange
        ScenePromptContext context = CreateContextWithLocationAndNPC();
        Situation situation = CreateTestSituation();

        // Act: Pass null hints
        string narrative = await _service.GenerateSituationNarrativeAsync(context, null, situation);

        // Assert: Still produces valid output
        Assert.NotNull(narrative);
        Assert.NotEmpty(narrative);
    }

    [Fact]
    public async Task GenerateSituationNarrativeAsync_WithEmptyHints_ReturnsValidNarrative()
    {
        // Arrange
        ScenePromptContext context = CreateContextWithLocationAndNPC();
        NarrativeHints emptyHints = new NarrativeHints(); // All properties null/default
        Situation situation = CreateTestSituation();

        // Act
        string narrative = await _service.GenerateSituationNarrativeAsync(context, emptyHints, situation);

        // Assert
        Assert.NotNull(narrative);
        Assert.NotEmpty(narrative);
    }

    // ========== CONTEXT VALIDATION ==========

    [Fact]
    public async Task GenerateSituationNarrativeAsync_WithNullContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.GenerateSituationNarrativeAsync(null, null, null));
    }

    // ========== TIME/WEATHER CONTEXT ==========

    [Fact]
    public async Task GenerateSituationNarrativeAsync_WithMorningTime_IncludesMorningContext()
    {
        // Arrange
        ScenePromptContext context = CreateContextWithLocationAndNPC();
        context.CurrentTimeBlock = TimeBlocks.Morning;
        NarrativeHints hints = CreateDefaultHints();
        Situation situation = CreateTestSituation();

        // Act
        string narrative = await _service.GenerateSituationNarrativeAsync(context, hints, situation);

        // Assert: Contains morning-related text
        Assert.Contains("Morning", narrative);
    }

    [Fact]
    public async Task GenerateSituationNarrativeAsync_WithEveningTime_IncludesEveningContext()
    {
        // Arrange
        ScenePromptContext context = CreateContextWithLocationAndNPC();
        context.CurrentTimeBlock = TimeBlocks.Evening;
        NarrativeHints hints = CreateDefaultHints();
        Situation situation = CreateTestSituation();

        // Act
        string narrative = await _service.GenerateSituationNarrativeAsync(context, hints, situation);

        // Assert: Contains evening-related text
        Assert.Contains("Evening", narrative);
    }

    // ========== HELPER METHODS ==========

    private ScenePromptContext CreateContextWithLocationAndNPC()
    {
        Location location = new Location("Test Location")
        {
            Purpose = LocationPurpose.Dwelling,
            Privacy = LocationPrivacy.SemiPublic,
            Safety = LocationSafety.Safe,
            Activity = LocationActivity.Moderate
        };

        NPC npc = new NPC
        {
            Name = "Test NPC",
            PersonalityType = PersonalityType.MERCANTILE,
            Profession = Professions.Merchant
        };

        return new ScenePromptContext
        {
            Location = location,
            NPC = npc,
            CurrentTimeBlock = TimeBlocks.Midday,
            CurrentWeather = WeatherCondition.Clear,
            CurrentDay = 1,
            NPCBondLevel = 0
        };
    }

    private NarrativeHints CreateDefaultHints()
    {
        return new NarrativeHints
        {
            Tone = "neutral",
            Theme = "economic_negotiation",
            Context = "A merchant encounter",
            Style = "Victorian"
        };
    }

    private Situation CreateTestSituation()
    {
        return new Situation
        {
            Name = "Test Situation",
            Type = SituationType.Normal,
            SystemType = TacticalSystemType.Social
        };
    }
}

/// <summary>
/// Tests for batch choice label parsing in SceneNarrativeService.
/// Uses MockAICompletionProvider to test JSON parsing without real AI calls.
/// </summary>
public class SceneNarrativeServiceBatchParsingTests
{
    private readonly GameWorld _gameWorld;
    private readonly ScenePromptBuilder _promptBuilder;

    public SceneNarrativeServiceBatchParsingTests()
    {
        _gameWorld = new GameWorld();
        _promptBuilder = new ScenePromptBuilder();
    }

    // ========== VALID JSON PARSING ==========

    [Fact]
    public async Task GenerateBatchChoiceLabelsAsync_WithValidJson_ReturnsLabels()
    {
        // Arrange: Valid JSON response
        string jsonResponse = @"{""choices"": [""Ask about the rate"", ""Demand a room"", ""Negotiate terms"", ""Leave quietly""]}";
        MockAICompletionProvider mockProvider = new MockAICompletionProvider(jsonResponse);
        SceneNarrativeService service = new SceneNarrativeService(_gameWorld, mockProvider, _promptBuilder);
        service.SetOllamaAvailability(true);

        ScenePromptContext context = CreateTestContext();
        Situation situation = CreateTestSituation();
        List<ChoiceData> choicesData = CreateTestChoiceData(4);

        // Act
        List<string> labels = await service.GenerateBatchChoiceLabelsAsync(context, situation, choicesData);

        // Assert
        Assert.NotNull(labels);
        Assert.Equal(4, labels.Count);
        Assert.Equal("Ask about the rate", labels[0]);
        Assert.Equal("Demand a room", labels[1]);
    }

    [Fact]
    public async Task GenerateBatchChoiceLabelsAsync_WithMarkdownWrappedJson_ParsesCorrectly()
    {
        // Arrange: JSON wrapped in markdown code block
        string jsonResponse = @"```json
{""choices"": [""First choice"", ""Second choice"", ""Third choice"", ""Fourth choice""]}
```";
        MockAICompletionProvider mockProvider = new MockAICompletionProvider(jsonResponse);
        SceneNarrativeService service = new SceneNarrativeService(_gameWorld, mockProvider, _promptBuilder);
        service.SetOllamaAvailability(true);

        ScenePromptContext context = CreateTestContext();
        Situation situation = CreateTestSituation();
        List<ChoiceData> choicesData = CreateTestChoiceData(4);

        // Act
        List<string> labels = await service.GenerateBatchChoiceLabelsAsync(context, situation, choicesData);

        // Assert
        Assert.NotNull(labels);
        Assert.Equal(4, labels.Count);
        Assert.Equal("First choice", labels[0]);
    }

    [Fact]
    public async Task GenerateBatchChoiceLabelsAsync_WithPrefixedJson_ExtractsJson()
    {
        // Arrange: JSON with text prefix (AI explains before JSON)
        string jsonResponse = @"Here's the response:
{""choices"": [""Option A"", ""Option B"", ""Option C"", ""Option D""]}";
        MockAICompletionProvider mockProvider = new MockAICompletionProvider(jsonResponse);
        SceneNarrativeService service = new SceneNarrativeService(_gameWorld, mockProvider, _promptBuilder);
        service.SetOllamaAvailability(true);

        ScenePromptContext context = CreateTestContext();
        Situation situation = CreateTestSituation();
        List<ChoiceData> choicesData = CreateTestChoiceData(4);

        // Act
        List<string> labels = await service.GenerateBatchChoiceLabelsAsync(context, situation, choicesData);

        // Assert
        Assert.NotNull(labels);
        Assert.Equal(4, labels.Count);
        Assert.Equal("Option A", labels[0]);
    }

    // ========== MALFORMED JSON HANDLING ==========

    [Fact]
    public async Task GenerateBatchChoiceLabelsAsync_WithMalformedJson_UsesFallback()
    {
        // Arrange: Invalid JSON
        string jsonResponse = @"{ not valid json at all";
        MockAICompletionProvider mockProvider = new MockAICompletionProvider(jsonResponse);
        SceneNarrativeService service = new SceneNarrativeService(_gameWorld, mockProvider, _promptBuilder);
        service.SetOllamaAvailability(true);

        ScenePromptContext context = CreateTestContext();
        Situation situation = CreateTestSituation();
        List<ChoiceData> choicesData = CreateTestChoiceData(4);

        // Act
        List<string> labels = await service.GenerateBatchChoiceLabelsAsync(context, situation, choicesData);

        // Assert: Falls back to template-based labels
        Assert.NotNull(labels);
        Assert.Equal(4, labels.Count);
        // Fallback uses ActionTextTemplate from choice data
        Assert.Contains("Choice 1", labels[0]);
    }

    [Fact]
    public async Task GenerateBatchChoiceLabelsAsync_WithNoJsonObject_UsesFallback()
    {
        // Arrange: No JSON object in response
        string jsonResponse = "I cannot generate that content.";
        MockAICompletionProvider mockProvider = new MockAICompletionProvider(jsonResponse);
        SceneNarrativeService service = new SceneNarrativeService(_gameWorld, mockProvider, _promptBuilder);
        service.SetOllamaAvailability(true);

        ScenePromptContext context = CreateTestContext();
        Situation situation = CreateTestSituation();
        List<ChoiceData> choicesData = CreateTestChoiceData(4);

        // Act
        List<string> labels = await service.GenerateBatchChoiceLabelsAsync(context, situation, choicesData);

        // Assert: Falls back
        Assert.NotNull(labels);
        Assert.Equal(4, labels.Count);
    }

    // ========== COUNT MISMATCH HANDLING ==========

    [Fact]
    public async Task GenerateBatchChoiceLabelsAsync_WithTooFewLabels_UsesFallback()
    {
        // Arrange: Only 2 labels when 4 expected
        string jsonResponse = @"{""choices"": [""Label 1"", ""Label 2""]}";
        MockAICompletionProvider mockProvider = new MockAICompletionProvider(jsonResponse);
        SceneNarrativeService service = new SceneNarrativeService(_gameWorld, mockProvider, _promptBuilder);
        service.SetOllamaAvailability(true);

        ScenePromptContext context = CreateTestContext();
        Situation situation = CreateTestSituation();
        List<ChoiceData> choicesData = CreateTestChoiceData(4);

        // Act
        List<string> labels = await service.GenerateBatchChoiceLabelsAsync(context, situation, choicesData);

        // Assert: Falls back because count mismatch
        Assert.NotNull(labels);
        Assert.Equal(4, labels.Count);
        Assert.Contains("Choice 1", labels[0]); // Fallback template
    }

    [Fact]
    public async Task GenerateBatchChoiceLabelsAsync_WithEmptyChoicesArray_UsesFallback()
    {
        // Arrange: Empty choices array
        string jsonResponse = @"{""choices"": []}";
        MockAICompletionProvider mockProvider = new MockAICompletionProvider(jsonResponse);
        SceneNarrativeService service = new SceneNarrativeService(_gameWorld, mockProvider, _promptBuilder);
        service.SetOllamaAvailability(true);

        ScenePromptContext context = CreateTestContext();
        Situation situation = CreateTestSituation();
        List<ChoiceData> choicesData = CreateTestChoiceData(4);

        // Act
        List<string> labels = await service.GenerateBatchChoiceLabelsAsync(context, situation, choicesData);

        // Assert: Falls back
        Assert.NotNull(labels);
        Assert.Equal(4, labels.Count);
    }

    // ========== OLLAMA UNAVAILABLE ==========

    [Fact]
    public async Task GenerateBatchChoiceLabelsAsync_WhenOllamaUnavailable_UsesFallbackImmediately()
    {
        // Arrange: Ollama marked unavailable
        string jsonResponse = @"{""choices"": [""AI Label""]}";
        MockAICompletionProvider mockProvider = new MockAICompletionProvider(jsonResponse);
        SceneNarrativeService service = new SceneNarrativeService(_gameWorld, mockProvider, _promptBuilder);
        service.SetOllamaAvailability(false); // Unavailable!

        ScenePromptContext context = CreateTestContext();
        Situation situation = CreateTestSituation();
        List<ChoiceData> choicesData = CreateTestChoiceData(4);

        // Act
        List<string> labels = await service.GenerateBatchChoiceLabelsAsync(context, situation, choicesData);

        // Assert: Uses fallback (doesn't even try AI)
        Assert.NotNull(labels);
        Assert.Equal(4, labels.Count);
        Assert.Contains("Choice 1", labels[0]); // Fallback template, not "AI Label"
    }

    // ========== HELPER METHODS ==========

    private ScenePromptContext CreateTestContext()
    {
        Location location = new Location("Test Inn")
        {
            Purpose = LocationPurpose.Dwelling,
            Privacy = LocationPrivacy.SemiPublic,
            Safety = LocationSafety.Safe,
            Activity = LocationActivity.Moderate
        };

        NPC npc = new NPC
        {
            Name = "Test NPC",
            PersonalityType = PersonalityType.MERCANTILE,
            Profession = Professions.Merchant
        };

        return new ScenePromptContext
        {
            Location = location,
            NPC = npc,
            CurrentTimeBlock = TimeBlocks.Midday,
            CurrentWeather = WeatherCondition.Clear,
            CurrentDay = 1,
            NPCBondLevel = 0
        };
    }

    private Situation CreateTestSituation()
    {
        return new Situation
        {
            Name = "Test Situation",
            Type = SituationType.Normal,
            SystemType = TacticalSystemType.Social,
            Description = "A test situation with friction."
        };
    }

    private List<ChoiceData> CreateTestChoiceData(int count)
    {
        List<ChoiceData> choices = new List<ChoiceData>();
        for (int i = 1; i <= count; i++)
        {
            ChoiceTemplate template = new ChoiceTemplate
            {
                ActionTextTemplate = $"Choice {i} action",
                PathType = ChoicePathType.Fallback
            };
            choices.Add(new ChoiceData(template, null, null));
        }
        return choices;
    }
}

using Xunit;

namespace Wayfarer.Tests.Services;

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

    // ========== SYNCHRONOUS METHOD TESTS ==========

    [Fact]
    public void GenerateSituationNarrative_Sync_ReturnsValidNarrative()
    {
        // Arrange
        ScenePromptContext context = CreateContextWithLocationAndNPC();
        NarrativeHints hints = CreateDefaultHints();

        // Act
        string narrative = _service.GenerateSituationNarrative(context, hints);

        // Assert
        Assert.NotNull(narrative);
        Assert.NotEmpty(narrative);
    }

    [Fact]
    public void GenerateSituationNarrative_Sync_WithNullContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => _service.GenerateSituationNarrative(null, null));
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
            CurrentWeather = "clear",
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

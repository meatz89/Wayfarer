using Xunit;

namespace Wayfarer.Tests.Services;

/// <summary>
/// Tests for ScenePromptBuilder (AI prompt construction for Pass 2 narrative).
/// Verifies prompt structure, section inclusion, and null handling.
/// </summary>
public class ScenePromptBuilderTests
{
    private readonly ScenePromptBuilder _builder;

    public ScenePromptBuilderTests()
    {
        _builder = new ScenePromptBuilder();
    }

    // ========== FULL CONTEXT TESTS ==========

    [Fact]
    public void BuildSituationPrompt_WithFullContext_IncludesWorldContextSection()
    {
        // Arrange
        ScenePromptContext context = CreateFullContext();
        NarrativeHints hints = CreateDefaultHints();
        Situation situation = CreateTestSituation();

        // Act
        string prompt = _builder.BuildSituationPrompt(context, hints, situation);

        // Assert
        Assert.Contains("## World Context", prompt);
        Assert.Contains("Time:", prompt);
        Assert.Contains("Day:", prompt);
    }

    [Fact]
    public void BuildSituationPrompt_WithFullContext_IncludesLocationSection()
    {
        // Arrange
        ScenePromptContext context = CreateFullContext();
        context.Location.Name = "The Golden Compass Inn";
        NarrativeHints hints = CreateDefaultHints();
        Situation situation = CreateTestSituation();

        // Act
        string prompt = _builder.BuildSituationPrompt(context, hints, situation);

        // Assert
        Assert.Contains("## Location", prompt);
        Assert.Contains("The Golden Compass Inn", prompt);
    }

    [Fact]
    public void BuildSituationPrompt_WithFullContext_IncludesCharacterSection()
    {
        // Arrange
        ScenePromptContext context = CreateFullContext();
        context.NPC.Name = "Helena the Healer";
        NarrativeHints hints = CreateDefaultHints();
        Situation situation = CreateTestSituation();

        // Act
        string prompt = _builder.BuildSituationPrompt(context, hints, situation);

        // Assert
        Assert.Contains("## Character Present", prompt);
        Assert.Contains("Helena the Healer", prompt);
    }

    [Fact]
    public void BuildSituationPrompt_WithFullContext_IncludesNarrativeDirectionSection()
    {
        // Arrange
        ScenePromptContext context = CreateFullContext();
        NarrativeHints hints = new NarrativeHints
        {
            Tone = "tense",
            Theme = "confrontation",
            Context = "A dispute over payment",
            Style = "dramatic"
        };
        Situation situation = CreateTestSituation();

        // Act
        string prompt = _builder.BuildSituationPrompt(context, hints, situation);

        // Assert
        Assert.Contains("## Narrative Direction", prompt);
        Assert.Contains("tense", prompt);
        Assert.Contains("confrontation", prompt);
    }

    [Fact]
    public void BuildSituationPrompt_WithFullContext_IncludesSituationSection()
    {
        // Arrange
        ScenePromptContext context = CreateFullContext();
        NarrativeHints hints = CreateDefaultHints();
        Situation situation = CreateTestSituation();
        situation.Name = "Contract Dispute";

        // Act
        string prompt = _builder.BuildSituationPrompt(context, hints, situation);

        // Assert
        Assert.Contains("## Situation", prompt);
        Assert.Contains("Contract Dispute", prompt);
    }

    [Fact]
    public void BuildSituationPrompt_WithFullContext_IncludesOutputInstructions()
    {
        // Arrange
        ScenePromptContext context = CreateFullContext();
        NarrativeHints hints = CreateDefaultHints();
        Situation situation = CreateTestSituation();

        // Act
        string prompt = _builder.BuildSituationPrompt(context, hints, situation);

        // Assert: Contains output guidance for AI
        Assert.Contains("## Output", prompt);
        Assert.Contains("2-3 sentence", prompt);
    }

    // ========== NULL HANDLING TESTS ==========

    [Fact]
    public void BuildSituationPrompt_WithNullNPC_OmitsCharacterSection()
    {
        // Arrange
        ScenePromptContext context = CreateFullContext();
        context.NPC = null; // No NPC
        NarrativeHints hints = CreateDefaultHints();
        Situation situation = CreateTestSituation();

        // Act
        string prompt = _builder.BuildSituationPrompt(context, hints, situation);

        // Assert: Location section present, Character section absent
        Assert.Contains("## Location", prompt);
        Assert.DoesNotContain("## Character Present", prompt);
    }

    [Fact]
    public void BuildSituationPrompt_WithNullLocation_OmitsLocationSection()
    {
        // Arrange
        ScenePromptContext context = CreateFullContext();
        context.Location = null; // No location
        NarrativeHints hints = CreateDefaultHints();
        Situation situation = CreateTestSituation();

        // Act
        string prompt = _builder.BuildSituationPrompt(context, hints, situation);

        // Assert: World context present, Location section absent
        Assert.Contains("## World Context", prompt);
        Assert.DoesNotContain("## Location", prompt);
    }

    [Fact]
    public void BuildSituationPrompt_WithNullHints_OmitsNarrativeDirectionSection()
    {
        // Arrange
        ScenePromptContext context = CreateFullContext();
        Situation situation = CreateTestSituation();

        // Act
        string prompt = _builder.BuildSituationPrompt(context, null, situation);

        // Assert
        Assert.DoesNotContain("## Narrative Direction", prompt);
    }

    [Fact]
    public void BuildSituationPrompt_WithNullSituation_OmitsSituationSection()
    {
        // Arrange
        ScenePromptContext context = CreateFullContext();
        NarrativeHints hints = CreateDefaultHints();

        // Act
        string prompt = _builder.BuildSituationPrompt(context, hints, null);

        // Assert
        Assert.DoesNotContain("## Situation", prompt);
    }

    // ========== WEATHER CONTEXT TESTS ==========

    [Fact]
    public void BuildSituationPrompt_WithWeather_IncludesWeatherInWorldContext()
    {
        // Arrange
        ScenePromptContext context = CreateFullContext();
        context.CurrentWeather = WeatherCondition.Rain;
        NarrativeHints hints = CreateDefaultHints();
        Situation situation = CreateTestSituation();

        // Act
        string prompt = _builder.BuildSituationPrompt(context, hints, situation);

        // Assert
        Assert.Contains("Weather:", prompt);
        Assert.Contains("rain", prompt);
    }

    // ========== ROUTE CONTEXT TESTS ==========

    [Fact]
    public void BuildSituationPrompt_WithRoute_IncludesJourneySection()
    {
        // Arrange
        ScenePromptContext context = CreateFullContext();
        context.Route = new RouteOption
        {
            Name = "Mountain Pass Trail"
        };
        NarrativeHints hints = CreateDefaultHints();
        Situation situation = CreateTestSituation();

        // Act
        string prompt = _builder.BuildSituationPrompt(context, hints, situation);

        // Assert
        Assert.Contains("## Journey", prompt);
        Assert.Contains("Mountain Pass Trail", prompt);
    }

    // ========== HELPER METHODS ==========

    private ScenePromptContext CreateFullContext()
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
            Context = "A test context",
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

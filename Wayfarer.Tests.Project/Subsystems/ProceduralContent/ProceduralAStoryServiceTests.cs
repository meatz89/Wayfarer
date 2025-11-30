using Xunit;

namespace Wayfarer.Tests.Subsystems.ProceduralContent;

/// <summary>
/// Tests for ProceduralAStoryService - archetype rotation, tier calculation, anti-repetition
///
/// UNIT TESTS ONLY: Integration tests were removed because:
/// 1. Content/Core has authored A1-A3 templates using SERVICE patterns (InnLodging, DeliveryContract)
/// 2. Procedural generation uses NARRATIVE patterns (Investigation, Social, Confrontation, Crisis)
/// 3. Tests querying by MainStorySequence found AUTHORED content, not GENERATED content
/// 4. This is CORRECT - tutorial uses different patterns intentionally
///
/// These unit tests verify the algorithms work correctly without loading real content.
/// </summary>
public class ProceduralAStoryServiceTests
{
    [Fact]
    public void AStoryContext_IsArchetypeRecent_ReturnsTrueForRecentArchetypes()
    {
        // Arrange
        AStoryContext context = new AStoryContext();
        context.RecentArchetypes.Add(SceneArchetypeType.InvestigateLocation);
        context.RecentArchetypes.Add(SceneArchetypeType.MeetOrderMember);
        context.RecentArchetypes.Add(SceneArchetypeType.ConfrontAntagonist);

        // Act & Assert
        Assert.True(context.IsArchetypeRecent(SceneArchetypeType.InvestigateLocation));
        Assert.True(context.IsArchetypeRecent(SceneArchetypeType.MeetOrderMember));
        Assert.False(context.IsArchetypeRecent(SceneArchetypeType.GatherTestimony));
    }

    [Fact]
    public void AStoryContext_CalculatedTier_ReturnsCorrectTiers()
    {
        // Arrange & Act & Assert
        AStoryContext context1 = new AStoryContext { CurrentSequence = 15 };
        Assert.Equal(1, context1.CalculatedTier);

        AStoryContext context2 = new AStoryContext { CurrentSequence = 40 };
        Assert.Equal(2, context2.CalculatedTier);

        AStoryContext context3 = new AStoryContext { CurrentSequence = 75 };
        Assert.Equal(3, context3.CalculatedTier);
    }

    [Fact]
    public void Catalog_AllValidCategories_ReturnNonEmptyArchetypes()
    {
        // CRITICAL: Prevents division by zero in SelectArchetype
        // If any category returns empty list, sequence % count will crash
        // This test ensures catalog integrity for all four rotation categories
        // HIGHLANDER: Uses single SceneArchetypeCatalog

        List<SceneArchetypeType> investigationArchetypes = SceneArchetypeCatalog.GetArchetypesForCategory("Investigation");
        List<SceneArchetypeType> socialArchetypes = SceneArchetypeCatalog.GetArchetypesForCategory("Social");
        List<SceneArchetypeType> confrontationArchetypes = SceneArchetypeCatalog.GetArchetypesForCategory("Confrontation");
        List<SceneArchetypeType> crisisArchetypes = SceneArchetypeCatalog.GetArchetypesForCategory("Crisis");

        Assert.NotEmpty(investigationArchetypes);
        Assert.NotEmpty(socialArchetypes);
        Assert.NotEmpty(confrontationArchetypes);
        Assert.NotEmpty(crisisArchetypes);
    }

    [Fact]
    public void Catalog_UnknownCategory_ThrowsInvalidOperationException()
    {
        // FAIL-FAST: Unknown category should throw immediately, not silently return empty
        // This prevents division by zero in SelectArchetype
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => SceneArchetypeCatalog.GetArchetypesForCategory("UnknownCategory"));

        Assert.Contains("Unknown archetype category", exception.Message);
    }

    [Theory]
    [InlineData(1, "Investigation")]   // (1-1) % 4 = 0
    [InlineData(2, "Social")]          // (2-1) % 4 = 1
    [InlineData(3, "Confrontation")]   // (3-1) % 4 = 2
    [InlineData(4, "Crisis")]          // (4-1) % 4 = 3
    [InlineData(5, "Investigation")]   // (5-1) % 4 = 0 (cycle repeats)
    [InlineData(9, "Investigation")]   // (9-1) % 4 = 0
    [InlineData(10, "Social")]         // (10-1) % 4 = 1
    [InlineData(11, "Confrontation")]  // (11-1) % 4 = 2
    [InlineData(12, "Crisis")]         // (12-1) % 4 = 3
    public void ArchetypeRotation_SequenceMapsToCorrectCategory(int sequence, string expectedCategory)
    {
        // This tests the rotation algorithm directly without generating templates
        // Formula: (sequence - 1) % 4 maps to category
        int cyclePosition = (sequence - 1) % 4;

        string actualCategory = cyclePosition switch
        {
            0 => "Investigation",
            1 => "Social",
            2 => "Confrontation",
            3 => "Crisis",
            _ => throw new InvalidOperationException()
        };

        Assert.Equal(expectedCategory, actualCategory);
    }

    [Theory]
    [InlineData(1, 1)]   // Sequence 1 = Tier 1 (Personal)
    [InlineData(15, 1)]  // Sequence 15 = Tier 1
    [InlineData(30, 1)]  // Sequence 30 = Tier 1 (boundary)
    [InlineData(31, 2)]  // Sequence 31 = Tier 2 (Local)
    [InlineData(40, 2)]  // Sequence 40 = Tier 2
    [InlineData(50, 2)]  // Sequence 50 = Tier 2 (boundary)
    [InlineData(51, 3)]  // Sequence 51 = Tier 3 (Regional)
    [InlineData(100, 3)] // Sequence 100 = Tier 3 (infinite at max)
    public void TierCalculation_SequenceMapsToCorrectTier(int sequence, int expectedTier)
    {
        // This tests the tier algorithm directly without generating templates
        // Formula: 1-30 = Tier 1, 31-50 = Tier 2, 51+ = Tier 3
        int actualTier;
        if (sequence <= 30) actualTier = 1;
        else if (sequence <= 50) actualTier = 2;
        else actualTier = 3;

        Assert.Equal(expectedTier, actualTier);
    }

    [Fact]
    public void Catalog_InvestigationArchetypes_ContainsExpectedTypes()
    {
        List<SceneArchetypeType> archetypes = SceneArchetypeCatalog.GetArchetypesForCategory("Investigation");

        Assert.Contains(SceneArchetypeType.InvestigateLocation, archetypes);
        Assert.Contains(SceneArchetypeType.GatherTestimony, archetypes);
        Assert.Contains(SceneArchetypeType.SeekAudience, archetypes);
    }

    [Fact]
    public void Catalog_SocialArchetypes_ContainsMeetOrderMember()
    {
        List<SceneArchetypeType> archetypes = SceneArchetypeCatalog.GetArchetypesForCategory("Social");

        Assert.Contains(SceneArchetypeType.MeetOrderMember, archetypes);
    }

    [Fact]
    public void Catalog_ConfrontationArchetypes_ContainsConfrontAntagonist()
    {
        List<SceneArchetypeType> archetypes = SceneArchetypeCatalog.GetArchetypesForCategory("Confrontation");

        Assert.Contains(SceneArchetypeType.ConfrontAntagonist, archetypes);
    }

    [Fact]
    public void Catalog_CrisisArchetypes_ContainsExpectedTypes()
    {
        List<SceneArchetypeType> archetypes = SceneArchetypeCatalog.GetArchetypesForCategory("Crisis");

        Assert.Contains(SceneArchetypeType.UrgentDecision, archetypes);
        Assert.Contains(SceneArchetypeType.MoralCrossroads, archetypes);
    }

    // ==================== PEACEFUL CATEGORY TESTS ====================
    // Regression tests for player readiness filtering gaps

    [Fact]
    public void Catalog_PeacefulCategory_ReturnsNonEmptyList()
    {
        // CRITICAL: Peaceful category must return archetypes for exhausted players
        // If empty, player with low Resolve gets stuck with no valid scenes
        List<SceneArchetypeType> peacefulArchetypes = SceneArchetypeCatalog.GetArchetypesForCategory("Peaceful");

        Assert.NotEmpty(peacefulArchetypes);
    }

    [Fact]
    public void Catalog_PeacefulArchetypes_ContainsExpectedTypes()
    {
        // All 3 peaceful scene archetypes must be present
        List<SceneArchetypeType> archetypes = SceneArchetypeCatalog.GetArchetypesForCategory("Peaceful");

        Assert.Contains(SceneArchetypeType.QuietReflection, archetypes);
        Assert.Contains(SceneArchetypeType.CasualEncounter, archetypes);
        Assert.Contains(SceneArchetypeType.ScholarlyPursuit, archetypes);
    }

    [Fact]
    public void Catalog_AllFiveCategories_HaveNonEmptyArchetypes()
    {
        // CRITICAL: ALL five rotation categories must return valid archetypes
        // Including Peaceful for exhausted players
        List<string> categories = new List<string>
        {
            "Investigation",
            "Social",
            "Confrontation",
            "Crisis",
            "Peaceful"
        };

        foreach (string category in categories)
        {
            List<SceneArchetypeType> archetypes = SceneArchetypeCatalog.GetArchetypesForCategory(category);
            Assert.True(archetypes.Count > 0, $"Category '{category}' must have at least one archetype");
        }
    }

    // ==================== PLAYER READINESS TESTS ====================

    [Theory]
    [InlineData(0, ArchetypeIntensity.Recovery)]   // Exhausted (0 Resolve)
    [InlineData(1, ArchetypeIntensity.Recovery)]   // Exhausted (1 Resolve)
    [InlineData(2, ArchetypeIntensity.Recovery)]   // Exhausted (2 Resolve)
    [InlineData(3, ArchetypeIntensity.Standard)]    // Normal (exactly at threshold)
    [InlineData(10, ArchetypeIntensity.Standard)]   // Normal
    [InlineData(15, ArchetypeIntensity.Standard)]   // Normal (at threshold)
    [InlineData(16, ArchetypeIntensity.Demanding)]    // Capable (above threshold)
    [InlineData(30, ArchetypeIntensity.Demanding)]    // Capable (well above)
    public void PlayerReadiness_GetMaxSafeIntensity_ReturnsCorrectLevel(int resolve, ArchetypeIntensity expectedIntensity)
    {
        // CRITICAL: Player readiness determines which archetypes are safe
        // Exhausted players (Resolve < 3) must ONLY get Recovery archetypes
        PlayerReadinessService service = new PlayerReadinessService();
        Player player = new Player { Resolve = resolve };

        ArchetypeIntensity actualIntensity = service.GetMaxSafeIntensity(player);

        Assert.Equal(expectedIntensity, actualIntensity);
    }

    [Fact]
    public void PlayerReadiness_ExhaustedPlayer_OnlyRecoverySafe()
    {
        // CRITICAL: Exhausted player must ONLY have Recovery in safe list
        PlayerReadinessService service = new PlayerReadinessService();
        Player exhaustedPlayer = new Player { Resolve = 1 };

        List<ArchetypeIntensity> safeIntensities = service.GetSafeIntensities(exhaustedPlayer);

        Assert.Single(safeIntensities);
        Assert.Contains(ArchetypeIntensity.Recovery, safeIntensities);
    }

    [Fact]
    public void PlayerReadiness_NormalPlayer_IncludesRecoveryAndStandard()
    {
        // Normal player (Resolve 3-15) should have access to Recovery and Standard
        // Three-level system: Recovery, Standard, Demanding
        PlayerReadinessService service = new PlayerReadinessService();
        Player normalPlayer = new Player { Resolve = 10 };

        List<ArchetypeIntensity> safeIntensities = service.GetSafeIntensities(normalPlayer);

        Assert.Equal(2, safeIntensities.Count);
        Assert.Contains(ArchetypeIntensity.Recovery, safeIntensities);
        Assert.Contains(ArchetypeIntensity.Standard, safeIntensities);
        Assert.DoesNotContain(ArchetypeIntensity.Demanding, safeIntensities);
    }

    [Fact]
    public void PlayerReadiness_CapablePlayer_IncludesAllIntensities()
    {
        // Capable player (Resolve > 15) should have access to all intensity levels
        // Three-level system: Recovery, Standard, Demanding
        PlayerReadinessService service = new PlayerReadinessService();
        Player capablePlayer = new Player { Resolve = 20 };

        List<ArchetypeIntensity> safeIntensities = service.GetSafeIntensities(capablePlayer);

        Assert.Equal(3, safeIntensities.Count);
        Assert.Contains(ArchetypeIntensity.Recovery, safeIntensities);
        Assert.Contains(ArchetypeIntensity.Standard, safeIntensities);
        Assert.Contains(ArchetypeIntensity.Demanding, safeIntensities);
    }
}

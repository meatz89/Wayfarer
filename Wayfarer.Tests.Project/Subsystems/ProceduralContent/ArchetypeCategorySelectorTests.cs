using Xunit;

namespace Wayfarer.Tests.Subsystems.ProceduralContent;

/// <summary>
/// Tests for ArchetypeCategorySelector utility methods.
///
/// NOTE: SelectCategory tests have been REMOVED.
/// Scene selection now uses ProceduralAStoryService.SelectArchetypeCategory
/// which takes SceneSelectionInputs (context injection pattern).
/// See HighlanderSceneGenerationTests for selection logic tests.
///
/// These tests verify ONLY the static mapping utilities:
/// - MapArchetypeToCategory: SceneArchetypeType → Category string
/// - MapArchetypeToIntensity: SceneArchetypeType → ArchetypeIntensity
/// </summary>
public class ArchetypeCategorySelectorTests
{
    // ==================== ARCHETYPE TO CATEGORY MAPPING TESTS ====================

    [Theory]
    [InlineData(SceneArchetypeType.InvestigateLocation, "Investigation")]
    [InlineData(SceneArchetypeType.GatherTestimony, "Investigation")]
    [InlineData(SceneArchetypeType.SeekAudience, "Investigation")]
    [InlineData(SceneArchetypeType.DiscoverArtifact, "Investigation")]
    [InlineData(SceneArchetypeType.UncoverConspiracy, "Investigation")]
    [InlineData(SceneArchetypeType.MeetOrderMember, "Social")]
    [InlineData(SceneArchetypeType.ConfrontAntagonist, "Confrontation")]
    [InlineData(SceneArchetypeType.UrgentDecision, "Crisis")]
    [InlineData(SceneArchetypeType.MoralCrossroads, "Crisis")]
    [InlineData(SceneArchetypeType.QuietReflection, "Peaceful")]
    [InlineData(SceneArchetypeType.CasualEncounter, "Peaceful")]
    [InlineData(SceneArchetypeType.ScholarlyPursuit, "Peaceful")]
    public void MapArchetypeToCategory_ReturnsCorrectCategory(
        SceneArchetypeType archetype, string expectedCategory)
    {
        // Act
        string result = ArchetypeCategorySelector.MapArchetypeToCategory(archetype);

        // Assert
        Assert.Equal(expectedCategory, result);
    }

    [Theory]
    [InlineData(SceneArchetypeType.InnLodging, "Service")]
    [InlineData(SceneArchetypeType.ConsequenceReflection, "Service")]
    [InlineData(SceneArchetypeType.DeliveryContract, "Service")]
    [InlineData(SceneArchetypeType.RouteSegmentTravel, "Service")]
    public void MapArchetypeToCategory_ServicePatterns_ReturnServiceCategory(
        SceneArchetypeType archetype, string expectedCategory)
    {
        // Service patterns should NOT be tracked for A-story rhythm
        // If they reach intensity recording, it indicates a data authoring error
        // (service pattern marked as MainStory category)
        string result = ArchetypeCategorySelector.MapArchetypeToCategory(archetype);
        Assert.Equal(expectedCategory, result);
    }

    // ==================== ARCHETYPE TO INTENSITY MAPPING TESTS ====================

    [Theory]
    [InlineData(SceneArchetypeType.QuietReflection, ArchetypeIntensity.Recovery)]
    [InlineData(SceneArchetypeType.CasualEncounter, ArchetypeIntensity.Recovery)]
    [InlineData(SceneArchetypeType.ScholarlyPursuit, ArchetypeIntensity.Recovery)]
    [InlineData(SceneArchetypeType.InvestigateLocation, ArchetypeIntensity.Standard)]
    [InlineData(SceneArchetypeType.GatherTestimony, ArchetypeIntensity.Standard)]
    [InlineData(SceneArchetypeType.SeekAudience, ArchetypeIntensity.Standard)]
    [InlineData(SceneArchetypeType.DiscoverArtifact, ArchetypeIntensity.Standard)]
    [InlineData(SceneArchetypeType.UncoverConspiracy, ArchetypeIntensity.Standard)]
    [InlineData(SceneArchetypeType.MeetOrderMember, ArchetypeIntensity.Standard)]
    [InlineData(SceneArchetypeType.ConfrontAntagonist, ArchetypeIntensity.Demanding)]
    [InlineData(SceneArchetypeType.UrgentDecision, ArchetypeIntensity.Demanding)]
    [InlineData(SceneArchetypeType.MoralCrossroads, ArchetypeIntensity.Demanding)]
    public void MapArchetypeToIntensity_ReturnsCorrectIntensity(
        SceneArchetypeType archetype, ArchetypeIntensity expectedIntensity)
    {
        // Act
        ArchetypeIntensity result = ArchetypeCategorySelector.MapArchetypeToIntensity(archetype);

        // Assert
        Assert.Equal(expectedIntensity, result);
    }

    [Theory]
    [InlineData(SceneArchetypeType.InnLodging, ArchetypeIntensity.Standard)]
    [InlineData(SceneArchetypeType.ConsequenceReflection, ArchetypeIntensity.Standard)]
    [InlineData(SceneArchetypeType.DeliveryContract, ArchetypeIntensity.Standard)]
    [InlineData(SceneArchetypeType.RouteSegmentTravel, ArchetypeIntensity.Standard)]
    public void MapArchetypeToIntensity_ServicePatterns_ReturnStandardIntensity(
        SceneArchetypeType archetype, ArchetypeIntensity expectedIntensity)
    {
        // Service patterns should NOT be tracked for A-story rhythm
        // If they reach intensity recording, they default to Standard intensity
        ArchetypeIntensity result = ArchetypeCategorySelector.MapArchetypeToIntensity(archetype);
        Assert.Equal(expectedIntensity, result);
    }

    // ==================== FAIL-FAST COMPLETENESS TESTS ====================

    [Fact]
    public void MapArchetypeToCategory_AllEnumValues_AreExplicitlyHandled()
    {
        // FAIL-FAST VERIFICATION: All SceneArchetypeType values must be explicitly mapped
        // This test ensures no enum values fall through to the throw statement
        foreach (SceneArchetypeType archetype in Enum.GetValues(typeof(SceneArchetypeType)))
        {
            // Should not throw - all values are explicitly handled
            string category = ArchetypeCategorySelector.MapArchetypeToCategory(archetype);
            Assert.NotNull(category);
            Assert.NotEmpty(category);
        }
    }

    [Fact]
    public void MapArchetypeToIntensity_AllEnumValues_AreExplicitlyHandled()
    {
        // FAIL-FAST VERIFICATION: All SceneArchetypeType values must be explicitly mapped
        // This test ensures no enum values fall through to the throw statement
        foreach (SceneArchetypeType archetype in Enum.GetValues(typeof(SceneArchetypeType)))
        {
            // Should not throw - all values are explicitly handled
            ArchetypeIntensity intensity = ArchetypeCategorySelector.MapArchetypeToIntensity(archetype);
            Assert.True(Enum.IsDefined(typeof(ArchetypeIntensity), intensity));
        }
    }
}

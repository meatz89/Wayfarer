using Xunit;

namespace Wayfarer.Tests.Content;

/// <summary>
/// Tests for SituationArchetypeCatalog - fail-fast validation, peaceful archetypes
///
/// REGRESSION TESTS: Prevent gaps in peaceful archetype generation from recurring
/// See ProceduralAStoryServiceTests for player readiness and category resolution tests
/// </summary>
public class SituationArchetypeCatalogTests
{
    // ==================== PEACEFUL SITUATION ARCHETYPES ====================

    [Fact]
    public void GetArchetype_MeditationAndReflection_ReturnsValidArchetype()
    {
        // CRITICAL: Peaceful archetype must exist for exhausted player recovery
        SituationArchetype archetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.MeditationAndReflection);

        Assert.NotNull(archetype);
        Assert.Equal(SituationArchetypeType.MeditationAndReflection, archetype.Type);
        Assert.Equal(ArchetypeIntensity.Peaceful, archetype.Intensity);
    }

    [Fact]
    public void GetArchetype_LocalConversation_ReturnsValidArchetype()
    {
        // CRITICAL: Peaceful archetype must exist for exhausted player recovery
        SituationArchetype archetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.LocalConversation);

        Assert.NotNull(archetype);
        Assert.Equal(SituationArchetypeType.LocalConversation, archetype.Type);
        Assert.Equal(ArchetypeIntensity.Peaceful, archetype.Intensity);
    }

    [Fact]
    public void GetArchetype_StudyInLibrary_ReturnsValidArchetype()
    {
        // CRITICAL: Peaceful archetype must exist for exhausted player recovery
        SituationArchetype archetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.StudyInLibrary);

        Assert.NotNull(archetype);
        Assert.Equal(SituationArchetypeType.StudyInLibrary, archetype.Type);
        Assert.Equal(ArchetypeIntensity.Peaceful, archetype.Intensity);
    }

    [Fact]
    public void AllPeacefulArchetypes_HaveValidPrimaryStats()
    {
        // CRITICAL: Peaceful archetypes must have valid primary stats for stat grant choices
        // CreateStatGrantConsequence will throw for None stat
        List<SituationArchetypeType> peacefulTypes = new List<SituationArchetypeType>
        {
            SituationArchetypeType.MeditationAndReflection,
            SituationArchetypeType.LocalConversation,
            SituationArchetypeType.StudyInLibrary
        };

        foreach (SituationArchetypeType type in peacefulTypes)
        {
            SituationArchetype archetype = SituationArchetypeCatalog.GetArchetype(type);

            // Primary stat must be a valid stat (not None)
            Assert.True(
                archetype.PrimaryStat != PlayerStatType.None,
                $"Peaceful archetype {type} must have valid PrimaryStat, got None");
        }
    }

    [Fact]
    public void AllPeacefulArchetypes_HaveValidSecondaryStats()
    {
        // CRITICAL: Peaceful archetypes must have valid secondary stats for stat grant choices
        // CreateStatGrantConsequence will throw for None stat
        List<SituationArchetypeType> peacefulTypes = new List<SituationArchetypeType>
        {
            SituationArchetypeType.MeditationAndReflection,
            SituationArchetypeType.LocalConversation,
            SituationArchetypeType.StudyInLibrary
        };

        foreach (SituationArchetypeType type in peacefulTypes)
        {
            SituationArchetype archetype = SituationArchetypeCatalog.GetArchetype(type);

            // Secondary stat must be a valid stat (not None)
            Assert.True(
                archetype.SecondaryStat != PlayerStatType.None,
                $"Peaceful archetype {type} must have valid SecondaryStat, got None");
        }
    }

    // ==================== CHOICE GENERATION ====================

    [Fact]
    public void GenerateChoiceTemplatesWithContext_PeacefulArchetype_ProducesChoices()
    {
        // CRITICAL: Building rhythm choice generation must work for peaceful archetypes
        SituationArchetype archetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.MeditationAndReflection);
        GenerationContext context = new GenerationContext
        {
            RhythmPattern = "Building",
            NPCDemeanor = NPCDemeanor.Friendly,
            EnvironmentQuality = EnvironmentQuality.Standard,
            Tier = 1
        };

        List<ChoiceTemplate> choices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            archetype,
            "test_peaceful",
            context);

        Assert.NotEmpty(choices);
        Assert.True(choices.Count >= 2, "Building rhythm should generate at least 2 choices for peaceful archetype");
    }

    [Theory]
    [InlineData(SituationArchetypeType.MeditationAndReflection)]
    [InlineData(SituationArchetypeType.LocalConversation)]
    [InlineData(SituationArchetypeType.StudyInLibrary)]
    public void GenerateChoiceTemplatesWithContext_AllPeacefulArchetypes_WorkWithBuildingRhythm(SituationArchetypeType type)
    {
        // CRITICAL: All 3 peaceful archetypes must work with Building rhythm
        SituationArchetype archetype = SituationArchetypeCatalog.GetArchetype(type);
        GenerationContext context = new GenerationContext
        {
            RhythmPattern = "Building",
            NPCDemeanor = NPCDemeanor.Friendly,
            EnvironmentQuality = EnvironmentQuality.Standard,
            Tier = 1
        };

        // This should not throw - if it does, the archetype has invalid stats
        List<ChoiceTemplate> choices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            archetype,
            $"test_{type}",
            context);

        Assert.NotEmpty(choices);
    }
}

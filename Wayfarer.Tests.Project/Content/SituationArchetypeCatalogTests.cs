using Xunit;

namespace Wayfarer.Tests.Content;

/// <summary>
/// Tests for SituationArchetypeCatalog - fail-fast validation, peaceful archetypes
///
/// REGRESSION TESTS: Prevent gaps in peaceful archetype generation from recurring
/// Peaceful archetypes appear every 8th sequence as earned structural respite
/// </summary>
public class SituationArchetypeCatalogTests
{
    // ==================== PEACEFUL SITUATION ARCHETYPES ====================
    // Used in 8-cycle rotation every 8th sequence for earned respite

    [Fact]
    public void GetArchetype_MeditationAndReflection_ReturnsValidArchetype()
    {
        // CRITICAL: Peaceful archetype must exist for 8-cycle rotation
        SituationArchetype archetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.MeditationAndReflection);

        Assert.NotNull(archetype);
        Assert.Equal(SituationArchetypeType.MeditationAndReflection, archetype.Type);
        Assert.Equal(ArchetypeIntensity.Recovery, archetype.Intensity);
    }

    [Fact]
    public void GetArchetype_LocalConversation_ReturnsValidArchetype()
    {
        // CRITICAL: Peaceful archetype must exist for 8-cycle rotation
        SituationArchetype archetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.LocalConversation);

        Assert.NotNull(archetype);
        Assert.Equal(SituationArchetypeType.LocalConversation, archetype.Type);
        Assert.Equal(ArchetypeIntensity.Recovery, archetype.Intensity);
    }

    [Fact]
    public void GetArchetype_StudyInLibrary_ReturnsValidArchetype()
    {
        // CRITICAL: Peaceful archetype must exist for 8-cycle rotation
        SituationArchetype archetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.StudyInLibrary);

        Assert.NotNull(archetype);
        Assert.Equal(SituationArchetypeType.StudyInLibrary, archetype.Type);
        Assert.Equal(ArchetypeIntensity.Recovery, archetype.Intensity);
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
            Rhythm = RhythmPattern.Building,
            NpcDemeanor = NPCDemeanor.Friendly,
            Environment = EnvironmentQuality.Standard
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
            Rhythm = RhythmPattern.Building,
            NpcDemeanor = NPCDemeanor.Friendly,
            Environment = EnvironmentQuality.Standard
        };

        // This should not throw - if it does, the archetype has invalid stats
        List<ChoiceTemplate> choices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            archetype,
            $"test_{type}",
            context);

        Assert.NotEmpty(choices);
    }

    // ==================== INCOME ARCHETYPES ====================
    // CRITICAL: ContractNegotiation earns coins (income), ServiceNegotiation costs coins (expense)

    [Fact]
    public void ContractNegotiation_HasPositiveCoinReward()
    {
        // CRITICAL: ContractNegotiation is an INCOME archetype - CoinReward must be positive
        SituationArchetype archetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.ContractNegotiation);

        Assert.True(archetype.CoinReward > 0,
            $"ContractNegotiation must have positive CoinReward (income archetype), got {archetype.CoinReward}");
        Assert.Equal(0, archetype.CoinCost); // Income archetypes don't cost coins
    }

    [Fact]
    public void ServiceNegotiation_HasPositiveCoinCost()
    {
        // CRITICAL: ServiceNegotiation is an EXPENSE archetype - CoinCost must be positive
        SituationArchetype archetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.ServiceNegotiation);

        Assert.True(archetype.CoinCost > 0,
            $"ServiceNegotiation must have positive CoinCost (expense archetype), got {archetype.CoinCost}");
        Assert.Equal(0, archetype.CoinReward); // Expense archetypes don't earn coins
    }

    [Fact]
    public void ContractNegotiation_GeneratesPositiveCoinConsequences()
    {
        // CRITICAL: ContractNegotiation choices must have POSITIVE coin consequences (player EARNS)
        SituationArchetype archetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.ContractNegotiation);
        GenerationContext context = new GenerationContext
        {
            Rhythm = RhythmPattern.Mixed, // DeliveryContract A2 uses Mixed rhythm
            NpcDemeanor = NPCDemeanor.Neutral,
            Quality = Quality.Standard,
            LocationDifficulty = 0
        };

        List<ChoiceTemplate> choices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            archetype,
            "test_contract",
            context);

        // Find the money choice (Choice 2)
        ChoiceTemplate moneyChoice = choices.FirstOrDefault(c => c.Id.Contains("money"));
        Assert.NotNull(moneyChoice);

        // CRITICAL: Money choice consequence must have POSITIVE coins (income, not expense)
        Assert.True(moneyChoice.Consequence.Coins > 0,
            $"ContractNegotiation money choice must have positive coin consequence (income), got {moneyChoice.Consequence.Coins}");
    }

    [Fact]
    public void ServiceNegotiation_GeneratesNegativeCoinConsequences()
    {
        // CRITICAL: ServiceNegotiation choices must have NEGATIVE coin consequences (player SPENDS)
        SituationArchetype archetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.ServiceNegotiation);
        GenerationContext context = new GenerationContext
        {
            Rhythm = RhythmPattern.Mixed,
            NpcDemeanor = NPCDemeanor.Neutral,
            Quality = Quality.Standard,
            LocationDifficulty = 0
        };

        List<ChoiceTemplate> choices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            archetype,
            "test_service",
            context);

        // Find the money choice (Choice 2)
        ChoiceTemplate moneyChoice = choices.FirstOrDefault(c => c.Id.Contains("money"));
        Assert.NotNull(moneyChoice);

        // CRITICAL: Money choice consequence must have NEGATIVE coins (expense, not income)
        Assert.True(moneyChoice.Consequence.Coins < 0,
            $"ServiceNegotiation money choice must have negative coin consequence (expense), got {moneyChoice.Consequence.Coins}");
    }
}

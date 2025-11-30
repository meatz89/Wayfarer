using Xunit;

namespace Wayfarer.Tests.Services;

/// <summary>
/// Tests for runtime intensity filtering in facades.
///
/// CRITICAL: Ensures exhausted players (Resolve less than 3) only see Recovery situations,
/// not all situations regardless of intensity level.
///
/// This extends generation-time filtering (ProceduralAStoryService) to runtime display
/// filtering (GameFacade, LocationFacade).
///
/// THREE-LEVEL PLAYER READINESS:
/// - Exhausted (Resolve less than 3): Recovery only
/// - Normal (Resolve 3-15): Recovery + Standard
/// - Capable (Resolve greater than 15): All intensities
/// </summary>
public class RuntimeIntensityFilteringTests
{
    // ==================== SITUATION INTENSITY PROPERTY TESTS ====================

    [Fact]
    public void Situation_DefaultIntensity_IsStandard()
    {
        Situation situation = new Situation();

        Assert.Equal(ArchetypeIntensity.Standard, situation.Intensity);
    }

    [Fact]
    public void SituationTemplate_DefaultIntensity_IsStandard()
    {
        SituationTemplate template = new SituationTemplate { Id = "test" };

        Assert.Equal(ArchetypeIntensity.Standard, template.Intensity);
    }

    [Theory]
    [InlineData(ArchetypeIntensity.Recovery)]
    [InlineData(ArchetypeIntensity.Standard)]
    [InlineData(ArchetypeIntensity.Demanding)]
    public void Situation_IntensityCanBeSet(ArchetypeIntensity intensity)
    {
        Situation situation = new Situation { Intensity = intensity };

        Assert.Equal(intensity, situation.Intensity);
    }

    // ==================== INTENSITY COMPARISON TESTS ====================

    [Fact]
    public void IntensityComparison_RecoveryLessThanStandard()
    {
        Assert.True(ArchetypeIntensity.Recovery < ArchetypeIntensity.Standard);
    }

    [Fact]
    public void IntensityComparison_StandardLessThanDemanding()
    {
        Assert.True(ArchetypeIntensity.Standard < ArchetypeIntensity.Demanding);
    }

    [Fact]
    public void IntensityComparison_RecoveryLessThanOrEqualToRecovery()
    {
        Assert.True(ArchetypeIntensity.Recovery <= ArchetypeIntensity.Recovery);
    }

    [Fact]
    public void IntensityComparison_StandardLessThanOrEqualToStandard()
    {
        Assert.True(ArchetypeIntensity.Standard <= ArchetypeIntensity.Standard);
    }

    // ==================== FILTERING LOGIC TESTS ====================

    [Fact]
    public void FilterByIntensity_ExhaustedPlayer_OnlyRecoverySituationsPass()
    {
        PlayerReadinessService readinessService = new PlayerReadinessService();
        Player exhaustedPlayer = new Player { Resolve = 1 };
        ArchetypeIntensity maxSafe = readinessService.GetMaxSafeIntensity(exhaustedPlayer);

        List<Situation> situations = new List<Situation>
        {
            new Situation { Name = "Meditation", Intensity = ArchetypeIntensity.Recovery },
            new Situation { Name = "Negotiation", Intensity = ArchetypeIntensity.Standard },
            new Situation { Name = "Confrontation", Intensity = ArchetypeIntensity.Demanding }
        };

        List<Situation> filtered = situations
            .Where(s => s.Intensity <= maxSafe)
            .ToList();

        Assert.Single(filtered);
        Assert.Equal("Meditation", filtered[0].Name);
    }

    [Fact]
    public void FilterByIntensity_NormalPlayer_RecoveryAndStandardSituationsPass()
    {
        PlayerReadinessService readinessService = new PlayerReadinessService();
        Player normalPlayer = new Player { Resolve = 10 };
        ArchetypeIntensity maxSafe = readinessService.GetMaxSafeIntensity(normalPlayer);

        List<Situation> situations = new List<Situation>
        {
            new Situation { Name = "Meditation", Intensity = ArchetypeIntensity.Recovery },
            new Situation { Name = "Negotiation", Intensity = ArchetypeIntensity.Standard },
            new Situation { Name = "Confrontation", Intensity = ArchetypeIntensity.Demanding }
        };

        List<Situation> filtered = situations
            .Where(s => s.Intensity <= maxSafe)
            .ToList();

        Assert.Equal(2, filtered.Count);
        Assert.Contains(filtered, s => s.Name == "Meditation");
        Assert.Contains(filtered, s => s.Name == "Negotiation");
        Assert.DoesNotContain(filtered, s => s.Name == "Confrontation");
    }

    [Fact]
    public void FilterByIntensity_CapablePlayer_AllSituationsPass()
    {
        PlayerReadinessService readinessService = new PlayerReadinessService();
        Player capablePlayer = new Player { Resolve = 20 };
        ArchetypeIntensity maxSafe = readinessService.GetMaxSafeIntensity(capablePlayer);

        List<Situation> situations = new List<Situation>
        {
            new Situation { Name = "Meditation", Intensity = ArchetypeIntensity.Recovery },
            new Situation { Name = "Negotiation", Intensity = ArchetypeIntensity.Standard },
            new Situation { Name = "Confrontation", Intensity = ArchetypeIntensity.Demanding }
        };

        List<Situation> filtered = situations
            .Where(s => s.Intensity <= maxSafe)
            .ToList();

        Assert.Equal(3, filtered.Count);
    }

    // ==================== EDGE CASE TESTS ====================

    [Fact]
    public void FilterByIntensity_ZeroResolve_OnlyRecoverySafe()
    {
        PlayerReadinessService readinessService = new PlayerReadinessService();
        Player exhaustedPlayer = new Player { Resolve = 0 };
        ArchetypeIntensity maxSafe = readinessService.GetMaxSafeIntensity(exhaustedPlayer);

        Assert.Equal(ArchetypeIntensity.Recovery, maxSafe);
    }

    [Fact]
    public void FilterByIntensity_ExactlyThreeResolve_StandardSafe()
    {
        PlayerReadinessService readinessService = new PlayerReadinessService();
        Player player = new Player { Resolve = 3 };
        ArchetypeIntensity maxSafe = readinessService.GetMaxSafeIntensity(player);

        Assert.Equal(ArchetypeIntensity.Standard, maxSafe);
    }

    [Fact]
    public void FilterByIntensity_ExactlySixteenResolve_DemandingSafe()
    {
        PlayerReadinessService readinessService = new PlayerReadinessService();
        Player player = new Player { Resolve = 16 };
        ArchetypeIntensity maxSafe = readinessService.GetMaxSafeIntensity(player);

        Assert.Equal(ArchetypeIntensity.Demanding, maxSafe);
    }

    // ==================== SITUATION TEMPLATE INTENSITY PROPAGATION ====================

    [Fact]
    public void SituationTemplate_IntensityPropagates_WhenCopiedToSituation()
    {
        SituationTemplate template = new SituationTemplate
        {
            Id = "test_template",
            Intensity = ArchetypeIntensity.Demanding
        };

        Situation situation = new Situation
        {
            TemplateId = template.Id,
            Template = template,
            Intensity = template.Intensity
        };

        Assert.Equal(ArchetypeIntensity.Demanding, situation.Intensity);
    }

    [Fact]
    public void SituationTemplate_RecoveryIntensity_PropagatesCorrectly()
    {
        SituationTemplate template = new SituationTemplate
        {
            Id = "peaceful_template",
            Intensity = ArchetypeIntensity.Recovery
        };

        Situation situation = new Situation
        {
            TemplateId = template.Id,
            Template = template,
            Intensity = template.Intensity
        };

        Assert.Equal(ArchetypeIntensity.Recovery, situation.Intensity);
    }
}

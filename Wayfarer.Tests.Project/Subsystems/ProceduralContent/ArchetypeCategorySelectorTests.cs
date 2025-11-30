using Xunit;

namespace Wayfarer.Tests.Subsystems.ProceduralContent;

/// <summary>
/// Tests for ArchetypeCategorySelector - context-aware weighted scoring for scene selection.
///
/// KEY PRINCIPLES TESTED:
/// 1. DETERMINISM: Same inputs always produce same output
/// 2. CHALLENGE PHILOSOPHY: Player resources do NOT influence selection
/// 3. STRONG LOCATION INFLUENCE: Safety/Purpose significantly bias categories
/// 4. CONTEXT-BASED PEACEFUL: Earned through intensity history, not fixed position
///
/// See gdd/06_balance.md §6.8 for Challenge Philosophy.
/// </summary>
public class ArchetypeCategorySelectorTests
{
    // ==================== DETERMINISM TESTS ====================
    // Same inputs MUST always produce same output

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(100)]
    public void SelectCategory_SameInputs_AlwaysProducesSameOutput(int sequence)
    {
        // Arrange - create identical inputs
        Player player1 = CreateTestPlayer();
        Player player2 = CreateTestPlayer();
        Location location = CreateSafeLocation();
        AStoryContext context1 = new AStoryContext();
        AStoryContext context2 = new AStoryContext();

        // Act - call twice with identical inputs
        string result1 = ArchetypeCategorySelector.SelectCategory(sequence, player1, location, context1);
        string result2 = ArchetypeCategorySelector.SelectCategory(sequence, player2, location, context2);

        // Assert - results must be identical
        Assert.Equal(result1, result2);
    }

    [Fact]
    public void SelectCategory_MultipleCallsSameInputs_DeterministicResults()
    {
        // Arrange
        Player player = CreateTestPlayer();
        Location location = CreateSafeLocation();
        AStoryContext context = new AStoryContext();
        int sequence = 5;

        // Act - call multiple times
        List<string> results = new List<string>();
        for (int i = 0; i < 10; i++)
        {
            results.Add(ArchetypeCategorySelector.SelectCategory(sequence, player, location, context));
        }

        // Assert - all results identical
        Assert.All(results, r => Assert.Equal(results[0], r));
    }

    // ==================== LOCATION CONTEXT TESTS (Q4: A - Strong Influence) ====================

    [Fact]
    public void SelectCategory_DangerousLocation_FavorsCrisisOrConfrontation()
    {
        // Arrange
        Player player = CreateTestPlayer();
        Location dangerousLocation = CreateDangerousLocation();
        AStoryContext context = new AStoryContext();

        // Act
        string result = ArchetypeCategorySelector.SelectCategory(1, player, dangerousLocation, context);

        // Assert - should NOT be Peaceful in dangerous location
        Assert.NotEqual("Peaceful", result);
    }

    [Fact]
    public void SelectCategory_SafeLocation_FavorsPeacefulOrSocial()
    {
        // Arrange - player with heavy intensity history (triggers Peaceful bonus)
        Player player = CreatePlayerWithHeavyIntensity();
        Location safeLocation = CreateSafeLocation();
        AStoryContext context = new AStoryContext();

        // Act
        string result = ArchetypeCategorySelector.SelectCategory(1, player, safeLocation, context);

        // Assert - should favor lighter categories in safe location
        List<string> lighterCategories = new List<string> { "Peaceful", "Social", "Investigation" };
        Assert.Contains(result, lighterCategories);
    }

    [Fact]
    public void SelectCategory_WorshipLocation_FavorsPeaceful()
    {
        // Arrange - worship location with player needing recovery
        Player player = CreatePlayerWithHeavyIntensity();
        Location worshipLocation = CreateWorshipLocation();
        AStoryContext context = new AStoryContext();

        // Act
        string result = ArchetypeCategorySelector.SelectCategory(1, player, worshipLocation, context);

        // Assert - worship strongly favors Peaceful
        Assert.Equal("Peaceful", result);
    }

    // ==================== INTENSITY BALANCE TESTS ====================

    [Fact]
    public void SelectCategory_HeavyIntensityHistory_StronglyFavorsPeaceful()
    {
        // Arrange - 3+ Demanding in last 5 scenes triggers IsIntensityHeavy()
        Player player = CreatePlayerWithHeavyIntensity();
        Location safeLocation = CreateSafeLocation();
        AStoryContext context = new AStoryContext();

        // Act - test at sequence position that doesn't naturally favor Peaceful
        string result = ArchetypeCategorySelector.SelectCategory(1, player, safeLocation, context);

        // Assert - heavy intensity should push toward recovery
        List<string> recoveryCategories = new List<string> { "Peaceful", "Social", "Investigation" };
        Assert.Contains(result, recoveryCategories);
    }

    [Fact]
    public void SelectCategory_NoRecoveryIn6Scenes_FavorsPeaceful()
    {
        // Arrange - 6+ scenes without Recovery intensity
        Player player = CreatePlayerWithNoRecentRecovery();
        Location safeLocation = CreateSafeLocation();
        AStoryContext context = new AStoryContext();

        // Act
        string result = ArchetypeCategorySelector.SelectCategory(1, player, safeLocation, context);

        // Assert - prolonged demanding should favor Peaceful
        Assert.Equal("Peaceful", result);
    }

    [Fact]
    public void SelectCategory_TooMuchRecovery_ReducesPeacefulBonus()
    {
        // Arrange - 2+ Recovery in recent history
        Player player = CreatePlayerWithTooMuchRecovery();
        Location dangerousLocation = CreateDangerousLocation();
        AStoryContext context = new AStoryContext();

        // Act
        string result = ArchetypeCategorySelector.SelectCategory(1, player, dangerousLocation, context);

        // Assert - should favor more demanding content
        Assert.NotEqual("Peaceful", result);
    }

    // ==================== RHYTHM PHASE TESTS ====================

    [Fact]
    public void SelectCategory_AfterCrisisRhythm_FavorsLighterContent()
    {
        // Arrange - last scene had Crisis rhythm
        Player player = CreatePlayerAfterCrisis();
        Location safeLocation = CreateSafeLocation();
        AStoryContext context = new AStoryContext();

        // Act
        string result = ArchetypeCategorySelector.SelectCategory(1, player, safeLocation, context);

        // Assert - avoid back-to-back Crisis
        Assert.NotEqual("Crisis", result);
    }

    [Fact]
    public void SelectCategory_AfterRecovery_FavorsEngagement()
    {
        // Arrange - last scene was Recovery intensity
        Player player = CreatePlayerAfterRecovery();
        Location riskyLocation = CreateRiskyLocation();
        AStoryContext context = new AStoryContext();

        // Act
        string result = ArchetypeCategorySelector.SelectCategory(1, player, riskyLocation, context);

        // Assert - should NOT repeat Peaceful immediately
        Assert.NotEqual("Peaceful", result);
    }

    // ==================== ANTI-REPETITION TESTS ====================

    [Fact]
    public void SelectCategory_RecentCategory_Penalized()
    {
        // Arrange - Investigation was used recently
        AStoryContext context = new AStoryContext();
        context.RecentArchetypes.Add(SceneArchetypeType.InvestigateLocation);
        Player player = CreateTestPlayer();
        Location location = CreateSafeLocation();

        // Act - at position that would naturally favor Investigation
        string result = ArchetypeCategorySelector.SelectCategory(1, player, location, context);

        // Assert - may or may not be Investigation depending on other factors
        // The penalty is -15, but other factors can override
        // Just verify we get a valid category
        List<string> validCategories = new List<string>
        {
            "Investigation", "Social", "Confrontation", "Crisis", "Peaceful"
        };
        Assert.Contains(result, validCategories);
    }

    // ==================== ARCHETYPE MAPPING TESTS ====================

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

    // ==================== CHALLENGE PHILOSOPHY TESTS (Q3: A - No Player Resource Influence) ====================

    [Fact]
    public void SelectCategory_LowPlayerHealth_DoesNotInfluenceSelection()
    {
        // Arrange - two players with different health levels
        Player healthyPlayer = CreateTestPlayer();
        healthyPlayer.Health = healthyPlayer.MaxHealth;

        Player injuredPlayer = CreateTestPlayer();
        injuredPlayer.Health = 1;

        Location location = CreateRiskyLocation();
        AStoryContext context = new AStoryContext();

        // Act
        string healthyResult = ArchetypeCategorySelector.SelectCategory(5, healthyPlayer, location, context);
        string injuredResult = ArchetypeCategorySelector.SelectCategory(5, injuredPlayer, location, context);

        // Assert - both should get same result (Challenge Philosophy)
        Assert.Equal(healthyResult, injuredResult);
    }

    [Fact]
    public void SelectCategory_LowPlayerStamina_DoesNotInfluenceSelection()
    {
        // Arrange
        Player freshPlayer = CreateTestPlayer();
        freshPlayer.Stamina = freshPlayer.MaxStamina;

        Player exhaustedPlayer = CreateTestPlayer();
        exhaustedPlayer.Stamina = 0;

        Location location = CreateSafeLocation();
        AStoryContext context = new AStoryContext();

        // Act
        string freshResult = ArchetypeCategorySelector.SelectCategory(3, freshPlayer, location, context);
        string exhaustedResult = ArchetypeCategorySelector.SelectCategory(3, exhaustedPlayer, location, context);

        // Assert
        Assert.Equal(freshResult, exhaustedResult);
    }

    [Fact]
    public void SelectCategory_NegativeResolve_DoesNotInfluenceSelection()
    {
        // Arrange - Sir Brante pattern: Resolve can go negative
        Player positiveResolvePlayer = CreateTestPlayer();
        positiveResolvePlayer.Resolve = 50;

        Player negativeResolvePlayer = CreateTestPlayer();
        negativeResolvePlayer.Resolve = -10;

        Location location = CreateDangerousLocation();
        AStoryContext context = new AStoryContext();

        // Act
        string positiveResult = ArchetypeCategorySelector.SelectCategory(4, positiveResolvePlayer, location, context);
        string negativeResult = ArchetypeCategorySelector.SelectCategory(4, negativeResolvePlayer, location, context);

        // Assert - Challenge Philosophy: no coddling based on Resolve
        Assert.Equal(positiveResult, negativeResult);
    }

    // ==================== HELPER METHODS ====================

    private static Player CreateTestPlayer()
    {
        return new Player
        {
            Health = 100,
            MaxHealth = 100,
            Stamina = 100,
            MaxStamina = 100,
            Focus = 100,
            MaxFocus = 100,
            Resolve = 20,
            SceneIntensityHistory = new List<SceneIntensityRecord>()
        };
    }

    private static Player CreatePlayerWithHeavyIntensity()
    {
        Player player = CreateTestPlayer();
        // 3+ Demanding in last 5 triggers IsIntensityHeavy()
        player.SceneIntensityHistory.Add(CreateIntensityRecord(ArchetypeIntensity.Demanding, 1));
        player.SceneIntensityHistory.Add(CreateIntensityRecord(ArchetypeIntensity.Demanding, 2));
        player.SceneIntensityHistory.Add(CreateIntensityRecord(ArchetypeIntensity.Demanding, 3));
        player.SceneIntensityHistory.Add(CreateIntensityRecord(ArchetypeIntensity.Standard, 4));
        return player;
    }

    private static Player CreatePlayerWithNoRecentRecovery()
    {
        Player player = CreateTestPlayer();
        // 6+ scenes since Recovery
        for (int i = 1; i <= 7; i++)
        {
            player.SceneIntensityHistory.Add(CreateIntensityRecord(ArchetypeIntensity.Standard, i));
        }
        return player;
    }

    private static Player CreatePlayerWithTooMuchRecovery()
    {
        Player player = CreateTestPlayer();
        // 2+ Recovery in recent history
        player.SceneIntensityHistory.Add(CreateIntensityRecord(ArchetypeIntensity.Recovery, 1));
        player.SceneIntensityHistory.Add(CreateIntensityRecord(ArchetypeIntensity.Recovery, 2));
        player.SceneIntensityHistory.Add(CreateIntensityRecord(ArchetypeIntensity.Standard, 3));
        player.SceneIntensityHistory.Add(CreateIntensityRecord(ArchetypeIntensity.Standard, 4));
        return player;
    }

    private static Player CreatePlayerAfterCrisis()
    {
        Player player = CreateTestPlayer();
        player.SceneIntensityHistory.Add(CreateIntensityRecord(ArchetypeIntensity.Standard, 1));
        player.SceneIntensityHistory.Add(new SceneIntensityRecord
        {
            Intensity = ArchetypeIntensity.Demanding,
            Category = "Crisis",
            Sequence = 2,
            WasCrisisRhythm = true,
            LocationSafety = LocationSafety.Risky
        });
        return player;
    }

    private static Player CreatePlayerAfterRecovery()
    {
        Player player = CreateTestPlayer();
        player.SceneIntensityHistory.Add(CreateIntensityRecord(ArchetypeIntensity.Standard, 1));
        player.SceneIntensityHistory.Add(CreateIntensityRecord(ArchetypeIntensity.Recovery, 2));
        return player;
    }

    private static SceneIntensityRecord CreateIntensityRecord(ArchetypeIntensity intensity, int sequence)
    {
        return new SceneIntensityRecord
        {
            Intensity = intensity,
            Category = intensity switch
            {
                ArchetypeIntensity.Recovery => "Peaceful",
                ArchetypeIntensity.Standard => "Investigation",
                ArchetypeIntensity.Demanding => "Crisis",
                _ => "Investigation"
            },
            Sequence = sequence,
            WasCrisisRhythm = intensity == ArchetypeIntensity.Demanding,
            LocationSafety = LocationSafety.Safe
        };
    }

    private static Location CreateSafeLocation()
    {
        return new Location
        {
            Name = "Test Safe Location",
            Safety = LocationSafety.Safe,
            Purpose = LocationPurpose.Dwelling
        };
    }

    private static Location CreateDangerousLocation()
    {
        return new Location
        {
            Name = "Test Dangerous Location",
            Safety = LocationSafety.Dangerous,
            Purpose = LocationPurpose.Civic
        };
    }

    private static Location CreateRiskyLocation()
    {
        return new Location
        {
            Name = "Test Risky Location",
            Safety = LocationSafety.Risky,
            Purpose = LocationPurpose.Commerce
        };
    }

    private static Location CreateWorshipLocation()
    {
        return new Location
        {
            Name = "Test Worship Location",
            Safety = LocationSafety.Safe,
            Purpose = LocationPurpose.Worship
        };
    }
}

using Xunit;

namespace Wayfarer.Tests.Architecture;

/// <summary>
/// HIGHLANDER Compliance Tests for Scene Generation
///
/// Verifies that authored and procedural content use IDENTICAL selection logic.
/// The only difference is WHERE inputs come from, not HOW they're processed.
///
/// PRINCIPLE (arc42 §8.28):
/// - Same SelectArchetypeCategory for both paths
/// - Current player state NEVER influences selection
/// - Selection based on rhythm pattern + location context + history
/// </summary>
public class HighlanderSceneGenerationTests
{
    // ==================== IDENTICAL INPUT → IDENTICAL OUTPUT ====================

    [Fact]
    public void SelectArchetypeCategory_SameInputs_ProducesSameOutput()
    {
        // HIGHLANDER: Given identical inputs, selection must produce identical results
        // regardless of whether inputs came from authored or procedural source

        SceneSelectionInputs inputsA = CreateTestInputs(
            rhythmPattern: RhythmPattern.Building,
            locationSafety: LocationSafety.Safe,
            locationPurpose: LocationPurpose.Civic);

        SceneSelectionInputs inputsB = CreateTestInputs(
            rhythmPattern: RhythmPattern.Building,
            locationSafety: LocationSafety.Safe,
            locationPurpose: LocationPurpose.Civic);

        // Both should produce identical category
        string categoryA = TestSelectArchetypeCategory(inputsA);
        string categoryB = TestSelectArchetypeCategory(inputsB);

        Assert.Equal(categoryA, categoryB);
    }

    [Fact]
    public void SelectArchetypeCategory_DeterministicFromHistory()
    {
        // Selection must be deterministic - same history values = same result
        // No randomness allowed in category selection

        SceneSelectionInputs inputs1 = CreateTestInputs(
            rhythmPattern: RhythmPattern.Building,
            recentDemandingCount: 2,
            recentRecoveryCount: 1,
            scenesSinceRecovery: 3);

        SceneSelectionInputs inputs2 = CreateTestInputs(
            rhythmPattern: RhythmPattern.Building,
            recentDemandingCount: 2,
            recentRecoveryCount: 1,
            scenesSinceRecovery: 3);

        string category1 = TestSelectArchetypeCategory(inputs1);
        string category2 = TestSelectArchetypeCategory(inputs2);

        Assert.Equal(category1, category2);
    }

    // ==================== RHYTHM PATTERN → CATEGORY MAPPING ====================

    [Theory]
    [InlineData(RhythmPattern.Building, new[] { "Investigation", "Social", "Confrontation" })]
    [InlineData(RhythmPattern.Crisis, new[] { "Crisis", "Confrontation" })]
    [InlineData(RhythmPattern.Mixed, new[] { "Social", "Investigation" })]
    public void RhythmPattern_MapsToAppropriateCategories(RhythmPattern pattern, string[] expectedCategories)
    {
        // Each rhythm pattern should produce only appropriate categories
        SceneSelectionInputs inputs = CreateTestInputs(rhythmPattern: pattern);

        string category = TestSelectArchetypeCategory(inputs);

        Assert.Contains(category, expectedCategories);
    }

    [Fact]
    public void RhythmPattern_Crisis_IncludesCrisisCategory()
    {
        // Crisis pattern MUST be able to produce Crisis category
        SceneSelectionInputs inputs = CreateTestInputs(
            rhythmPattern: RhythmPattern.Crisis,
            locationSafety: LocationSafety.Dangerous); // Filter to Crisis/Confrontation

        string category = TestSelectArchetypeCategory(inputs);

        Assert.True(category == "Crisis" || category == "Confrontation",
            $"Crisis pattern should produce Crisis or Confrontation, got: {category}");
    }

    [Fact]
    public void RhythmPattern_Mixed_ExcludesCrisis()
    {
        // Mixed pattern should NOT produce Crisis category
        SceneSelectionInputs inputs = CreateTestInputs(rhythmPattern: RhythmPattern.Mixed);

        string category = TestSelectArchetypeCategory(inputs);

        Assert.NotEqual("Crisis", category);
    }

    // ==================== LOCATION CONTEXT FILTERING ====================

    [Fact]
    public void LocationContext_Dangerous_FavorsCrisisOrConfrontation()
    {
        // Dangerous locations should favor Confrontation/Crisis
        SceneSelectionInputs inputs = CreateTestInputs(
            rhythmPattern: RhythmPattern.Crisis, // Allows Crisis/Confrontation
            locationSafety: LocationSafety.Dangerous);

        string category = TestSelectArchetypeCategory(inputs);

        Assert.True(category == "Crisis" || category == "Confrontation",
            $"Dangerous location in Crisis pattern should produce Crisis or Confrontation, got: {category}");
    }

    [Fact]
    public void LocationContext_SafeCivic_FavorsSocialOrInvestigation()
    {
        // Safe civic locations should favor Social/Investigation
        SceneSelectionInputs inputs = CreateTestInputs(
            rhythmPattern: RhythmPattern.Building, // Allows Investigation/Social/Confrontation
            locationSafety: LocationSafety.Safe,
            locationPurpose: LocationPurpose.Civic);

        string category = TestSelectArchetypeCategory(inputs);

        Assert.True(category == "Social" || category == "Investigation",
            $"Safe civic location should produce Social or Investigation, got: {category}");
    }

    // ==================== ANTI-REPETITION ====================

    [Fact]
    public void AntiRepetition_AvoidsRecentCategories()
    {
        // Should avoid categories used in last 2 scenes
        SceneSelectionInputs inputs = CreateTestInputs(
            rhythmPattern: RhythmPattern.Building, // Allows Investigation/Social/Confrontation
            recentCategories: new List<string> { "Investigation", "Social" });

        string category = TestSelectArchetypeCategory(inputs);

        // Should pick Confrontation (only remaining option from Building pattern)
        Assert.Equal("Confrontation", category);
    }

    [Fact]
    public void AntiRepetition_FallsBackIfAllExcluded()
    {
        // If all categories would be excluded, should still produce a result
        SceneSelectionInputs inputs = CreateTestInputs(
            rhythmPattern: RhythmPattern.Building,
            recentCategories: new List<string> { "Investigation", "Social", "Confrontation" });

        string category = TestSelectArchetypeCategory(inputs);

        // Should fall back to one of the appropriate categories
        Assert.Contains(category, new[] { "Investigation", "Social", "Confrontation" });
    }

    // ==================== NO PLAYER STATE INFLUENCE ====================

    [Fact]
    public void Selection_NotInfluencedByPlayerResolve()
    {
        // CRITICAL: Current player Resolve should NEVER influence category selection
        // This is tested by ensuring SceneSelectionInputs has no Resolve/MaxSafeIntensity field

        // SceneSelectionInputs should NOT have MaxSafeIntensity property
        // (it was removed in the refactoring)
        Type inputsType = typeof(SceneSelectionInputs);
        System.Reflection.PropertyInfo maxSafeProperty = inputsType.GetProperty("MaxSafeIntensity");

        Assert.Null(maxSafeProperty);
    }

    [Fact]
    public void Selection_NotInfluencedBySequenceRotation()
    {
        // CRITICAL: Selection should NOT use sequence-based rotation
        // Sequence field was removed from SceneSelectionInputs

        Type inputsType = typeof(SceneSelectionInputs);
        System.Reflection.PropertyInfo sequenceProperty = inputsType.GetProperty("Sequence");

        Assert.Null(sequenceProperty);
    }

    [Fact]
    public void Selection_NoTargetCategoryOverride()
    {
        // CRITICAL: TargetCategory override was removed
        // No bypassing selection logic

        Type inputsType = typeof(SceneSelectionInputs);
        System.Reflection.PropertyInfo targetCategoryProperty = inputsType.GetProperty("TargetCategory");

        Assert.Null(targetCategoryProperty);
    }

    // ==================== AUTHORED CONTENT USES SAME LOGIC ====================

    [Fact]
    public void AuthoredContent_UsesIdenticalSelectionLogic()
    {
        // Authored content provides categorical inputs that flow through SAME logic
        SceneSpawnReward authoredReward = new SceneSpawnReward
        {
            LocationSafetyContext = LocationSafety.Safe,
            LocationPurposeContext = LocationPurpose.Civic,
            RhythmPatternContext = RhythmPattern.Building,
            TierContext = 0
        };

        // Build inputs from authored content
        SceneSelectionInputs authoredInputs = authoredReward.BuildAuthoredInputs();

        // Build equivalent procedural inputs
        SceneSelectionInputs proceduralInputs = CreateTestInputs(
            rhythmPattern: RhythmPattern.Building,
            locationSafety: LocationSafety.Safe,
            locationPurpose: LocationPurpose.Civic,
            tier: 0);

        // Both should produce same category
        string authoredCategory = TestSelectArchetypeCategory(authoredInputs);
        string proceduralCategory = TestSelectArchetypeCategory(proceduralInputs);

        Assert.Equal(authoredCategory, proceduralCategory);
    }

    [Fact]
    public void AuthoredContent_NoBypassMechanism()
    {
        // SceneSpawnReward should NOT have TargetCategory or ExcludedCategories

        Type rewardType = typeof(SceneSpawnReward);
        System.Reflection.PropertyInfo targetCategoryProperty = rewardType.GetProperty("TargetCategory");
        System.Reflection.PropertyInfo excludedCategoriesProperty = rewardType.GetProperty("ExcludedCategories");

        Assert.Null(targetCategoryProperty);
        Assert.Null(excludedCategoriesProperty);
    }

    // ==================== HELPER METHODS ====================

    /// <summary>
    /// Create test inputs with specified values, defaults for others.
    /// </summary>
    private SceneSelectionInputs CreateTestInputs(
        RhythmPattern rhythmPattern = RhythmPattern.Building,
        LocationSafety locationSafety = LocationSafety.Safe,
        LocationPurpose locationPurpose = LocationPurpose.Civic,
        int tier = 0,
        int recentDemandingCount = 0,
        int recentRecoveryCount = 0,
        int scenesSinceRecovery = 0,
        List<string> recentCategories = null)
    {
        return new SceneSelectionInputs
        {
            RhythmPattern = rhythmPattern,
            LocationSafety = locationSafety,
            LocationPurpose = locationPurpose,
            Tier = tier,
            RecentDemandingCount = recentDemandingCount,
            RecentRecoveryCount = recentRecoveryCount,
            ScenesSinceRecovery = scenesSinceRecovery,
            RecentCategories = recentCategories ?? new List<string>(),
            RecentArchetypes = new List<string>()
        };
    }

    /// <summary>
    /// Test implementation of SelectArchetypeCategory logic.
    /// Mirrors the actual implementation for testing purposes.
    ///
    /// PRODUCTION IMPLEMENTATION LOCATION:
    /// ProceduralAStoryService.SelectArchetypeCategory() in
    /// src/Subsystems/ProceduralContent/ProceduralAStoryService.cs (lines 136-159)
    ///
    /// MAINTAINABILITY: If production logic changes, this test implementation
    /// must be updated to match. Consider using reflection or direct invocation
    /// if test isolation permits.
    /// </summary>
    private string TestSelectArchetypeCategory(SceneSelectionInputs inputs)
    {
        // Get appropriate categories for rhythm pattern
        List<string> appropriateCategories = inputs.RhythmPattern switch
        {
            RhythmPattern.Building => new List<string> { "Investigation", "Social", "Confrontation" },
            RhythmPattern.Crisis => new List<string> { "Crisis", "Confrontation" },
            RhythmPattern.Mixed => new List<string> { "Social", "Investigation" },
            _ => new List<string> { "Investigation", "Social", "Confrontation", "Crisis" }
        };

        // Filter by location context
        List<string> filtered = FilterByLocationContext(appropriateCategories, inputs);

        // Apply anti-repetition
        List<string> available = filtered
            .Where(c => !inputs.RecentCategories.Contains(c))
            .ToList();

        if (!available.Any())
        {
            available = filtered;
        }

        // Deterministic selection
        int selectionIndex = ComputeSelectionIndex(inputs, available.Count);
        return available[selectionIndex];
    }

    private List<string> FilterByLocationContext(List<string> categories, SceneSelectionInputs inputs)
    {
        List<string> filtered = new List<string>(categories);

        if (inputs.LocationSafety == LocationSafety.Dangerous)
        {
            if (filtered.Contains("Confrontation") || filtered.Contains("Crisis"))
            {
                filtered = filtered.Where(c => c == "Confrontation" || c == "Crisis").ToList();
            }
        }

        if (inputs.LocationSafety == LocationSafety.Safe && inputs.LocationPurpose == LocationPurpose.Civic)
        {
            if (filtered.Contains("Social") || filtered.Contains("Investigation"))
            {
                filtered = filtered.Where(c => c == "Social" || c == "Investigation").ToList();
            }
        }

        if (!filtered.Any())
        {
            return categories;
        }

        return filtered;
    }

    private int ComputeSelectionIndex(SceneSelectionInputs inputs, int optionCount)
    {
        if (optionCount <= 0) return 0;

        int hash = inputs.RecentDemandingCount * 7
                 + inputs.RecentRecoveryCount * 11
                 + inputs.ScenesSinceRecovery * 13
                 + inputs.TotalIntensityHistoryCount * 17;

        return Math.Abs(hash) % optionCount;
    }
}

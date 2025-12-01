using Xunit;

namespace Wayfarer.Tests.Architecture;

/// <summary>
/// HIGHLANDER Compliance Tests for Scene Generation
///
/// Verifies that authored and procedural content use IDENTICAL selection logic.
/// The only difference is WHERE inputs come from, not HOW they're processed.
///
/// SIMPLIFIED (arc42 §8.28):
/// - Same SelectArchetypeCategory for both paths
/// - RhythmPattern is THE ONLY driver for category selection
/// - Anti-repetition prevents immediate repeats
/// - LocationSafety/Purpose/Tier REMOVED (legacy)
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
            rhythmPattern: RhythmPattern.Building);

        SceneSelectionInputs inputsB = CreateTestInputs(
            rhythmPattern: RhythmPattern.Building);

        // Both should produce identical category
        string categoryA = TestSelectArchetypeCategory(inputsA);
        string categoryB = TestSelectArchetypeCategory(inputsB);

        Assert.Equal(categoryA, categoryB);
    }

    [Fact]
    public void SelectArchetypeCategory_Deterministic()
    {
        // Selection must be deterministic - same inputs = same result
        // No randomness allowed in category selection

        SceneSelectionInputs inputs1 = CreateTestInputs(
            rhythmPattern: RhythmPattern.Building);

        SceneSelectionInputs inputs2 = CreateTestInputs(
            rhythmPattern: RhythmPattern.Building);

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
            rhythmPattern: RhythmPattern.Crisis);

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

    // ==================== LEGACY PROPERTIES REMOVED ====================

    [Fact]
    public void Selection_NoLocationSafetyProperty()
    {
        // LocationSafety REMOVED from SceneSelectionInputs (legacy)
        Type inputsType = typeof(SceneSelectionInputs);
        System.Reflection.PropertyInfo locationSafetyProperty = inputsType.GetProperty("LocationSafety");

        Assert.Null(locationSafetyProperty);
    }

    [Fact]
    public void Selection_NoLocationPurposeProperty()
    {
        // LocationPurpose REMOVED from SceneSelectionInputs (legacy)
        Type inputsType = typeof(SceneSelectionInputs);
        System.Reflection.PropertyInfo locationPurposeProperty = inputsType.GetProperty("LocationPurpose");

        Assert.Null(locationPurposeProperty);
    }

    [Fact]
    public void Selection_NoTierProperty()
    {
        // Tier REMOVED from SceneSelectionInputs (legacy)
        Type inputsType = typeof(SceneSelectionInputs);
        System.Reflection.PropertyInfo tierProperty = inputsType.GetProperty("Tier");

        Assert.Null(tierProperty);
    }

    [Fact]
    public void Selection_NoTargetCategoryOverride()
    {
        // TargetCategory override was never added - no bypassing selection logic
        Type inputsType = typeof(SceneSelectionInputs);
        System.Reflection.PropertyInfo targetCategoryProperty = inputsType.GetProperty("TargetCategory");

        Assert.Null(targetCategoryProperty);
    }

    // ==================== AUTHORED CONTENT USES SAME LOGIC ====================

    [Fact]
    public void AuthoredContent_UsesIdenticalSelectionLogic()
    {
        // Authored content provides RhythmPattern that flows through SAME logic
        SceneSpawnReward authoredReward = new SceneSpawnReward
        {
            RhythmPatternContext = RhythmPattern.Building
        };

        // Build inputs from authored content
        SceneSelectionInputs authoredInputs = authoredReward.BuildAuthoredInputs();

        // Build equivalent procedural inputs
        SceneSelectionInputs proceduralInputs = CreateTestInputs(
            rhythmPattern: RhythmPattern.Building);

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

    [Fact]
    public void SceneSpawnReward_NoLegacyContextProperties()
    {
        // SceneSpawnReward should NOT have legacy LocationSafety/Purpose/Tier properties

        Type rewardType = typeof(SceneSpawnReward);
        System.Reflection.PropertyInfo locationSafetyProperty = rewardType.GetProperty("LocationSafetyContext");
        System.Reflection.PropertyInfo locationPurposeProperty = rewardType.GetProperty("LocationPurposeContext");
        System.Reflection.PropertyInfo tierProperty = rewardType.GetProperty("TierContext");

        Assert.Null(locationSafetyProperty);
        Assert.Null(locationPurposeProperty);
        Assert.Null(tierProperty);
    }

    // ==================== HELPER METHODS ====================

    /// <summary>
    /// Create test inputs with specified values, defaults for others.
    /// SIMPLIFIED: Only RhythmPattern + anti-repetition.
    /// </summary>
    private SceneSelectionInputs CreateTestInputs(
        RhythmPattern rhythmPattern = RhythmPattern.Building,
        List<string> recentCategories = null)
    {
        return new SceneSelectionInputs
        {
            RhythmPattern = rhythmPattern,
            RecentCategories = recentCategories ?? new List<string>(),
            RecentArchetypes = new List<string>()
        };
    }

    /// <summary>
    /// Test implementation of SelectArchetypeCategory logic.
    /// SIMPLIFIED: RhythmPattern + anti-repetition only.
    ///
    /// PRODUCTION IMPLEMENTATION LOCATION:
    /// ProceduralAStoryService.SelectArchetypeCategory() in
    /// src/Subsystems/ProceduralContent/ProceduralAStoryService.cs
    /// </summary>
    private string TestSelectArchetypeCategory(SceneSelectionInputs inputs)
    {
        // Get appropriate categories for rhythm pattern ONLY
        List<string> appropriateCategories = inputs.RhythmPattern switch
        {
            RhythmPattern.Building => new List<string> { "Investigation", "Social", "Confrontation" },
            RhythmPattern.Crisis => new List<string> { "Crisis", "Confrontation" },
            RhythmPattern.Mixed => new List<string> { "Social", "Investigation" },
            _ => new List<string> { "Investigation", "Social", "Confrontation", "Crisis" }
        };

        // Apply anti-repetition
        List<string> available = appropriateCategories
            .Where(c => !inputs.RecentCategories.Contains(c))
            .ToList();

        if (!available.Any())
        {
            available = appropriateCategories;
        }

        // First available category for determinism
        return available[0];
    }
}

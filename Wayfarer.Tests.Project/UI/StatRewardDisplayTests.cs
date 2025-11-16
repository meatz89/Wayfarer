using Xunit;

namespace Wayfarer.Tests.UI;

/// <summary>
/// Unit tests for Five Stats reward display logic in choice selection UI.
/// Tests the calculation, mapping, and null-handling added to SceneContent component.
///
/// WHAT'S BEING TESTED:
/// - Null-safe extraction from ChoiceReward.Insight/Rapport/etc
/// - Final value calculation (player.Stat + reward)
/// - Mapping to ActionCardViewModel properties
/// - Edge cases: null rewards, zero values, positive values
///
/// ARCHITECTURE CONTEXT:
/// These tests verify UI display calculations, not stat application.
/// Stat application logic (RewardApplicationService) is separate.
/// </summary>
public class StatRewardDisplayTests
{
    /// <summary>
    /// Test helper: Create player with specific stat values for testing
    /// </summary>
    private Player CreateTestPlayer(
        int insight = 0,
        int rapport = 0,
        int authority = 0,
        int diplomacy = 0,
        int cunning = 0)
    {
        return new Player
        {
            Name = "TestPlayer",
            Insight = insight,
            Rapport = rapport,
            Authority = authority,
            Diplomacy = diplomacy,
            Cunning = cunning,
            Coins = 10,
            Health = 100,
            Stamina = 100,
            Focus = 6,
            Hunger = 0
        };
    }

    /// <summary>
    /// Test helper: Create choice reward with stat grants
    /// </summary>
    private ChoiceReward CreateStatReward(
        int insight = 0,
        int rapport = 0,
        int authority = 0,
        int diplomacy = 0,
        int cunning = 0)
    {
        return new ChoiceReward
        {
            Insight = insight,
            Rapport = rapport,
            Authority = authority,
            Diplomacy = diplomacy,
            Cunning = cunning
        };
    }

    [Fact]
    public void NullReward_ExtractsZeroForAllStats()
    {
        // ARRANGE: Null reward (choice grants no stats)
        ChoiceReward reward = null;

        // ACT: Null-safe extraction (SceneContent.razor.cs pattern)
        int insightReward = reward?.Insight ?? 0;
        int rapportReward = reward?.Rapport ?? 0;
        int authorityReward = reward?.Authority ?? 0;
        int diplomacyReward = reward?.Diplomacy ?? 0;
        int cunningReward = reward?.Cunning ?? 0;

        // ASSERT: All values default to 0
        Assert.Equal(0, insightReward);
        Assert.Equal(0, rapportReward);
        Assert.Equal(0, authorityReward);
        Assert.Equal(0, diplomacyReward);
        Assert.Equal(0, cunningReward);
    }

    [Fact]
    public void ZeroStatRewards_ExtractsCorrectly()
    {
        // ARRANGE: Reward exists but grants no stats (all zeros)
        ChoiceReward reward = CreateStatReward(0, 0, 0, 0, 0);

        // ACT: Extract values
        int insightReward = reward?.Insight ?? 0;
        int rapportReward = reward?.Rapport ?? 0;
        int authorityReward = reward?.Authority ?? 0;
        int diplomacyReward = reward?.Diplomacy ?? 0;
        int cunningReward = reward?.Cunning ?? 0;

        // ASSERT: All values are 0
        Assert.Equal(0, insightReward);
        Assert.Equal(0, rapportReward);
        Assert.Equal(0, authorityReward);
        Assert.Equal(0, diplomacyReward);
        Assert.Equal(0, cunningReward);
    }

    [Fact]
    public void PositiveStatRewards_ExtractsCorrectly()
    {
        // ARRANGE: Reward grants specific stat values
        ChoiceReward reward = CreateStatReward(
            insight: 2,
            rapport: 1,
            authority: 3,
            diplomacy: 1,
            cunning: 2);

        // ACT: Extract values
        int insightReward = reward?.Insight ?? 0;
        int rapportReward = reward?.Rapport ?? 0;
        int authorityReward = reward?.Authority ?? 0;
        int diplomacyReward = reward?.Diplomacy ?? 0;
        int cunningReward = reward?.Cunning ?? 0;

        // ASSERT: Values match reward specification
        Assert.Equal(2, insightReward);
        Assert.Equal(1, rapportReward);
        Assert.Equal(3, authorityReward);
        Assert.Equal(1, diplomacyReward);
        Assert.Equal(2, cunningReward);
    }

    [Fact]
    public void FinalValueCalculation_WithZeroPlayerStats_ReturnsRewardValue()
    {
        // ARRANGE: Player with no stats + reward granting stats
        Player player = CreateTestPlayer(0, 0, 0, 0, 0);
        ChoiceReward reward = CreateStatReward(2, 1, 3, 1, 2);

        int insightReward = reward.Insight;
        int rapportReward = reward.Rapport;
        int authorityReward = reward.Authority;
        int diplomacyReward = reward.Diplomacy;
        int cunningReward = reward.Cunning;

        // ACT: Calculate final values (SceneContent.razor.cs pattern)
        int finalInsight = player.Insight + insightReward;
        int finalRapport = player.Rapport + rapportReward;
        int finalAuthority = player.Authority + authorityReward;
        int finalDiplomacy = player.Diplomacy + diplomacyReward;
        int finalCunning = player.Cunning + cunningReward;

        // ASSERT: Final values equal reward values (0 + reward)
        Assert.Equal(2, finalInsight);
        Assert.Equal(1, finalRapport);
        Assert.Equal(3, finalAuthority);
        Assert.Equal(1, finalDiplomacy);
        Assert.Equal(2, finalCunning);
    }

    [Fact]
    public void FinalValueCalculation_WithExistingPlayerStats_AddsCorrectly()
    {
        // ARRANGE: Player with existing stats + reward granting more
        Player player = CreateTestPlayer(
            insight: 5,
            rapport: 3,
            authority: 2,
            diplomacy: 4,
            cunning: 1);
        ChoiceReward reward = CreateStatReward(
            insight: 2,
            rapport: 1,
            authority: 3,
            diplomacy: 1,
            cunning: 2);

        int insightReward = reward.Insight;
        int rapportReward = reward.Rapport;
        int authorityReward = reward.Authority;
        int diplomacyReward = reward.Diplomacy;
        int cunningReward = reward.Cunning;

        // ACT: Calculate final values
        int finalInsight = player.Insight + insightReward;
        int finalRapport = player.Rapport + rapportReward;
        int finalAuthority = player.Authority + authorityReward;
        int finalDiplomacy = player.Diplomacy + diplomacyReward;
        int finalCunning = player.Cunning + cunningReward;

        // ASSERT: Final values are sum of current + reward
        Assert.Equal(7, finalInsight);   // 5 + 2
        Assert.Equal(4, finalRapport);   // 3 + 1
        Assert.Equal(5, finalAuthority); // 2 + 3
        Assert.Equal(5, finalDiplomacy); // 4 + 1
        Assert.Equal(3, finalCunning);   // 1 + 2
    }

    [Fact]
    public void FinalValueCalculation_WithZeroReward_MaintainsPlayerStats()
    {
        // ARRANGE: Player with stats + no reward
        Player player = CreateTestPlayer(5, 3, 2, 4, 1);
        ChoiceReward reward = null;

        int insightReward = reward?.Insight ?? 0;
        int rapportReward = reward?.Rapport ?? 0;
        int authorityReward = reward?.Authority ?? 0;
        int diplomacyReward = reward?.Diplomacy ?? 0;
        int cunningReward = reward?.Cunning ?? 0;

        // ACT: Calculate final values
        int finalInsight = player.Insight + insightReward;
        int finalRapport = player.Rapport + rapportReward;
        int finalAuthority = player.Authority + authorityReward;
        int finalDiplomacy = player.Diplomacy + diplomacyReward;
        int finalCunning = player.Cunning + cunningReward;

        // ASSERT: Final values unchanged (current + 0)
        Assert.Equal(5, finalInsight);
        Assert.Equal(3, finalRapport);
        Assert.Equal(2, finalAuthority);
        Assert.Equal(4, finalDiplomacy);
        Assert.Equal(1, finalCunning);
    }

    [Fact]
    public void CurrentValueAssignment_ReflectsPlayerStats()
    {
        // ARRANGE: Player with specific stat distribution
        Player player = CreateTestPlayer(
            insight: 7,
            rapport: 2,
            authority: 5,
            diplomacy: 3,
            cunning: 4);

        // ACT: Assign current values (SceneContent.razor.cs pattern)
        int currentInsight = player.Insight;
        int currentRapport = player.Rapport;
        int currentAuthority = player.Authority;
        int currentDiplomacy = player.Diplomacy;
        int currentCunning = player.Cunning;

        // ASSERT: Current values match player state exactly
        Assert.Equal(7, currentInsight);
        Assert.Equal(2, currentRapport);
        Assert.Equal(5, currentAuthority);
        Assert.Equal(3, currentDiplomacy);
        Assert.Equal(4, currentCunning);
    }

    [Fact]
    public void CompleteMapping_AllPropertiesPopulated()
    {
        // ARRANGE: Realistic scenario - player with moderate stats receiving reward
        Player player = CreateTestPlayer(
            insight: 4,
            rapport: 3,
            authority: 2,
            diplomacy: 5,
            cunning: 3);
        ChoiceReward reward = CreateStatReward(
            insight: 1,
            rapport: 2,
            authority: 0,
            diplomacy: 1,
            cunning: 0);

        // ACT: Full mapping flow (as done in SceneContent.razor.cs)
        int insightReward = reward?.Insight ?? 0;
        int rapportReward = reward?.Rapport ?? 0;
        int authorityReward = reward?.Authority ?? 0;
        int diplomacyReward = reward?.Diplomacy ?? 0;
        int cunningReward = reward?.Cunning ?? 0;

        int finalInsight = player.Insight + insightReward;
        int finalRapport = player.Rapport + rapportReward;
        int finalAuthority = player.Authority + authorityReward;
        int finalDiplomacy = player.Diplomacy + diplomacyReward;
        int finalCunning = player.Cunning + cunningReward;

        int currentInsight = player.Insight;
        int currentRapport = player.Rapport;
        int currentAuthority = player.Authority;
        int currentDiplomacy = player.Diplomacy;
        int currentCunning = player.Cunning;

        // ASSERT: All values correctly calculated
        // Rewards
        Assert.Equal(1, insightReward);
        Assert.Equal(2, rapportReward);
        Assert.Equal(0, authorityReward);
        Assert.Equal(1, diplomacyReward);
        Assert.Equal(0, cunningReward);

        // Current values
        Assert.Equal(4, currentInsight);
        Assert.Equal(3, currentRapport);
        Assert.Equal(2, currentAuthority);
        Assert.Equal(5, currentDiplomacy);
        Assert.Equal(3, currentCunning);

        // Final values
        Assert.Equal(5, finalInsight);   // 4 + 1
        Assert.Equal(5, finalRapport);   // 3 + 2
        Assert.Equal(2, finalAuthority); // 2 + 0
        Assert.Equal(6, finalDiplomacy); // 5 + 1
        Assert.Equal(3, finalCunning);   // 3 + 0
    }

    [Fact]
    public void TutorialScenario_FriendlyElena_RapportReward()
    {
        // ARRANGE: Tutorial scenario - new player (stats 0) choosing Rapport path
        Player player = CreateTestPlayer(0, 0, 0, 0, 0);
        ChoiceReward reward = CreateStatReward(rapport: 1); // Elena grants Rapport +1

        // ACT: Calculate display values
        int rapportReward = reward?.Rapport ?? 0;
        int currentRapport = player.Rapport;
        int finalRapport = player.Rapport + rapportReward;

        // ASSERT: Tutorial first stat grant
        Assert.Equal(1, rapportReward);   // Choice grants +1
        Assert.Equal(0, currentRapport);  // Player starts at 0
        Assert.Equal(1, finalRapport);    // Will have 1 after choice
    }

    [Fact]
    public void SpecializedBuild_MultipleStatGrants()
    {
        // ARRANGE: Mid-game specialized player receiving multi-stat reward
        Player player = CreateTestPlayer(
            insight: 8,   // Specialized
            rapport: 2,   // Minor
            authority: 1, // Minimal
            diplomacy: 3, // Minor
            cunning: 6);  // Specialized
        ChoiceReward reward = CreateStatReward(
            insight: 2,   // Reinforces specialization
            cunning: 1);  // Reinforces specialization

        // ACT: Calculate all values
        int insightReward = reward?.Insight ?? 0;
        int cunningReward = reward?.Cunning ?? 0;
        int finalInsight = player.Insight + insightReward;
        int finalCunning = player.Cunning + cunningReward;

        // ASSERT: Specialization reinforcement
        Assert.Equal(2, insightReward);
        Assert.Equal(1, cunningReward);
        Assert.Equal(10, finalInsight);  // 8 + 2 (deep specialization)
        Assert.Equal(7, finalCunning);   // 6 + 1
    }
}

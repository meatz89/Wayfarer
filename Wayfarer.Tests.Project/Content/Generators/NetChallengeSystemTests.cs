using Xunit;

namespace Wayfarer.Tests.Content.Generators;

/// <summary>
/// Tests for the Net Challenge system: player strength vs location difficulty scaling.
/// Validates the query-time requirement adjustment documented in gdd/06_balance.md.
///
/// Net Challenge Formula: LocationDifficulty - (PlayerStrength / 5), clamped to [-3, +3]
/// - Negative = player overpowered (easier stat requirements)
/// - Positive = player underpowered (harder stat requirements)
/// </summary>
public class NetChallengeSystemTests
{
    // ============================================
    // PLAYER TOTAL STAT STRENGTH
    // ============================================

    [Fact]
    public void Player_TotalStatStrength_SumsAllFiveStats()
    {
        Player player = CreatePlayer(insight: 3, rapport: 2, authority: 4, diplomacy: 1, cunning: 5);

        Assert.Equal(15, player.TotalStatStrength);
    }

    [Fact]
    public void Player_TotalStatStrength_ZeroWhenNoStats()
    {
        Player player = CreatePlayer();

        Assert.Equal(0, player.TotalStatStrength);
    }

    [Fact]
    public void Player_TotalStatStrength_HighValuesCalculateCorrectly()
    {
        Player player = CreatePlayer(insight: 10, rapport: 10, authority: 10, diplomacy: 10, cunning: 10);

        Assert.Equal(50, player.TotalStatStrength);
    }

    [Fact]
    public void Player_GetStatValue_ReturnsCorrectStatByType()
    {
        Player player = CreatePlayer(insight: 3, rapport: 5, authority: 2, diplomacy: 7, cunning: 4);

        Assert.Equal(3, player.GetStatValue(PlayerStatType.Insight));
        Assert.Equal(5, player.GetStatValue(PlayerStatType.Rapport));
        Assert.Equal(2, player.GetStatValue(PlayerStatType.Authority));
        Assert.Equal(7, player.GetStatValue(PlayerStatType.Diplomacy));
        Assert.Equal(4, player.GetStatValue(PlayerStatType.Cunning));
    }

    [Fact]
    public void Player_GetStatValue_ThrowsForNoneType()
    {
        Player player = CreatePlayer();

        Assert.Throws<InvalidOperationException>(() => player.GetStatValue(PlayerStatType.None));
    }

    // ============================================
    // NET CHALLENGE DERIVATION
    // ============================================

    [Fact]
    public void RuntimeScalingContext_NetChallenge_ZeroWhenBalanced()
    {
        // Player with 25 stats (25/5 = 5) at location with difficulty 5
        Player player = CreatePlayer(insight: 5, rapport: 5, authority: 5, diplomacy: 5, cunning: 5);
        Location location = CreateLocation(difficulty: 5);

        RuntimeScalingContext context = RuntimeScalingContext.FromEntities(null, location, player);

        Assert.Equal(0, context.NetChallengeAdjustment);
    }

    [Fact]
    public void RuntimeScalingContext_NetChallenge_PositiveWhenUnderpowered()
    {
        // Player with 10 stats (10/5 = 2) at location with difficulty 4
        // Net = 4 - 2 = +2 (underpowered)
        Player player = CreatePlayer(insight: 2, rapport: 2, authority: 2, diplomacy: 2, cunning: 2);
        Location location = CreateLocation(difficulty: 4);

        RuntimeScalingContext context = RuntimeScalingContext.FromEntities(null, location, player);

        Assert.Equal(2, context.NetChallengeAdjustment);
    }

    [Fact]
    public void RuntimeScalingContext_NetChallenge_NegativeWhenOverpowered()
    {
        // Player with 30 stats (30/5 = 6) at location with difficulty 3
        // Net = 3 - 6 = -3 (overpowered)
        Player player = CreatePlayer(insight: 6, rapport: 6, authority: 6, diplomacy: 6, cunning: 6);
        Location location = CreateLocation(difficulty: 3);

        RuntimeScalingContext context = RuntimeScalingContext.FromEntities(null, location, player);

        Assert.Equal(-3, context.NetChallengeAdjustment);
    }

    [Fact]
    public void RuntimeScalingContext_NetChallenge_ClampedToPositiveThree()
    {
        // Player with 0 stats at location with difficulty 10
        // Net = 10 - 0 = 10, clamped to +3
        Player player = CreatePlayer();
        Location location = CreateLocation(difficulty: 10);

        RuntimeScalingContext context = RuntimeScalingContext.FromEntities(null, location, player);

        Assert.Equal(3, context.NetChallengeAdjustment);
    }

    [Fact]
    public void RuntimeScalingContext_NetChallenge_ClampedToNegativeThree()
    {
        // Player with 50 stats (50/5 = 10) at location with difficulty 0
        // Net = 0 - 10 = -10, clamped to -3
        Player player = CreatePlayer(insight: 10, rapport: 10, authority: 10, diplomacy: 10, cunning: 10);
        Location location = CreateLocation(difficulty: 0);

        RuntimeScalingContext context = RuntimeScalingContext.FromEntities(null, location, player);

        Assert.Equal(-3, context.NetChallengeAdjustment);
    }

    [Fact]
    public void RuntimeScalingContext_NetChallenge_ZeroWhenNoEntities()
    {
        RuntimeScalingContext context = RuntimeScalingContext.FromEntities(null, null, null);

        Assert.Equal(0, context.NetChallengeAdjustment);
    }

    // ============================================
    // APPLY STAT ADJUSTMENT (COMBINED)
    // ============================================

    [Fact]
    public void ApplyStatAdjustment_CombinesNpcDemeanorAndNetChallenge()
    {
        // StatRequirementAdjustment = +2 (hostile NPC)
        // NetChallengeAdjustment = +2 (underpowered)
        // Total = +4 on base requirement
        Player player = CreatePlayer(insight: 2, rapport: 2, authority: 2, diplomacy: 2, cunning: 2);
        Location location = CreateLocation(difficulty: 4);
        NPC hostileNpc = CreateHostileNpc();

        RuntimeScalingContext context = RuntimeScalingContext.FromEntities(hostileNpc, location, player);

        // Base requirement of 5, adjusted by +2 (hostile) + +2 (underpowered) = 9
        int scaled = context.ApplyStatAdjustment(5);
        Assert.Equal(9, scaled);
    }

    [Fact]
    public void ApplyStatAdjustment_FriendlyNpcAndOverpoweredPlayer_ReducesRequirements()
    {
        // StatRequirementAdjustment = -2 (friendly NPC)
        // NetChallengeAdjustment = -3 (overpowered, clamped)
        // Total = -5 on base requirement
        Player player = CreatePlayer(insight: 10, rapport: 10, authority: 10, diplomacy: 10, cunning: 10);
        Location location = CreateLocation(difficulty: 0);
        NPC friendlyNpc = CreateFriendlyNpc();

        RuntimeScalingContext context = RuntimeScalingContext.FromEntities(friendlyNpc, location, player);

        // Base requirement of 8, adjusted by -2 (friendly) + -3 (overpowered) = 3
        int scaled = context.ApplyStatAdjustment(8);
        Assert.Equal(3, scaled);
    }

    [Fact]
    public void ApplyStatAdjustment_NeverReturnsNegative()
    {
        // Heavy negative adjustment on low base value
        Player player = CreatePlayer(insight: 10, rapport: 10, authority: 10, diplomacy: 10, cunning: 10);
        Location location = CreateLocation(difficulty: 0);
        NPC friendlyNpc = CreateFriendlyNpc();

        RuntimeScalingContext context = RuntimeScalingContext.FromEntities(friendlyNpc, location, player);

        // Base requirement of 2, adjusted by -2 (friendly) + -3 (overpowered) = -3, clamped to 0
        int scaled = context.ApplyStatAdjustment(2);
        Assert.Equal(0, scaled);
    }

    [Fact]
    public void ApplyStatAdjustment_NeutralNpcAndBalancedPlayer_NoChange()
    {
        // StatRequirementAdjustment = 0 (neutral NPC, flow 10-14)
        // NetChallengeAdjustment = 0 (balanced)
        Player player = CreatePlayer(insight: 5, rapport: 5, authority: 5, diplomacy: 5, cunning: 5);
        Location location = CreateLocation(difficulty: 5);
        NPC neutralNpc = CreateNeutralNpc();

        RuntimeScalingContext context = RuntimeScalingContext.FromEntities(neutralNpc, location, player);

        // Base requirement of 6, no adjustment
        int scaled = context.ApplyStatAdjustment(6);
        Assert.Equal(6, scaled);
    }

    // ============================================
    // APPLY TO REQUIREMENT (FULL INTEGRATION)
    // ============================================

    [Fact]
    public void ApplyToRequirement_ScalesAllStatThresholdsWithNetChallenge()
    {
        Player player = CreatePlayer(insight: 2, rapport: 2, authority: 2, diplomacy: 2, cunning: 2);
        Location location = CreateLocation(difficulty: 4);
        NPC hostileNpc = CreateHostileNpc();

        RuntimeScalingContext context = RuntimeScalingContext.FromEntities(hostileNpc, location, player);

        CompoundRequirement original = new CompoundRequirement
        {
            OrPaths = new List<OrPath>
            {
                new OrPath
                {
                    InsightRequired = 3,
                    RapportRequired = 4,
                    AuthorityRequired = 5
                }
            }
        };

        CompoundRequirement scaled = context.ApplyToRequirement(original);

        // Each stat requirement increased by +4 (+2 hostile + +2 underpowered)
        Assert.Equal(7, scaled.OrPaths[0].InsightRequired);
        Assert.Equal(8, scaled.OrPaths[0].RapportRequired);
        Assert.Equal(9, scaled.OrPaths[0].AuthorityRequired);
    }

    // ============================================
    // HELPER METHODS
    // ============================================

    private Player CreatePlayer(
        int insight = 0,
        int rapport = 0,
        int authority = 0,
        int diplomacy = 0,
        int cunning = 0)
    {
        return new Player
        {
            Insight = insight,
            Rapport = rapport,
            Authority = authority,
            Diplomacy = diplomacy,
            Cunning = cunning,
            Health = 100,
            Stamina = 100,
            Focus = 6,
            Coins = 50,
            CurrentPosition = new AxialCoordinates(0, 0)
        };
    }

    private Location CreateLocation(int difficulty)
    {
        return new Location("TestLocation")
        {
            Difficulty = difficulty,
            HexPosition = new AxialCoordinates(difficulty * 5, 0),
            Purpose = LocationPurpose.Commerce
        };
    }

    private NPC CreateHostileNpc()
    {
        return new NPC
        {
            Name = "HostileNPC",
            PersonalityType = PersonalityType.MERCANTILE,
            RelationshipFlow = 5, // <= 9 = Hostile
            Level = 3
        };
    }

    private NPC CreateFriendlyNpc()
    {
        return new NPC
        {
            Name = "FriendlyNPC",
            PersonalityType = PersonalityType.DEVOTED,
            RelationshipFlow = 18, // > 14 = Friendly
            Level = 3
        };
    }

    private NPC CreateNeutralNpc()
    {
        return new NPC
        {
            Name = "NeutralNPC",
            PersonalityType = PersonalityType.STEADFAST,
            RelationshipFlow = 12, // 10-14 = Neutral
            Level = 3
        };
    }
}

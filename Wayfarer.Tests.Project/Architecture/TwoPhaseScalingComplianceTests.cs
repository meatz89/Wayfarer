using System.Reflection;
using Xunit;

namespace Wayfarer.Tests.Architecture;

/// <summary>
/// Compliance tests for Two-Phase Scaling Model (arc42 §8.26)
///
/// Design Principle: Display = Execution (Perfect Information)
/// - ScaledRequirement used for BOTH UI display AND requirement checking
/// - ScaledConsequence used for BOTH UI display AND cost application
/// - Player sees exactly what they will pay/receive
///
/// Forbidden: Display showing scaled values while execution uses original values
/// </summary>
public class TwoPhaseScalingComplianceTests
{
    // ==================== SCALING FORMULA TESTS ====================

    [Fact]
    public void StatRequirementAdjustment_HostileNpc_AddsTwoToRequirement()
    {
        // Arrange: Create hostile NPC (RelationshipFlow <= 9)
        NPC hostileNpc = new NPC { RelationshipFlow = 5 };
        Location location = new Location("TestLocation") { Difficulty = 1 };
        Player player = new Player();

        // Act
        RuntimeScalingContext context = RuntimeScalingContext.FromEntities(hostileNpc, location, player);

        // Assert: Hostile adds +2 to stat requirements
        Assert.Equal(2, context.StatRequirementAdjustment);
    }

    [Fact]
    public void StatRequirementAdjustment_FriendlyNpc_SubtractsTwoFromRequirement()
    {
        // Arrange: Create friendly NPC (RelationshipFlow >= 15)
        NPC friendlyNpc = new NPC { RelationshipFlow = 18 };
        Location location = new Location("TestLocation") { Difficulty = 1 };
        Player player = new Player();

        // Act
        RuntimeScalingContext context = RuntimeScalingContext.FromEntities(friendlyNpc, location, player);

        // Assert: Friendly subtracts -2 from stat requirements
        Assert.Equal(-2, context.StatRequirementAdjustment);
    }

    [Fact]
    public void StatRequirementAdjustment_NeutralNpc_NoAdjustment()
    {
        // Arrange: Create neutral NPC (RelationshipFlow 10-14)
        NPC neutralNpc = new NPC { RelationshipFlow = 12 };
        Location location = new Location("TestLocation") { Difficulty = 1 };
        Player player = new Player();

        // Act
        RuntimeScalingContext context = RuntimeScalingContext.FromEntities(neutralNpc, location, player);

        // Assert: Neutral has no adjustment
        Assert.Equal(0, context.StatRequirementAdjustment);
    }

    [Fact]
    public void CoinCostAdjustment_BasicLocation_SubtractsThree()
    {
        // Arrange: Basic location (Difficulty 0 = Basic quality)
        NPC npc = new NPC { RelationshipFlow = 12 };
        Location basicLocation = new Location("BasicLocation") { Difficulty = 0 };
        Player player = new Player();

        // Act
        RuntimeScalingContext context = RuntimeScalingContext.FromEntities(npc, basicLocation, player);

        // Assert: Basic location subtracts 3 from costs
        Assert.Equal(-3, context.CoinCostAdjustment);
    }

    [Fact]
    public void CoinCostAdjustment_LuxuryLocation_AddsTen()
    {
        // Arrange: Luxury location (Difficulty >= 3 = Luxury quality)
        NPC npc = new NPC { RelationshipFlow = 12 };
        Location luxuryLocation = new Location("LuxuryLocation") { Difficulty = 3 };
        Player player = new Player();

        // Act
        RuntimeScalingContext context = RuntimeScalingContext.FromEntities(npc, luxuryLocation, player);

        // Assert: Luxury location adds 10 to costs
        Assert.Equal(10, context.CoinCostAdjustment);
    }

    [Fact]
    public void ApplyStatAdjustment_ClampsToZero_WhenAdjustmentExceedsBase()
    {
        // Arrange: Friendly NPC (-2 adjustment) with low base requirement
        RuntimeScalingContext context = new RuntimeScalingContext
        {
            StatRequirementAdjustment = -5  // Would make requirement negative
        };

        // Act: Apply to base requirement of 3
        int result = context.ApplyStatAdjustment(3);

        // Assert: Clamped to 0, not negative
        Assert.Equal(0, result);
    }

    // ==================== REQUIREMENT SCALING TESTS ====================

    [Fact]
    public void ApplyToRequirement_ScalesStatThresholds()
    {
        // Arrange
        RuntimeScalingContext context = new RuntimeScalingContext
        {
            StatRequirementAdjustment = -2  // Friendly NPC
        };

        CompoundRequirement original = new CompoundRequirement
        {
            OrPaths = new List<OrPath>
            {
                new OrPath { InsightRequired = 5, AuthorityRequired = 3 }
            }
        };

        // Act
        CompoundRequirement scaled = context.ApplyToRequirement(original);

        // Assert: Both stats reduced by 2
        Assert.Equal(3, scaled.OrPaths[0].InsightRequired);
        Assert.Equal(1, scaled.OrPaths[0].AuthorityRequired);
    }

    [Fact]
    public void ApplyToRequirement_PreservesOriginal_Immutable()
    {
        // Arrange
        RuntimeScalingContext context = new RuntimeScalingContext
        {
            StatRequirementAdjustment = -2
        };

        CompoundRequirement original = new CompoundRequirement
        {
            OrPaths = new List<OrPath>
            {
                new OrPath { InsightRequired = 5 }
            }
        };

        // Act
        CompoundRequirement scaled = context.ApplyToRequirement(original);

        // Assert: Original unchanged (immutability)
        Assert.Equal(5, original.OrPaths[0].InsightRequired);
        Assert.Equal(3, scaled.OrPaths[0].InsightRequired);
        Assert.NotSame(original, scaled);
    }

    // ==================== CONSEQUENCE SCALING TESTS ====================

    [Fact]
    public void ApplyToConsequence_ScalesCoinCosts()
    {
        // Arrange: Premium location adds 5 to costs
        RuntimeScalingContext context = new RuntimeScalingContext
        {
            CoinCostAdjustment = 5
        };

        Consequence original = new Consequence { Coins = -10 };  // Cost 10 coins

        // Act
        Consequence scaled = context.ApplyToConsequence(original);

        // Assert: Cost increased by 5 (10 + 5 = 15)
        Assert.Equal(-15, scaled.Coins);
    }

    [Fact]
    public void ApplyToConsequence_DoesNotScaleRewards()
    {
        // Arrange
        RuntimeScalingContext context = new RuntimeScalingContext
        {
            CoinCostAdjustment = 5
        };

        Consequence original = new Consequence { Coins = 10 };  // Reward 10 coins

        // Act
        Consequence scaled = context.ApplyToConsequence(original);

        // Assert: Rewards not scaled (only costs)
        Assert.Equal(10, scaled.Coins);
    }

    [Fact]
    public void ApplyToConsequence_PreservesOriginal_Immutable()
    {
        // Arrange
        RuntimeScalingContext context = new RuntimeScalingContext
        {
            CoinCostAdjustment = 5
        };

        Consequence original = new Consequence { Coins = -10 };

        // Act
        Consequence scaled = context.ApplyToConsequence(original);

        // Assert: Original unchanged
        Assert.Equal(-10, original.Coins);
        Assert.Equal(-15, scaled.Coins);
        Assert.NotSame(original, scaled);
    }

    // ==================== ARCHITECTURE COMPLIANCE TESTS ====================

    [Fact]
    public void SceneFacade_CreatesScaledValues_ForLocationActions()
    {
        // Verify SceneFacade creates ScaledRequirement and ScaledConsequence
        // This is a code structure test - verifies the pattern exists

        Type sceneFacadeType = typeof(SceneFacade);
        MethodInfo getActionsMethod = sceneFacadeType.GetMethod("GetActionsAtLocation");

        Assert.NotNull(getActionsMethod);

        // Verify LocationAction has scaled properties
        Type locationActionType = typeof(LocationAction);
        PropertyInfo scaledRequirement = locationActionType.GetProperty("ScaledRequirement");
        PropertyInfo scaledConsequence = locationActionType.GetProperty("ScaledConsequence");

        Assert.NotNull(scaledRequirement);
        Assert.NotNull(scaledConsequence);
    }

    [Fact]
    public void LocationAction_HasScaledProperties()
    {
        Type type = typeof(LocationAction);

        Assert.NotNull(type.GetProperty("ScaledRequirement"));
        Assert.NotNull(type.GetProperty("ScaledConsequence"));
    }

    [Fact]
    public void NPCAction_HasScaledProperties()
    {
        Type type = typeof(NPCAction);

        Assert.NotNull(type.GetProperty("ScaledRequirement"));
        Assert.NotNull(type.GetProperty("ScaledConsequence"));
    }

    [Fact]
    public void PathCard_HasScaledProperties()
    {
        Type type = typeof(PathCard);

        Assert.NotNull(type.GetProperty("ScaledRequirement"));
        Assert.NotNull(type.GetProperty("ScaledConsequence"));
    }

    [Fact]
    public void RuntimeScalingContext_HasRequiredAdjustmentMethods()
    {
        Type type = typeof(RuntimeScalingContext);

        // Factory method
        Assert.NotNull(type.GetMethod("FromEntities"));
        Assert.NotNull(type.GetMethod("None"));

        // Application methods
        Assert.NotNull(type.GetMethod("ApplyToRequirement"));
        Assert.NotNull(type.GetMethod("ApplyToConsequence"));
        Assert.NotNull(type.GetMethod("ApplyStatAdjustment"));
        Assert.NotNull(type.GetMethod("ApplyCoinAdjustment"));
        Assert.NotNull(type.GetMethod("ApplyResolveAdjustment"));
    }
}

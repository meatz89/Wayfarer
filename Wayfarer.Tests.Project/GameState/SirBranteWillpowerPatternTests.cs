using Xunit;

namespace Wayfarer.Tests.GameState;

/// <summary>
/// Tests for the Sir Brante Willpower Pattern implementation.
/// Validates that Resolve uses GATE logic (>= 0), not AFFORDABILITY logic (>= cost).
/// See arc42/08 §8.20 and ADR-017 for documentation.
/// </summary>
public class SirBranteWillpowerPatternTests
{
    // ============================================
    // RESOLVE GATE LOGIC (OrPath.IsSatisfied)
    // ============================================

    [Fact]
    public void OrPath_ResolveZero_SatisfiesGateCheck()
    {
        // Sir Brante pattern: Resolve = 0 satisfies >= 0 gate
        Player player = CreatePlayer(resolve: 0);
        OrPath path = new OrPath { ResolveRequired = 0 };

        Assert.True(path.IsSatisfied(player, CreateGameWorld()));
    }

    [Fact]
    public void OrPath_ResolvePositive_SatisfiesGateCheck()
    {
        // Sir Brante pattern: Resolve = 10 satisfies >= 0 gate
        Player player = CreatePlayer(resolve: 10);
        OrPath path = new OrPath { ResolveRequired = 0 };

        Assert.True(path.IsSatisfied(player, CreateGameWorld()));
    }

    [Fact]
    public void OrPath_ResolveNegative_FailsGateCheck()
    {
        // Sir Brante pattern: Resolve = -5 does NOT satisfy >= 0 gate
        Player player = CreatePlayer(resolve: -5);
        OrPath path = new OrPath { ResolveRequired = 0 };

        Assert.False(path.IsSatisfied(player, CreateGameWorld()));
    }

    [Fact]
    public void OrPath_ResolveNegativeOne_FailsGateCheck()
    {
        // Edge case: Resolve = -1 fails the gate
        Player player = CreatePlayer(resolve: -1);
        OrPath path = new OrPath { ResolveRequired = 0 };

        Assert.False(path.IsSatisfied(player, CreateGameWorld()));
    }

    // ============================================
    // COMPOUND REQUIREMENT FACTORY
    // ============================================

    [Fact]
    public void CompoundRequirement_CreateForConsequence_AddsResolveGate_WhenNegativeResolve()
    {
        // When consequence costs Resolve, should add ResolveRequired = 0 (gate check)
        Consequence consequence = new Consequence { Resolve = -5 };

        CompoundRequirement req = CompoundRequirement.CreateForConsequence(consequence);

        Assert.Single(req.OrPaths);
        Assert.Equal(0, req.OrPaths[0].ResolveRequired);
        Assert.Equal("Resolve 0+", req.OrPaths[0].Label);
    }

    [Fact]
    public void CompoundRequirement_CreateForConsequence_NoResolveGate_WhenPositiveResolve()
    {
        // When consequence GIVES Resolve (positive), no gate needed
        Consequence consequence = new Consequence { Resolve = 10 };

        CompoundRequirement req = CompoundRequirement.CreateForConsequence(consequence);

        Assert.Empty(req.OrPaths);
    }

    [Fact]
    public void CompoundRequirement_CreateForConsequence_NoResolveGate_WhenZeroResolve()
    {
        // When consequence has no Resolve change, no gate needed
        Consequence consequence = new Consequence { Resolve = 0 };

        CompoundRequirement req = CompoundRequirement.CreateForConsequence(consequence);

        Assert.Empty(req.OrPaths);
    }

    // ============================================
    // CONSEQUENCE.ISAFFORDABLE (Resolve NOT Checked)
    // ============================================

    [Fact]
    public void Consequence_IsAffordable_DoesNotCheckResolve_WhenZeroResolve()
    {
        // Player has Resolve = 0, consequence costs -5 Resolve
        // Should be AFFORDABLE (Resolve is NOT an affordability resource)
        Player player = CreatePlayer(resolve: 0, coins: 100);
        Consequence consequence = new Consequence { Resolve = -5 };

        Assert.True(consequence.IsAffordable(player));
    }

    [Fact]
    public void Consequence_IsAffordable_DoesNotCheckResolve_WhenNegativeResolve()
    {
        // Player has Resolve = -10, consequence costs -5 Resolve
        // Should be AFFORDABLE (Resolve is NOT an affordability resource)
        Player player = CreatePlayer(resolve: -10, coins: 100);
        Consequence consequence = new Consequence { Resolve = -5 };

        Assert.True(consequence.IsAffordable(player));
    }

    [Fact]
    public void Consequence_IsAffordable_StillChecksCoins()
    {
        // Coins ARE still checked for affordability
        Player player = CreatePlayer(resolve: 10, coins: 0);
        Consequence consequence = new Consequence { Coins = -5 };

        Assert.False(consequence.IsAffordable(player));
    }

    [Fact]
    public void Consequence_IsAffordable_StillChecksHealth()
    {
        // Health IS still checked for affordability
        Player player = CreatePlayer(resolve: 10);
        player.Health = 2;
        Consequence consequence = new Consequence { Health = -5 };

        Assert.False(consequence.IsAffordable(player));
    }

    [Fact]
    public void Consequence_IsAffordable_StillChecksStamina()
    {
        // Stamina IS still checked for affordability
        Player player = CreatePlayer(resolve: 10);
        player.Stamina = 2;
        Consequence consequence = new Consequence { Stamina = -5 };

        Assert.False(consequence.IsAffordable(player));
    }

    [Fact]
    public void Consequence_IsAffordable_StillChecksFocus()
    {
        // Focus IS still checked for affordability
        Player player = CreatePlayer(resolve: 10);
        player.Focus = 2;
        Consequence consequence = new Consequence { Focus = -5 };

        Assert.False(consequence.IsAffordable(player));
    }

    // ============================================
    // FULL INTEGRATION: Gate + Cost
    // ============================================

    [Fact]
    public void Integration_ResolveZero_CanTakeResolveCostingAction()
    {
        // Player at Resolve = 0 CAN take action costing -5 Resolve
        // Gate: 0 >= 0 = TRUE
        // Affordability: Resolve not checked = TRUE
        // Result: Action IS available
        Player player = CreatePlayer(resolve: 0, coins: 100);
        Consequence consequence = new Consequence { Resolve = -5 };
        CompoundRequirement gateReq = CompoundRequirement.CreateForConsequence(consequence);

        bool gateCheck = gateReq.IsAnySatisfied(player, CreateGameWorld());
        bool affordabilityCheck = consequence.IsAffordable(player);

        Assert.True(gateCheck, "Gate check (Resolve >= 0) should pass");
        Assert.True(affordabilityCheck, "Affordability check should pass (Resolve not checked)");
    }

    [Fact]
    public void Integration_ResolveNegative_CannotTakeResolveCostingAction()
    {
        // Player at Resolve = -5 CANNOT take action costing -5 Resolve
        // Gate: -5 >= 0 = FALSE
        // Affordability: Resolve not checked = TRUE (irrelevant, gate failed)
        // Result: Action is NOT available
        Player player = CreatePlayer(resolve: -5, coins: 100);
        Consequence consequence = new Consequence { Resolve = -5 };
        CompoundRequirement gateReq = CompoundRequirement.CreateForConsequence(consequence);

        bool gateCheck = gateReq.IsAnySatisfied(player, CreateGameWorld());

        Assert.False(gateCheck, "Gate check (Resolve >= 0) should FAIL when Resolve is negative");
    }

    [Fact]
    public void Integration_AfterCostApplied_ResolveCanGoNegative()
    {
        // Taking action costs -5 Resolve, player goes from 0 to -5
        // This is the Sir Brante pattern - you CAN go negative
        Player player = CreatePlayer(resolve: 0);
        int resolveCost = 5; // From Consequence.Resolve = -5

        player.Resolve -= resolveCost;

        Assert.Equal(-5, player.Resolve);
    }

    [Fact]
    public void Integration_AfterGoingNegative_NextCostlyActionBlocked()
    {
        // After going negative (-5), the NEXT costly action is blocked
        // This is the willpower gate kicking in
        Player player = CreatePlayer(resolve: -5);
        Consequence nextAction = new Consequence { Resolve = -5 };
        CompoundRequirement gateReq = CompoundRequirement.CreateForConsequence(nextAction);

        bool canTakeNextAction = gateReq.IsAnySatisfied(player, CreateGameWorld());

        Assert.False(canTakeNextAction, "Player at negative Resolve cannot take costly actions");
    }

    [Fact]
    public void Integration_Rebuild_ThenCanTakeActions()
    {
        // Player at -5, gains +10 Resolve, now at +5
        // Can take costly actions again
        Player player = CreatePlayer(resolve: -5);
        player.Resolve += 10; // Rebuild through positive choice

        Consequence costlyAction = new Consequence { Resolve = -5 };
        CompoundRequirement gateReq = CompoundRequirement.CreateForConsequence(costlyAction);

        bool canTakeAction = gateReq.IsAnySatisfied(player, CreateGameWorld());

        Assert.True(canTakeAction, "Player who rebuilt to positive Resolve can take costly actions");
    }

    // ============================================
    // EDGE CASES
    // ============================================

    [Fact]
    public void EdgeCase_LargeNegativeResolve_StillBlocksActions()
    {
        // Even at Resolve = -100, gate check works correctly
        Player player = CreatePlayer(resolve: -100);
        Consequence consequence = new Consequence { Resolve = -5 };
        CompoundRequirement gateReq = CompoundRequirement.CreateForConsequence(consequence);

        Assert.False(gateReq.IsAnySatisfied(player, CreateGameWorld()));
    }

    [Fact]
    public void EdgeCase_LargePositiveResolve_PassesGate()
    {
        // Resolve = 100 passes gate check
        Player player = CreatePlayer(resolve: 100);
        Consequence consequence = new Consequence { Resolve = -50 };
        CompoundRequirement gateReq = CompoundRequirement.CreateForConsequence(consequence);

        Assert.True(gateReq.IsAnySatisfied(player, CreateGameWorld()));
    }

    [Fact]
    public void EdgeCase_CombinedConsequence_OnlyResolveCreatesGate()
    {
        // Consequence with both Coins and Resolve costs
        // Only Resolve creates a gate requirement
        Consequence consequence = new Consequence { Resolve = -5, Coins = -10 };
        CompoundRequirement gateReq = CompoundRequirement.CreateForConsequence(consequence);

        Assert.Single(gateReq.OrPaths);
        Assert.Equal(0, gateReq.OrPaths[0].ResolveRequired);
        Assert.Null(gateReq.OrPaths[0].CoinsRequired); // Coins don't create gate
    }

    // ============================================
    // HELPER METHODS
    // ============================================

    private Player CreatePlayer(int resolve = 0, int coins = 100)
    {
        return new Player
        {
            Resolve = resolve,
            Coins = coins,
            Insight = 0,
            Rapport = 0,
            Authority = 0,
            Diplomacy = 0,
            Cunning = 0,
            Health = 100,
            Stamina = 100,
            Focus = 100,
            Hunger = 0,
            MaxHealth = 100,
            MaxStamina = 100,
            MaxFocus = 100,
            MaxHunger = 100,
            Scales = new PlayerScales(),
            ActiveStates = new List<ActiveState>(),
            Inventory = new Inventory(),
            EarnedAchievements = new List<PlayerAchievement>(),
            CompletedSituations = new List<Situation>()
        };
    }

    private GameWorld CreateGameWorld()
    {
        return new GameWorld();
    }
}

using Xunit;

namespace Wayfarer.Tests.GameState;

/// <summary>
/// Tests for the Sir Brante Willpower Pattern implementation.
/// Validates that Resolve uses GATE logic (>= 0), not AFFORDABILITY logic (>= cost).
/// See arc42/08 §8.20 and ADR-017 for documentation.
///
/// BEHAVIOR-ONLY TESTING: Tests verify boolean outcomes (satisfied/not satisfied),
/// not implementation details. Callers build OrPath directly.
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
    // RESOURCE AFFORDABILITY (OrPath.IsSatisfied)
    // ============================================

    [Fact]
    public void OrPath_CoinsAffordability_PassesWhenEnough()
    {
        Player player = CreatePlayer(coins: 100);
        OrPath path = new OrPath { CoinsRequired = 50 };

        Assert.True(path.IsSatisfied(player, CreateGameWorld()));
    }

    [Fact]
    public void OrPath_CoinsAffordability_FailsWhenInsufficient()
    {
        Player player = CreatePlayer(coins: 10);
        OrPath path = new OrPath { CoinsRequired = 50 };

        Assert.False(path.IsSatisfied(player, CreateGameWorld()));
    }

    [Fact]
    public void OrPath_HealthAffordability_FailsWhenInsufficient()
    {
        Player player = CreatePlayer();
        player.Health = 2;
        OrPath path = new OrPath { HealthRequired = 5 };

        Assert.False(path.IsSatisfied(player, CreateGameWorld()));
    }

    [Fact]
    public void OrPath_HungerCapacity_FailsWhenFull()
    {
        Player player = CreatePlayer();
        player.Hunger = 90;
        OrPath path = new OrPath { HungerCapacityRequired = 20 };

        Assert.False(path.IsSatisfied(player, CreateGameWorld()));
    }

    [Fact]
    public void OrPath_HungerCapacity_PassesWhenEnoughRoom()
    {
        Player player = CreatePlayer();
        player.Hunger = 50;
        OrPath path = new OrPath { HungerCapacityRequired = 20 };

        Assert.True(path.IsSatisfied(player, CreateGameWorld()));
    }

    // ============================================
    // FULL INTEGRATION: Sir Brante Willpower Pattern
    // Tests build OrPath directly - no factory methods
    // ============================================

    [Fact]
    public void Integration_ResolveZero_CanTakeResolveCostingAction()
    {
        // Player at Resolve = 0 CAN take action costing -5 Resolve
        // Gate: 0 >= 0 = TRUE
        // Result: Action IS available
        Player player = CreatePlayer(resolve: 0, coins: 100);
        OrPath path = new OrPath { ResolveRequired = 0 };  // Gate pattern

        Assert.True(path.IsSatisfied(player, CreateGameWorld()));
    }

    [Fact]
    public void Integration_ResolveNegative_CannotTakeResolveCostingAction()
    {
        // Player at Resolve = -5 CANNOT take action costing -5 Resolve
        // Gate: -5 >= 0 = FALSE
        // Result: Action is NOT available
        Player player = CreatePlayer(resolve: -5, coins: 100);
        OrPath path = new OrPath { ResolveRequired = 0 };  // Gate pattern

        Assert.False(path.IsSatisfied(player, CreateGameWorld()));
    }

    [Fact]
    public void Integration_AfterCostApplied_ResolveCanGoNegative()
    {
        // Taking action costs Resolve, player can go negative
        // This is the Sir Brante pattern - you CAN go negative
        Player player = CreatePlayer(resolve: 0);
        int resolveCost = 5;

        player.Resolve -= resolveCost;

        Assert.True(player.Resolve < 0);
    }

    [Fact]
    public void Integration_AfterGoingNegative_NextCostlyActionBlocked()
    {
        // After going negative (-5), the NEXT costly action is blocked
        // This is the willpower gate kicking in
        Player player = CreatePlayer(resolve: -5);
        OrPath path = new OrPath { ResolveRequired = 0 };  // Gate pattern

        Assert.False(path.IsSatisfied(player, CreateGameWorld()));
    }

    [Fact]
    public void Integration_Rebuild_ThenCanTakeActions()
    {
        // Player at -5, gains +10 Resolve, now at +5
        // Can take costly actions again
        Player player = CreatePlayer(resolve: -5);
        player.Resolve += 10; // Rebuild through positive choice

        OrPath path = new OrPath { ResolveRequired = 0 };  // Gate pattern

        Assert.True(path.IsSatisfied(player, CreateGameWorld()));
    }

    // ============================================
    // EDGE CASES
    // ============================================

    [Fact]
    public void EdgeCase_LargeNegativeResolve_StillBlocksActions()
    {
        // Even at Resolve = -100, gate check works correctly
        Player player = CreatePlayer(resolve: -100);
        OrPath path = new OrPath { ResolveRequired = 0 };  // Gate pattern

        Assert.False(path.IsSatisfied(player, CreateGameWorld()));
    }

    [Fact]
    public void EdgeCase_LargePositiveResolve_PassesGate()
    {
        // Resolve = 100 passes gate check
        Player player = CreatePlayer(resolve: 100);
        OrPath path = new OrPath { ResolveRequired = 0 };  // Gate pattern

        Assert.True(path.IsSatisfied(player, CreateGameWorld()));
    }

    [Fact]
    public void EdgeCase_CombinedRequirements_BothMustPass()
    {
        // Both Resolve gate AND Coins affordability must pass
        Player player = CreatePlayer(resolve: 0, coins: 100);
        OrPath path = new OrPath
        {
            ResolveRequired = 0,     // Gate pattern
            CoinsRequired = 50       // Affordability pattern
        };

        Assert.True(path.IsSatisfied(player, CreateGameWorld()));
    }

    [Fact]
    public void EdgeCase_CombinedRequirements_FailsIfResolveFails()
    {
        // Resolve gate fails, even though coins are sufficient
        Player player = CreatePlayer(resolve: -5, coins: 100);
        OrPath path = new OrPath
        {
            ResolveRequired = 0,     // Gate pattern - FAILS
            CoinsRequired = 50       // Affordability pattern - passes
        };

        Assert.False(path.IsSatisfied(player, CreateGameWorld()));
    }

    [Fact]
    public void EdgeCase_CombinedRequirements_FailsIfCoinsFails()
    {
        // Coins affordability fails, even though resolve gate passes
        Player player = CreatePlayer(resolve: 5, coins: 10);
        OrPath path = new OrPath
        {
            ResolveRequired = 0,     // Gate pattern - passes
            CoinsRequired = 50       // Affordability pattern - FAILS
        };

        Assert.False(path.IsSatisfied(player, CreateGameWorld()));
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

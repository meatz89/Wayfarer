using Xunit;

namespace Wayfarer.Tests.GameState;

public class ConsequenceTests
{
    // ============== HasAnyCosts Tests ==============

    [Fact]
    public void HasAnyCosts_WithNegativeCoins_ReturnsTrue()
    {
        Consequence consequence = new Consequence { Coins = -50 };
        Assert.True(consequence.HasAnyCosts());
    }

    [Fact]
    public void HasAnyCosts_WithZeroValues_ReturnsFalse()
    {
        Consequence consequence = new Consequence();
        Assert.False(consequence.HasAnyCosts());
    }

    [Fact]
    public void HasAnyCosts_WithPositiveHunger_ReturnsTrue()
    {
        // Positive hunger is a cost (bad for player)
        Consequence consequence = new Consequence { Hunger = 5 };
        Assert.True(consequence.HasAnyCosts());
    }

    [Fact]
    public void HasAnyCosts_WithTimeSegments_ReturnsTrue()
    {
        Consequence consequence = new Consequence { TimeSegments = 2 };
        Assert.True(consequence.HasAnyCosts());
    }

    // ============== HasAnyRewards Tests ==============

    [Fact]
    public void HasAnyRewards_WithPositiveCoins_ReturnsTrue()
    {
        Consequence consequence = new Consequence { Coins = 100 };
        Assert.True(consequence.HasAnyRewards());
    }

    [Fact]
    public void HasAnyRewards_WithStatGrants_ReturnsTrue()
    {
        Consequence consequence = new Consequence { Rapport = 1 };
        Assert.True(consequence.HasAnyRewards());
    }

    [Fact]
    public void HasAnyRewards_WithFullRecovery_ReturnsTrue()
    {
        Consequence consequence = new Consequence { FullRecovery = true };
        Assert.True(consequence.HasAnyRewards());
    }

    [Fact]
    public void HasAnyRewards_WithBondChanges_ReturnsTrue()
    {
        Consequence consequence = new Consequence
        {
            BondChanges = new List<BondChange> { new BondChange() }
        };
        Assert.True(consequence.HasAnyRewards());
    }

    // ============== HasAnyEffect Tests ==============

    [Fact]
    public void HasAnyEffect_WithCostsOnly_ReturnsTrue()
    {
        Consequence consequence = new Consequence { Resolve = -5 };
        Assert.True(consequence.HasAnyEffect());
    }

    [Fact]
    public void HasAnyEffect_WithRewardsOnly_ReturnsTrue()
    {
        Consequence consequence = new Consequence { Insight = 2 };
        Assert.True(consequence.HasAnyEffect());
    }

    [Fact]
    public void HasAnyEffect_WithNoEffects_ReturnsFalse()
    {
        Consequence consequence = new Consequence();
        Assert.False(consequence.HasAnyEffect());
    }

    // ============== IsAffordable Tests DELETED ==============
    // HIGHLANDER: All resource availability now tested via CompoundRequirement
    // See SirBranteWillpowerPatternTests.cs for unified tests

    // ============== GetProjectedState Tests ==============

    [Fact]
    public void GetProjectedState_WithNormalConsequence_ReturnsCorrectProjection()
    {
        Player player = CreatePlayerWithResources(100, 10, 100, 100, 100, 0);
        player.Insight = 5;
        Consequence consequence = new Consequence { Coins = -50, Insight = 2 };

        PlayerStateProjection projection = consequence.GetProjectedState(player);

        Assert.True(projection.Coins < player.Coins);
        Assert.True(projection.Insight > player.Insight);
    }

    [Fact]
    public void GetProjectedState_WithFullRecovery_ReturnsMaxValues()
    {
        Player player = CreatePlayerWithResources(100, 5, 50, 50, 50, 30);
        player.MaxHealth = 100;
        player.MaxStamina = 100;
        player.MaxFocus = 100;
        Consequence consequence = new Consequence { FullRecovery = true };

        PlayerStateProjection projection = consequence.GetProjectedState(player);

        Assert.True(projection.Health > player.Health);
        Assert.True(projection.Stamina > player.Stamina);
        Assert.True(projection.Focus > player.Focus);
        Assert.True(projection.Hunger < player.Hunger);
    }

    // ============== Helper ==============

    private Player CreatePlayerWithResources(int coins, int resolve, int health, int stamina, int focus, int hunger)
    {
        return new Player
        {
            Coins = coins,
            Resolve = resolve,
            Health = health,
            Stamina = stamina,
            Focus = focus,
            Hunger = hunger,
            MaxHealth = 100,
            MaxStamina = 100,
            MaxFocus = 100,
            MaxHunger = 100
        };
    }
}

using Xunit;

namespace Wayfarer.Tests.GameState;

/// <summary>
/// Tests for OrPath and CompoundRequirement with explicit property pattern.
/// Validates the Explicit Property Principle (arc42 §8.19) implementation.
/// </summary>
public class CompoundRequirementTests
{
    // ============================================
    // ORPATH STAT REQUIREMENTS
    // ============================================

    [Fact]
    public void OrPath_InsightRequired_SatisfiedWhenPlayerHasEnough()
    {
        Player player = CreatePlayer(insight: 5);
        OrPath path = new OrPath { InsightRequired = 3 };

        Assert.True(path.IsSatisfied(player, CreateGameWorld()));
    }

    [Fact]
    public void OrPath_InsightRequired_NotSatisfiedWhenPlayerHasLess()
    {
        Player player = CreatePlayer(insight: 2);
        OrPath path = new OrPath { InsightRequired = 3 };

        Assert.False(path.IsSatisfied(player, CreateGameWorld()));
    }

    [Fact]
    public void OrPath_RapportRequired_SatisfiedAtExactValue()
    {
        Player player = CreatePlayer(rapport: 5);
        OrPath path = new OrPath { RapportRequired = 5 };

        Assert.True(path.IsSatisfied(player, CreateGameWorld()));
    }

    [Fact]
    public void OrPath_AuthorityRequired_NotSatisfiedWhenBelowThreshold()
    {
        Player player = CreatePlayer(authority: 2);
        OrPath path = new OrPath { AuthorityRequired = 4 };

        Assert.False(path.IsSatisfied(player, CreateGameWorld()));
    }

    [Fact]
    public void OrPath_DiplomacyRequired_SatisfiedWhenAboveThreshold()
    {
        Player player = CreatePlayer(diplomacy: 7);
        OrPath path = new OrPath { DiplomacyRequired = 5 };

        Assert.True(path.IsSatisfied(player, CreateGameWorld()));
    }

    [Fact]
    public void OrPath_CunningRequired_NotSatisfiedWhenZero()
    {
        Player player = CreatePlayer(cunning: 0);
        OrPath path = new OrPath { CunningRequired = 1 };

        Assert.False(path.IsSatisfied(player, CreateGameWorld()));
    }

    // ============================================
    // ORPATH RESOURCE REQUIREMENTS
    // ============================================

    [Fact]
    public void OrPath_ResolveRequired_SatisfiedWhenPositive()
    {
        Player player = CreatePlayer(resolve: 5);
        OrPath path = new OrPath { ResolveRequired = 0 };

        Assert.True(path.IsSatisfied(player, CreateGameWorld()));
    }

    [Fact]
    public void OrPath_ResolveRequired_NotSatisfiedWhenNegative()
    {
        Player player = CreatePlayer(resolve: -1);
        OrPath path = new OrPath { ResolveRequired = 0 };

        Assert.False(path.IsSatisfied(player, CreateGameWorld()));
    }

    [Fact]
    public void OrPath_CoinsRequired_SatisfiedWithExactAmount()
    {
        Player player = CreatePlayer(coins: 100);
        OrPath path = new OrPath { CoinsRequired = 100 };

        Assert.True(path.IsSatisfied(player, CreateGameWorld()));
    }

    [Fact]
    public void OrPath_CoinsRequired_NotSatisfiedWhenInsufficient()
    {
        Player player = CreatePlayer(coins: 50);
        OrPath path = new OrPath { CoinsRequired = 100 };

        Assert.False(path.IsSatisfied(player, CreateGameWorld()));
    }

    // ============================================
    // ORPATH MULTIPLE REQUIREMENTS (AND LOGIC)
    // ============================================

    [Fact]
    public void OrPath_MultipleStats_AllMustBeSatisfied()
    {
        Player player = CreatePlayer(insight: 5, rapport: 3, authority: 2);
        OrPath path = new OrPath
        {
            InsightRequired = 3,
            RapportRequired = 3,
            AuthorityRequired = 2
        };

        Assert.True(path.IsSatisfied(player, CreateGameWorld()));
    }

    [Fact]
    public void OrPath_MultipleStats_FailsIfOneMissing()
    {
        Player player = CreatePlayer(insight: 5, rapport: 2, authority: 2);
        OrPath path = new OrPath
        {
            InsightRequired = 3,
            RapportRequired = 3, // Player has 2, needs 3
            AuthorityRequired = 2
        };

        Assert.False(path.IsSatisfied(player, CreateGameWorld()));
    }

    [Fact]
    public void OrPath_NoRequirements_AlwaysSatisfied()
    {
        Player player = CreatePlayer();
        OrPath path = new OrPath { Label = "No requirements" };

        Assert.True(path.IsSatisfied(player, CreateGameWorld()));
    }

    // ============================================
    // ORPATH STATE REQUIREMENTS
    // ============================================

    [Fact]
    public void OrPath_RequiredState_SatisfiedWhenPlayerHasState()
    {
        Player player = CreatePlayer();
        player.ActiveStates.Add(new ActiveState { Type = StateType.Wounded });
        OrPath path = new OrPath { RequiredState = StateType.Wounded };

        Assert.True(path.IsSatisfied(player, CreateGameWorld()));
    }

    [Fact]
    public void OrPath_RequiredState_NotSatisfiedWhenPlayerLacksState()
    {
        Player player = CreatePlayer();
        OrPath path = new OrPath { RequiredState = StateType.Wounded };

        Assert.False(path.IsSatisfied(player, CreateGameWorld()));
    }

    // ============================================
    // ORPATH ITEM REQUIREMENTS
    // ============================================

    [Fact]
    public void OrPath_RequiredItem_SatisfiedWhenPlayerHasItem()
    {
        Player player = CreatePlayer();
        Item testItem = new Item { Name = "Test Key" };
        player.Inventory.Add(testItem);
        OrPath path = new OrPath { RequiredItem = testItem };

        Assert.True(path.IsSatisfied(player, CreateGameWorld()));
    }

    [Fact]
    public void OrPath_RequiredItem_NotSatisfiedWhenPlayerLacksItem()
    {
        Player player = CreatePlayer();
        Item testItem = new Item { Name = "Test Key" };
        OrPath path = new OrPath { RequiredItem = testItem };

        Assert.False(path.IsSatisfied(player, CreateGameWorld()));
    }

    // ============================================
    // ORPATH SCALE REQUIREMENTS
    // ============================================

    [Fact]
    public void OrPath_PositiveScaleThreshold_SatisfiedWhenAbove()
    {
        Player player = CreatePlayer();
        player.Scales.Morality = 5;
        OrPath path = new OrPath
        {
            RequiredScaleType = ScaleType.Morality,
            ScaleValueRequired = 3
        };

        Assert.True(path.IsSatisfied(player, CreateGameWorld()));
    }

    [Fact]
    public void OrPath_PositiveScaleThreshold_NotSatisfiedWhenBelow()
    {
        Player player = CreatePlayer();
        player.Scales.Morality = 2;
        OrPath path = new OrPath
        {
            RequiredScaleType = ScaleType.Morality,
            ScaleValueRequired = 3
        };

        Assert.False(path.IsSatisfied(player, CreateGameWorld()));
    }

    [Fact]
    public void OrPath_NegativeScaleThreshold_SatisfiedWhenBelow()
    {
        Player player = CreatePlayer();
        player.Scales.Lawfulness = -5;
        OrPath path = new OrPath
        {
            RequiredScaleType = ScaleType.Lawfulness,
            ScaleValueRequired = -3 // Must be <= -3
        };

        Assert.True(path.IsSatisfied(player, CreateGameWorld()));
    }

    [Fact]
    public void OrPath_NegativeScaleThreshold_NotSatisfiedWhenAbove()
    {
        Player player = CreatePlayer();
        player.Scales.Lawfulness = -1;
        OrPath path = new OrPath
        {
            RequiredScaleType = ScaleType.Lawfulness,
            ScaleValueRequired = -3 // Must be <= -3
        };

        Assert.False(path.IsSatisfied(player, CreateGameWorld()));
    }

    // ============================================
    // COMPOUND REQUIREMENT OR LOGIC
    // ============================================

    [Fact]
    public void CompoundRequirement_EmptyOrPaths_AlwaysSatisfied()
    {
        Player player = CreatePlayer();
        CompoundRequirement req = new CompoundRequirement();

        Assert.True(req.IsAnySatisfied(player, CreateGameWorld()));
    }

    [Fact]
    public void CompoundRequirement_SinglePath_MatchesPathResult()
    {
        Player player = CreatePlayer(insight: 5);
        CompoundRequirement req = new CompoundRequirement
        {
            OrPaths = new List<OrPath>
            {
                new OrPath { InsightRequired = 3 }
            }
        };

        Assert.True(req.IsAnySatisfied(player, CreateGameWorld()));
    }

    [Fact]
    public void CompoundRequirement_MultiplePaths_SatisfiedIfAnyMatch()
    {
        Player player = CreatePlayer(insight: 2, rapport: 5);
        CompoundRequirement req = new CompoundRequirement
        {
            OrPaths = new List<OrPath>
            {
                new OrPath { InsightRequired = 5 }, // Not satisfied (player has 2)
                new OrPath { RapportRequired = 3 }  // Satisfied (player has 5)
            }
        };

        Assert.True(req.IsAnySatisfied(player, CreateGameWorld()));
    }

    [Fact]
    public void CompoundRequirement_MultiplePaths_NotSatisfiedIfNoneMatch()
    {
        Player player = CreatePlayer(insight: 2, rapport: 2);
        CompoundRequirement req = new CompoundRequirement
        {
            OrPaths = new List<OrPath>
            {
                new OrPath { InsightRequired = 5 }, // Not satisfied
                new OrPath { RapportRequired = 5 }  // Not satisfied
            }
        };

        Assert.False(req.IsAnySatisfied(player, CreateGameWorld()));
    }

    // ============================================
    // ORPATH PROJECTION TESTS
    // ============================================

    [Fact]
    public void OrPath_GetProjection_ReturnsCorrectSatisfactionStatus()
    {
        Player player = CreatePlayer(insight: 5, rapport: 2);
        OrPath path = new OrPath
        {
            Label = "Test Path",
            InsightRequired = 3,
            RapportRequired = 5
        };

        PathProjection projection = path.GetProjection(player, CreateGameWorld());

        Assert.False(projection.IsSatisfied); // Rapport not met
        Assert.Equal(2, projection.Requirements.Count);
    }

    [Fact]
    public void OrPath_GetProjection_IncludesCurrentAndRequiredValues()
    {
        Player player = CreatePlayer(insight: 2);
        OrPath path = new OrPath { InsightRequired = 5 };

        PathProjection projection = path.GetProjection(player, CreateGameWorld());
        RequirementStatus insightStatus = projection.Requirements.First();

        Assert.True(insightStatus.CurrentValue < insightStatus.RequiredValue);
        Assert.False(insightStatus.IsSatisfied);
    }

    [Fact]
    public void OrPath_GetProjection_MissingRequirementsFiltered()
    {
        Player player = CreatePlayer(insight: 5, rapport: 2);
        OrPath path = new OrPath
        {
            InsightRequired = 3,  // Satisfied
            RapportRequired = 5   // Not satisfied
        };

        PathProjection projection = path.GetProjection(player, CreateGameWorld());

        Assert.Single(projection.MissingRequirements);
        Assert.Contains("Rapport", projection.MissingRequirements[0].Label);
    }

    // ============================================
    // COMPOUND REQUIREMENT PROJECTION TESTS
    // ============================================

    [Fact]
    public void CompoundRequirement_GetProjection_NoRequirements_ReturnsNoRequirements()
    {
        Player player = CreatePlayer();
        CompoundRequirement req = new CompoundRequirement();

        RequirementProjection projection = req.GetProjection(player, CreateGameWorld());

        Assert.False(projection.HasRequirements);
        Assert.True(projection.IsSatisfied);
    }

    [Fact]
    public void CompoundRequirement_GetProjection_ReturnsSatisfiedWhenAnyPathMet()
    {
        Player player = CreatePlayer(insight: 5);
        CompoundRequirement req = new CompoundRequirement
        {
            OrPaths = new List<OrPath>
            {
                new OrPath { RapportRequired = 10 }, // Not met
                new OrPath { InsightRequired = 3 }   // Met
            }
        };

        RequirementProjection projection = req.GetProjection(player, CreateGameWorld());

        Assert.True(projection.HasRequirements);
        Assert.True(projection.IsSatisfied);
        Assert.Equal(2, projection.Paths.Count);
    }

    // ============================================
    // HELPERS
    // ============================================

    private Player CreatePlayer(
        int insight = 0,
        int rapport = 0,
        int authority = 0,
        int diplomacy = 0,
        int cunning = 0,
        int resolve = 0,
        int coins = 0)
    {
        return new Player
        {
            Insight = insight,
            Rapport = rapport,
            Authority = authority,
            Diplomacy = diplomacy,
            Cunning = cunning,
            Resolve = resolve,
            Coins = coins,
            Health = 100,
            Stamina = 100,
            Focus = 100,
            MaxHealth = 100,
            MaxStamina = 100,
            MaxFocus = 100,
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

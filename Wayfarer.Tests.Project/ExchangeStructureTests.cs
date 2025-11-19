using Xunit;

namespace Wayfarer.Tests;

/// <summary>
/// Tests for ExchangeCostStructure and ExchangeRewardStructure after Dictionary → List refactoring.
/// Validates that List<TokenCount> pattern maintains correct business logic.
/// </summary>
public class ExchangeStructureTests
{
    [Fact]
    public void MeetsTokenRequirements_EmptyRequirements_ReturnsTrue()
    {
        ExchangeCostStructure cost = new ExchangeCostStructure
        {
            TokenRequirements = new List<TokenCount>()
        };

        List<TokenCount> playerTokens = new List<TokenCount>
        {
            new TokenCount { Type = ConnectionType.Trust, Count = 5 }
        };

        bool result = cost.MeetsTokenRequirements(playerTokens);

        Assert.True(result, "Empty token requirements should always be met");
    }

    [Fact]
    public void MeetsTokenRequirements_PlayerHasExactly_ReturnsTrue()
    {
        ExchangeCostStructure cost = new ExchangeCostStructure
        {
            TokenRequirements = new List<TokenCount>
            {
                new TokenCount { Type = ConnectionType.Trust, Count = 3 }
            }
        };

        List<TokenCount> playerTokens = new List<TokenCount>
        {
            new TokenCount { Type = ConnectionType.Trust, Count = 3 }
        };

        bool result = cost.MeetsTokenRequirements(playerTokens);

        Assert.True(result, "Player with exactly required tokens should meet requirements");
    }

    [Fact]
    public void MeetsTokenRequirements_PlayerHasMore_ReturnsTrue()
    {
        ExchangeCostStructure cost = new ExchangeCostStructure
        {
            TokenRequirements = new List<TokenCount>
            {
                new TokenCount { Type = ConnectionType.Trust, Count = 3 }
            }
        };

        List<TokenCount> playerTokens = new List<TokenCount>
        {
            new TokenCount { Type = ConnectionType.Trust, Count = 5 }
        };

        bool result = cost.MeetsTokenRequirements(playerTokens);

        Assert.True(result, "Player with more than required tokens should meet requirements");
    }

    [Fact]
    public void MeetsTokenRequirements_PlayerHasInsufficient_ReturnsFalse()
    {
        ExchangeCostStructure cost = new ExchangeCostStructure
        {
            TokenRequirements = new List<TokenCount>
            {
                new TokenCount { Type = ConnectionType.Trust, Count = 5 }
            }
        };

        List<TokenCount> playerTokens = new List<TokenCount>
        {
            new TokenCount { Type = ConnectionType.Trust, Count = 3 }
        };

        bool result = cost.MeetsTokenRequirements(playerTokens);

        Assert.False(result, "Player with insufficient tokens should NOT meet requirements");
    }

    [Fact]
    public void MeetsTokenRequirements_PlayerMissingTokenType_ReturnsFalse()
    {
        ExchangeCostStructure cost = new ExchangeCostStructure
        {
            TokenRequirements = new List<TokenCount>
            {
                new TokenCount { Type = ConnectionType.Trust, Count = 3 }
            }
        };

        List<TokenCount> playerTokens = new List<TokenCount>
        {
            new TokenCount { Type = ConnectionType.Diplomacy, Count = 5 }
        };

        bool result = cost.MeetsTokenRequirements(playerTokens);

        Assert.False(result, "Player missing required token type should NOT meet requirements");
    }

    [Fact]
    public void MeetsTokenRequirements_MultipleRequirements_AllMet_ReturnsTrue()
    {
        ExchangeCostStructure cost = new ExchangeCostStructure
        {
            TokenRequirements = new List<TokenCount>
            {
                new TokenCount { Type = ConnectionType.Trust, Count = 2 },
                new TokenCount { Type = ConnectionType.Diplomacy, Count = 1 }
            }
        };

        List<TokenCount> playerTokens = new List<TokenCount>
        {
            new TokenCount { Type = ConnectionType.Trust, Count = 3 },
            new TokenCount { Type = ConnectionType.Diplomacy, Count = 2 }
        };

        bool result = cost.MeetsTokenRequirements(playerTokens);

        Assert.True(result, "Player meeting all multiple requirements should pass");
    }

    [Fact]
    public void MeetsTokenRequirements_MultipleRequirements_OneMissing_ReturnsFalse()
    {
        ExchangeCostStructure cost = new ExchangeCostStructure
        {
            TokenRequirements = new List<TokenCount>
            {
                new TokenCount { Type = ConnectionType.Trust, Count = 2 },
                new TokenCount { Type = ConnectionType.Diplomacy, Count = 3 }
            }
        };

        List<TokenCount> playerTokens = new List<TokenCount>
        {
            new TokenCount { Type = ConnectionType.Trust, Count = 3 },
            new TokenCount { Type = ConnectionType.Diplomacy, Count = 1 }
        };

        bool result = cost.MeetsTokenRequirements(playerTokens);

        Assert.False(result, "Player missing one requirement should fail even if others met");
    }

    [Fact]
    public void CanAfford_SufficientResources_ReturnsTrue()
    {
        ExchangeCostStructure cost = new ExchangeCostStructure
        {
            Resources = new List<ResourceAmount>
            {
                new ResourceAmount { Type = ResourceType.Coins, Amount = 10 }
            }
        };

        PlayerResourceState playerResources = new PlayerResourceState
        {
            Coins = 15
        };

        bool result = cost.CanAfford(playerResources);

        Assert.True(result, "Player with sufficient resources should be able to afford");
    }

    [Fact]
    public void CanAfford_InsufficientCoins_ReturnsFalse()
    {
        ExchangeCostStructure cost = new ExchangeCostStructure
        {
            Resources = new List<ResourceAmount>
            {
                new ResourceAmount { Type = ResourceType.Coins, Amount = 20 }
            }
        };

        PlayerResourceState playerResources = new PlayerResourceState
        {
            Coins = 10
        };

        bool result = cost.CanAfford(playerResources);

        Assert.False(result, "Player with insufficient coins should NOT be able to afford");
    }

    [Fact]
    public void GetDescription_EmptyCosts_ReturnsFree()
    {
        ExchangeCostStructure cost = new ExchangeCostStructure
        {
            Resources = new List<ResourceAmount>(),
            ConsumedItemIds = new List<string>()
        };

        string description = cost.GetDescription();

        Assert.Equal("Free", description);
    }

    [Fact]
    public void GetDescription_WithResources_ReturnsFormattedString()
    {
        ExchangeCostStructure cost = new ExchangeCostStructure
        {
            Resources = new List<ResourceAmount>
            {
                new ResourceAmount { Type = ResourceType.Coins, Amount = 10 }
            }
        };

        string description = cost.GetDescription();

        Assert.Contains("10", description);
        Assert.Contains("Coins", description);
    }

    [Fact]
    public void RewardGetDescription_EmptyRewards_ReturnsNothing()
    {
        ExchangeRewardStructure reward = new ExchangeRewardStructure
        {
            Resources = new List<ResourceAmount>(),
            ItemIds = new List<string>(),
            Tokens = new List<TokenCount>()
        };

        string description = reward.GetDescription();

        Assert.Equal("Nothing", description);
    }

    [Fact]
    public void RewardGetDescription_WithTokens_ReturnsFormattedString()
    {
        ExchangeRewardStructure reward = new ExchangeRewardStructure
        {
            Tokens = new List<TokenCount>
            {
                new TokenCount { Type = ConnectionType.Trust, Count = 2 }
            }
        };

        string description = reward.GetDescription();

        Assert.Contains("2", description);
        Assert.Contains("Trust", description);
        Assert.Contains("Token", description);
    }

    [Fact]
    public void RewardHasRewards_WithTokens_ReturnsTrue()
    {
        ExchangeRewardStructure reward = new ExchangeRewardStructure
        {
            Tokens = new List<TokenCount>
            {
                new TokenCount { Type = ConnectionType.Trust, Count = 1 }
            }
        };

        bool result = reward.HasRewards();

        Assert.True(result, "Reward with tokens should return true for HasRewards");
    }

    [Fact]
    public void RewardHasRewards_Empty_ReturnsFalse()
    {
        ExchangeRewardStructure reward = new ExchangeRewardStructure
        {
            Resources = new List<ResourceAmount>(),
            ItemIds = new List<string>(),
            Tokens = new List<TokenCount>(),
            EffectIds = new List<string>()
        };

        bool result = reward.HasRewards();

        Assert.False(result, "Empty reward should return false for HasRewards");
    }
}

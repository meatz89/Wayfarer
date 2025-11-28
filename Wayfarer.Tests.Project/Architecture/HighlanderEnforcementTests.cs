using System.Reflection;
using Xunit;

namespace Wayfarer.Tests.Architecture;

/// <summary>
/// HIGHLANDER ENFORCEMENT TESTS
/// Verifies that only approved classes handle resource values.
/// ADR-018: HIGHLANDER Resource Value Classes
///
/// TWO APPROVED CLASSES:
/// 1. Consequence - ALL resource OUTCOMES (costs as negative, rewards as positive)
/// 2. CompoundRequirement.OrPath - ALL resource PREREQUISITES
///
/// ALL OTHER CLASSES WITH RESOURCE PROPERTIES ARE FORBIDDEN.
/// </summary>
public class HighlanderEnforcementTests
{
    /// <summary>
    /// Verify that ChoiceReward class no longer exists.
    /// HIGHLANDER: Consequence is the ONLY class for rewards.
    /// </summary>
    [Fact]
    public void ChoiceReward_ShouldNotExist()
    {
        Type choiceRewardType = GetTypeByName("ChoiceReward");
        Assert.Null(choiceRewardType);
    }

    /// <summary>
    /// Verify that ChoiceCost class no longer exists.
    /// HIGHLANDER: Consequence is the ONLY class for costs.
    /// </summary>
    [Fact]
    public void ChoiceCost_ShouldNotExist()
    {
        Type choiceCostType = GetTypeByName("ChoiceCost");
        Assert.Null(choiceCostType);
    }

    /// <summary>
    /// Verify that ActionCosts class no longer exists.
    /// HIGHLANDER: Consequence handles atmospheric action costs.
    /// </summary>
    [Fact]
    public void ActionCosts_ShouldNotExist()
    {
        Type actionCostsType = GetTypeByName("ActionCosts");
        Assert.Null(actionCostsType);
    }

    /// <summary>
    /// Verify that ActionRewards class no longer exists.
    /// HIGHLANDER: Consequence handles atmospheric action rewards.
    /// </summary>
    [Fact]
    public void ActionRewards_ShouldNotExist()
    {
        Type actionRewardsType = GetTypeByName("ActionRewards");
        Assert.Null(actionRewardsType);
    }

    /// <summary>
    /// Verify that SituationCosts class no longer exists.
    /// HIGHLANDER: Consequence handles situation entry costs (as EntryCost property).
    /// </summary>
    [Fact]
    public void SituationCosts_ShouldNotExist()
    {
        Type situationCostsType = GetTypeByName("SituationCosts");
        Assert.Null(situationCostsType);
    }

    /// <summary>
    /// Verify that Consequence class exists and is the approved resource outcome class.
    /// </summary>
    [Fact]
    public void Consequence_ShouldExist_WithResourceProperties()
    {
        Type consequenceType = GetTypeByName("Consequence");
        Assert.NotNull(consequenceType);

        // Verify it has the key resource properties
        Assert.NotNull(consequenceType.GetProperty("Coins"));
        Assert.NotNull(consequenceType.GetProperty("Health"));
        Assert.NotNull(consequenceType.GetProperty("Stamina"));
        Assert.NotNull(consequenceType.GetProperty("Focus"));
        Assert.NotNull(consequenceType.GetProperty("Resolve"));
        Assert.NotNull(consequenceType.GetProperty("Hunger"));
    }

    /// <summary>
    /// Verify that CompoundRequirement.OrPath exists and is the approved prerequisite class.
    /// </summary>
    [Fact]
    public void OrPath_ShouldExist_WithPrerequisiteProperties()
    {
        Type orPathType = GetTypeByName("OrPath");
        Assert.NotNull(orPathType);

        // Verify it has the key prerequisite properties
        Assert.NotNull(orPathType.GetProperty("CoinsRequired"));
        Assert.NotNull(orPathType.GetProperty("ResolveRequired"));
        Assert.NotNull(orPathType.GetProperty("FocusRequired"));
        Assert.NotNull(orPathType.GetProperty("StaminaRequired"));
    }

    /// <summary>
    /// Verify that LocationAction uses Consequence (not ActionCosts/ActionRewards).
    /// </summary>
    [Fact]
    public void LocationAction_ShouldUseConsequence()
    {
        Type locationActionType = GetTypeByName("LocationAction");
        Assert.NotNull(locationActionType);

        // Should have Consequence property
        PropertyInfo consequenceProperty = locationActionType.GetProperty("Consequence");
        Assert.NotNull(consequenceProperty);

        // Should NOT have Costs or Rewards properties
        Assert.Null(locationActionType.GetProperty("Costs"));
        Assert.Null(locationActionType.GetProperty("Rewards"));
    }

    /// <summary>
    /// Verify that PlayerAction uses Consequence (not ActionCosts/ActionRewards).
    /// </summary>
    [Fact]
    public void PlayerAction_ShouldUseConsequence()
    {
        Type playerActionType = GetTypeByName("PlayerAction");
        Assert.NotNull(playerActionType);

        // Should have Consequence property
        PropertyInfo consequenceProperty = playerActionType.GetProperty("Consequence");
        Assert.NotNull(consequenceProperty);

        // Should NOT have Costs or Rewards properties
        Assert.Null(playerActionType.GetProperty("Costs"));
        Assert.Null(playerActionType.GetProperty("Rewards"));
    }

    /// <summary>
    /// Verify that Situation uses EntryCost (Consequence, not SituationCosts).
    /// </summary>
    [Fact]
    public void Situation_ShouldUseEntryCost()
    {
        Type situationType = GetTypeByName("Situation");
        Assert.NotNull(situationType);

        // Should have EntryCost property of type Consequence
        PropertyInfo entryCostProperty = situationType.GetProperty("EntryCost");
        Assert.NotNull(entryCostProperty);

        Type consequenceType = GetTypeByName("Consequence");
        Assert.Equal(consequenceType, entryCostProperty.PropertyType);

        // Should NOT have Costs property
        Assert.Null(situationType.GetProperty("Costs"));
    }

    /// <summary>
    /// Verify Consequence sign convention: negative = cost, positive = reward.
    /// </summary>
    [Fact]
    public void Consequence_SignConvention_NegativeIsCost()
    {
        Consequence cost = new Consequence { Coins = -50, Health = -10 };

        Assert.True(cost.HasAnyCosts());
        Assert.False(cost.HasAnyRewards());
    }

    /// <summary>
    /// Verify Consequence sign convention: positive = reward.
    /// </summary>
    [Fact]
    public void Consequence_SignConvention_PositiveIsReward()
    {
        Consequence reward = new Consequence { Coins = 100, Health = 5 };

        Assert.False(reward.HasAnyCosts());
        Assert.True(reward.HasAnyRewards());
    }

    /// <summary>
    /// Helper method to find a type by name in the loaded assemblies.
    /// </summary>
    private Type GetTypeByName(string typeName)
    {
        // Search in the Wayfarer assembly (main project)
        Assembly wayfarerAssembly = Assembly.Load("Wayfarer");

        foreach (Type type in wayfarerAssembly.GetTypes())
        {
            if (type.Name == typeName)
                return type;
        }

        return null;
    }
}

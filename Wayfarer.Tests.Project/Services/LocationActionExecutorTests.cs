using Xunit;

namespace Wayfarer.Tests.Services;

/// <summary>
/// Comprehensive tests for LocationActionExecutor (atmospheric action validation).
/// Tests fallback scene validation logic: Travel, Work, Rest, IntraVenueMove.
/// MANDATORY per CLAUDE.md: Business logic requires complete test coverage.
/// </summary>
public class LocationActionExecutorTests
{
    private readonly LocationActionExecutor _executor;

    public LocationActionExecutorTests()
    {
        _executor = new LocationActionExecutor();
    }

    // ========== VALIDATATEANDEXTRACT (LOCATIONACTION) TESTS ==========

    [Fact]
    public void ValidateAndExtract_SufficientResources_ReturnsValidPlan()
    {
        // Arrange: Player with resources, action with costs
        Player player = CreateTestPlayer(coins: 10, stamina: 5, focus: 3, health: 8);
        LocationAction action = CreateAtmosphericAction(
            coinCost: 5,
            staminaCost: 2,
            focusCost: 1,
            healthCost: 0
        );

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(action, player);

        // Assert
        Assert.True(plan.IsValid);
        Assert.True(plan.CoinsCost > 0);
        Assert.True(plan.StaminaCost > 0);
        Assert.True(plan.FocusCost > 0);
        Assert.Equal(0, plan.HealthCost);
        Assert.True(plan.IsAtmosphericAction);
        Assert.NotNull(plan.Consequence); // HIGHLANDER: Consequence replaces DirectRewards
    }

    [Fact]
    public void ValidateAndExtract_InsufficientCoins_ReturnsInvalid()
    {
        // BEHAVIOR: Cannot execute action when coins insufficient
        Player player = CreateTestPlayer(coins: 3, stamina: 10, focus: 10, health: 10);
        LocationAction action = CreateAtmosphericAction(coinCost: 5);

        ActionExecutionPlan plan = _executor.ValidateAndExtract(action, player);

        Assert.False(plan.IsValid);
        Assert.NotNull(plan.FailureReason);
    }

    [Fact]
    public void ValidateAndExtract_InsufficientStamina_ReturnsInvalid()
    {
        // BEHAVIOR: Cannot execute action when stamina insufficient
        Player player = CreateTestPlayer(coins: 10, stamina: 1, focus: 10, health: 10);
        LocationAction action = CreateAtmosphericAction(staminaCost: 3);

        ActionExecutionPlan plan = _executor.ValidateAndExtract(action, player);

        Assert.False(plan.IsValid);
        Assert.NotNull(plan.FailureReason);
    }

    [Fact]
    public void ValidateAndExtract_InsufficientFocus_ReturnsInvalid()
    {
        // BEHAVIOR: Cannot execute action when focus insufficient
        Player player = CreateTestPlayer(coins: 10, stamina: 10, focus: 0, health: 10);
        LocationAction action = CreateAtmosphericAction(focusCost: 2);

        ActionExecutionPlan plan = _executor.ValidateAndExtract(action, player);

        Assert.False(plan.IsValid);
        Assert.NotNull(plan.FailureReason);
    }

    [Fact]
    public void ValidateAndExtract_InsufficientHealth_ReturnsInvalid()
    {
        // BEHAVIOR: Cannot execute action when health insufficient
        Player player = CreateTestPlayer(coins: 10, stamina: 10, focus: 10, health: 2);
        LocationAction action = CreateAtmosphericAction(healthCost: 3);

        ActionExecutionPlan plan = _executor.ValidateAndExtract(action, player);

        Assert.False(plan.IsValid);
        Assert.NotNull(plan.FailureReason);
    }

    [Fact]
    public void ValidateAndExtract_FreeAction_ReturnsValidPlan()
    {
        // Arrange: Travel action with zero costs (free atmospheric action)
        Player player = CreateTestPlayer(coins: 0, stamina: 0, focus: 0, health: 1);
        LocationAction action = CreateAtmosphericAction(
            coinCost: 0,
            staminaCost: 0,
            focusCost: 0,
            healthCost: 0
        );

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(action, player);

        // Assert
        Assert.True(plan.IsValid);
        Assert.Equal(0, plan.CoinsCost);
        Assert.Equal(0, plan.StaminaCost);
        Assert.Equal(0, plan.FocusCost);
        Assert.Equal(0, plan.HealthCost);
        Assert.True(plan.IsAtmosphericAction);
    }

    [Fact]
    public void ValidateAndExtract_ExactResources_ReturnsValidPlan()
    {
        // Arrange: Player has exactly the required resources (boundary case)
        Player player = CreateTestPlayer(coins: 5, stamina: 2, focus: 1, health: 3);
        LocationAction action = CreateAtmosphericAction(
            coinCost: 5,
            staminaCost: 2,
            focusCost: 1,
            healthCost: 3
        );

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(action, player);

        // Assert
        Assert.True(plan.IsValid);
    }

    [Fact]
    public void ValidateAndExtract_Consequence_SetCorrectly()
    {
        // Arrange: Action with rewards (positive values in Consequence)
        // HIGHLANDER: Consequence is the ONLY class for resource outcomes
        Player player = CreateTestPlayer(coins: 10, stamina: 10, focus: 10, health: 10);
        LocationAction action = CreateAtmosphericAction(coinReward: 8);

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(action, player);

        // Assert
        Assert.True(plan.IsValid);
        Assert.NotNull(plan.Consequence);
        Assert.True(plan.Consequence.Coins > 0); // Positive = reward
    }

    [Fact]
    public void ValidateAndExtract_ActionName_SetCorrectly()
    {
        // Arrange
        Player player = CreateTestPlayer(coins: 10, stamina: 10, focus: 10, health: 10);
        LocationAction action = CreateAtmosphericAction(name: "Work");

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(action, player);

        // Assert
        Assert.True(plan.IsValid);
        Assert.Equal("Work", plan.ActionName);
    }

    [Fact]
    public void ValidateAndExtract_TimeRequired_SetCorrectly()
    {
        // Arrange: Rest action costs 1 time segment
        Player player = CreateTestPlayer(coins: 10, stamina: 10, focus: 10, health: 10);
        LocationAction action = CreateAtmosphericAction(timeRequired: 1);

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(action, player);

        // Assert
        Assert.True(plan.IsValid);
        Assert.True(plan.TimeSegments > 0);
    }

    [Fact]
    public void ValidateAndExtract_ActionType_AlwaysInstant()
    {
        // Arrange: Atmospheric actions are always instant
        Player player = CreateTestPlayer(coins: 10, stamina: 10, focus: 10, health: 10);
        LocationAction action = CreateAtmosphericAction();

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(action, player);

        // Assert
        Assert.True(plan.IsValid);
        Assert.Equal(ChoiceActionType.Instant, plan.ActionType);
    }

    // ========== VALIDATEATMOSPHERICPATHCARD TESTS ==========

    [Fact]
    public void ValidateAtmosphericPathCard_SufficientResources_ReturnsValidPlan()
    {
        // Arrange: Player with resources, PathCard with costs
        Player player = CreateTestPlayer(coins: 10, stamina: 5);
        PathCard card = CreateAtmosphericPathCard(
            coinRequirement: 5,
            staminaCost: 3,
            travelTimeSegments: 2
        );

        // Act
        ActionExecutionPlan plan = _executor.ValidateAtmosphericPathCard(card, player);

        // Assert
        Assert.True(plan.IsValid);
        Assert.True(plan.CoinsCost > 0);
        Assert.True(plan.StaminaCost > 0);
        Assert.True(plan.TimeSegments > 0);
        Assert.True(plan.IsAtmosphericAction);
    }

    [Fact]
    public void ValidateAtmosphericPathCard_InsufficientCoins_ReturnsInvalid()
    {
        // BEHAVIOR: Cannot execute action when coins insufficient
        Player player = CreateTestPlayer(coins: 2, stamina: 10);
        PathCard card = CreateAtmosphericPathCard(coinRequirement: 5);

        ActionExecutionPlan plan = _executor.ValidateAtmosphericPathCard(card, player);

        Assert.False(plan.IsValid);
        Assert.NotNull(plan.FailureReason);
    }

    [Fact]
    public void ValidateAtmosphericPathCard_InsufficientStamina_ReturnsInvalid()
    {
        // BEHAVIOR: Cannot execute action when stamina insufficient
        Player player = CreateTestPlayer(coins: 10, stamina: 1);
        PathCard card = CreateAtmosphericPathCard(staminaCost: 4);

        ActionExecutionPlan plan = _executor.ValidateAtmosphericPathCard(card, player);

        Assert.False(plan.IsValid);
        Assert.NotNull(plan.FailureReason);
    }

    [Fact]
    public void ValidateAtmosphericPathCard_MissingPermit_ReturnsInvalid()
    {
        // BEHAVIOR: Cannot execute action when permit missing
        Player player = CreateTestPlayer(coins: 10, stamina: 10);
        Item requiredPermit = CreateTestItem("Travel Permit");
        PathCard card = CreateAtmosphericPathCard(permitRequirement: requiredPermit);

        ActionExecutionPlan plan = _executor.ValidateAtmosphericPathCard(card, player);

        Assert.False(plan.IsValid);
        Assert.NotNull(plan.FailureReason);
    }

    [Fact]
    public void ValidateAtmosphericPathCard_HasPermit_ReturnsValidPlan()
    {
        // Arrange: Player has required permit
        Item permit = CreateTestItem("Travel Permit");
        Player player = CreateTestPlayer(coins: 10, stamina: 10);
        player.Inventory.Add(permit);

        PathCard card = CreateAtmosphericPathCard(permitRequirement: permit);

        // Act
        ActionExecutionPlan plan = _executor.ValidateAtmosphericPathCard(card, player);

        // Assert
        Assert.True(plan.IsValid);
    }

    [Fact]
    public void ValidateAtmosphericPathCard_FreePathCard_ReturnsValidPlan()
    {
        // Arrange: PathCard with zero costs (free route)
        Player player = CreateTestPlayer(coins: 0, stamina: 0);
        PathCard card = CreateAtmosphericPathCard(
            coinRequirement: 0,
            staminaCost: 0,
            travelTimeSegments: 1
        );

        // Act
        ActionExecutionPlan plan = _executor.ValidateAtmosphericPathCard(card, player);

        // Assert
        Assert.True(plan.IsValid);
        Assert.Equal(0, plan.CoinsCost);
        Assert.Equal(0, plan.StaminaCost);
    }

    [Fact]
    public void ValidateAtmosphericPathCard_HungerEffect_SetCorrectly()
    {
        // Arrange: PathCard increases hunger by 5
        Player player = CreateTestPlayer(coins: 10, stamina: 10);
        PathCard card = CreateAtmosphericPathCard(hungerEffect: 5);

        // Act
        ActionExecutionPlan plan = _executor.ValidateAtmosphericPathCard(card, player);

        // Assert
        Assert.True(plan.IsValid);
        Assert.True(plan.HungerCost > 0);
    }

    [Fact]
    public void ValidateAtmosphericPathCard_CardName_SetCorrectly()
    {
        // Arrange
        Player player = CreateTestPlayer(coins: 10, stamina: 10);
        PathCard card = CreateAtmosphericPathCard(name: "Mountain Pass");

        // Act
        ActionExecutionPlan plan = _executor.ValidateAtmosphericPathCard(card, player);

        // Assert
        Assert.True(plan.IsValid);
        Assert.Equal("Mountain Pass", plan.ActionName);
    }

    [Fact]
    public void ValidateAtmosphericPathCard_ActionType_AlwaysInstant()
    {
        // Arrange: Atmospheric PathCards execute instantly
        Player player = CreateTestPlayer(coins: 10, stamina: 10);
        PathCard card = CreateAtmosphericPathCard();

        // Act
        ActionExecutionPlan plan = _executor.ValidateAtmosphericPathCard(card, player);

        // Assert
        Assert.True(plan.IsValid);
        Assert.Equal(ChoiceActionType.Instant, plan.ActionType);
    }

    // ========== HELPER METHODS ==========

    /// <summary>
    /// Create test player with specified resources
    /// </summary>
    private Player CreateTestPlayer(int coins = 10, int stamina = 10, int focus = 10, int health = 10)
    {
        Player player = new Player
        {
            Coins = coins,
            Stamina = stamina,
            MaxStamina = 10,
            Focus = focus,
            MaxFocus = 10,
            Health = health,
            MaxHealth = 10,
            Hunger = 0,
            MaxHunger = 100
        };

        return player;
    }

    /// <summary>
    /// Create atmospheric LocationAction (no ChoiceTemplate)
    /// HIGHLANDER: Uses Consequence with negative values for costs
    /// </summary>
    private LocationAction CreateAtmosphericAction(
        string name = "Test Action",
        int coinCost = 0,
        int staminaCost = 0,
        int focusCost = 0,
        int healthCost = 0,
        int timeRequired = 0,
        int coinReward = 0)
    {
        return new LocationAction
        {
            Name = name,
            Consequence = new Consequence
            {
                // HIGHLANDER: Negative values = costs, Positive values = rewards
                Coins = coinReward - coinCost,
                Stamina = -staminaCost,
                Focus = -focusCost,
                Health = -healthCost
            },
            TimeRequired = timeRequired,
            ChoiceTemplate = null  // ATMOSPHERIC PATTERN
        };
    }

    /// <summary>
    /// Create atmospheric PathCard (no ChoiceTemplate)
    /// </summary>
    private PathCard CreateAtmosphericPathCard(
        string name = "Test Route",
        int coinRequirement = 0,
        int staminaCost = 0,
        int travelTimeSegments = 1,
        int hungerEffect = 0,
        Item permitRequirement = null)
    {
        return new PathCard
        {
            Name = name,
            CoinRequirement = coinRequirement,
            StaminaCost = staminaCost,
            TravelTimeSegments = travelTimeSegments,
            HungerEffect = hungerEffect,
            PermitRequirement = permitRequirement,
            // DOMAIN COLLECTION PRINCIPLE: Explicit stat properties default to 0
            ChoiceTemplate = null  // ATMOSPHERIC PATTERN
        };
    }

    /// <summary>
    /// Create test item (for permit requirements)
    /// </summary>
    private Item CreateTestItem(string name)
    {
        return new Item
        {
            Name = name,
            Description = $"Test item: {name}"
        };
    }
}

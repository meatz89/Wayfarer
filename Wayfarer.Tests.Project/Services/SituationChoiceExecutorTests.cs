using Xunit;

namespace Wayfarer.Tests.Services;

/// <summary>
/// Comprehensive tests for SituationChoiceExecutor (ChoiceTemplate validation).
/// Tests scene-based action validation logic with complex requirements/costs/rewards.
/// MANDATORY per CLAUDE.md: Business logic requires complete test coverage.
/// HIGHLANDER: This is the single source of truth for ChoiceTemplate validation.
/// </summary>
public class SituationChoiceExecutorTests
{
    private readonly SituationChoiceExecutor _executor;
    private readonly GameWorld _gameWorld;

    public SituationChoiceExecutorTests()
    {
        _executor = new SituationChoiceExecutor();
        _gameWorld = CreateTestGameWorld();
    }

    // ========== VALIDATEANDEXTRACT (CHOICETEMPLATE) TESTS ==========

    [Fact]
    public void ValidateAndExtract_SufficientResources_ReturnsValidPlan()
    {
        // Arrange: Player with resources, template with costs
        Player player = CreateTestPlayer(resolve: 10, coins: 20, health: 5, stamina: 3, focus: 2);
        ChoiceTemplate template = CreateChoiceTemplate(
            resolveCost: 5,
            coinCost: 10,
            healthCost: 2,
            staminaCost: 1,
            focusCost: 1
        );

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action", player, _gameWorld);

        // Assert
        Assert.True(plan.IsValid);
        Assert.Equal(5, plan.ResolveCoins);
        Assert.Equal(10, plan.CoinsCost);
        Assert.Equal(2, plan.HealthCost);
        Assert.Equal(1, plan.StaminaCost);
        Assert.Equal(1, plan.FocusCost);
        Assert.False(plan.IsAtmosphericAction);  // Scene-based action
    }

    [Fact]
    public void ValidateAndExtract_RequirementNotMet_ReturnsInvalid()
    {
        // Arrange: Player doesn't meet requirement (Resolve < 15)
        Player player = CreateTestPlayer(resolve: 10);
        CompoundRequirement requirement = CreateCompoundRequirement(
            requirementType: "Resolve",
            threshold: 15
        );
        ChoiceTemplate template = CreateChoiceTemplate(requirement: requirement);

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action", player, _gameWorld);

        // Assert
        Assert.False(plan.IsValid);
        Assert.Equal("Requirements not met", plan.FailureReason);
    }

    [Fact]
    public void ValidateAndExtract_InsufficientResolve_ReturnsInvalid()
    {
        // Arrange: Player with 3 resolve, template costs 5 resolve
        Player player = CreateTestPlayer(resolve: 3);
        ChoiceTemplate template = CreateChoiceTemplate(resolveCost: 5);

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action", player, _gameWorld);

        // Assert
        Assert.False(plan.IsValid);
        Assert.Contains("Not enough Resolve", plan.FailureReason);
        Assert.Contains("need 5", plan.FailureReason);
        Assert.Contains("have 3", plan.FailureReason);
    }

    [Fact]
    public void ValidateAndExtract_InsufficientCoins_ReturnsInvalid()
    {
        // Arrange: Player with 5 coins, template costs 10 coins
        Player player = CreateTestPlayer(resolve: 10, coins: 5);
        ChoiceTemplate template = CreateChoiceTemplate(coinCost: 10);

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action", player, _gameWorld);

        // Assert
        Assert.False(plan.IsValid);
        Assert.Contains("Not enough Coins", plan.FailureReason);
        Assert.Contains("need 10", plan.FailureReason);
        Assert.Contains("have 5", plan.FailureReason);
    }

    [Fact]
    public void ValidateAndExtract_InsufficientHealth_ReturnsInvalid()
    {
        // Arrange: Player with 2 health, template costs 4 health
        Player player = CreateTestPlayer(resolve: 10, health: 2);
        ChoiceTemplate template = CreateChoiceTemplate(healthCost: 4);

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action", player, _gameWorld);

        // Assert
        Assert.False(plan.IsValid);
        Assert.Contains("Not enough Health", plan.FailureReason);
        Assert.Contains("need 4", plan.FailureReason);
        Assert.Contains("have 2", plan.FailureReason);
    }

    [Fact]
    public void ValidateAndExtract_InsufficientStamina_ReturnsInvalid()
    {
        // Arrange: Player with 1 stamina, template costs 3 stamina
        Player player = CreateTestPlayer(resolve: 10, stamina: 1);
        ChoiceTemplate template = CreateChoiceTemplate(staminaCost: 3);

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action", player, _gameWorld);

        // Assert
        Assert.False(plan.IsValid);
        Assert.Contains("Not enough Stamina", plan.FailureReason);
        Assert.Contains("need 3", plan.FailureReason);
        Assert.Contains("have 1", plan.FailureReason);
    }

    [Fact]
    public void ValidateAndExtract_InsufficientFocus_ReturnsInvalid()
    {
        // Arrange: Player with 0 focus, template costs 2 focus
        Player player = CreateTestPlayer(resolve: 10, focus: 0);
        ChoiceTemplate template = CreateChoiceTemplate(focusCost: 2);

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action", player, _gameWorld);

        // Assert
        Assert.False(plan.IsValid);
        Assert.Contains("Not enough Focus", plan.FailureReason);
        Assert.Contains("need 2", plan.FailureReason);
        Assert.Contains("have 0", plan.FailureReason);
    }

    [Fact]
    public void ValidateAndExtract_HungerWouldExceedMax_ReturnsInvalid()
    {
        // Arrange: Player at 95 hunger (max 100), template adds 10 hunger
        Player player = CreateTestPlayer(resolve: 10, hunger: 95, maxHunger: 100);
        ChoiceTemplate template = CreateChoiceTemplate(hungerCost: 10);

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action", player, _gameWorld);

        // Assert
        Assert.False(plan.IsValid);
        Assert.Contains("Too hungry to continue", plan.FailureReason);
        Assert.Contains("current 95", plan.FailureReason);
        Assert.Contains("action adds 10", plan.FailureReason);
        Assert.Contains("max 100", plan.FailureReason);
    }

    [Fact]
    public void ValidateAndExtract_HungerAtMax_ReturnsInvalid()
    {
        // Arrange: Player at exactly max hunger, template adds any hunger
        Player player = CreateTestPlayer(resolve: 10, hunger: 100, maxHunger: 100);
        ChoiceTemplate template = CreateChoiceTemplate(hungerCost: 1);

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action", player, _gameWorld);

        // Assert
        Assert.False(plan.IsValid);
        Assert.Contains("Too hungry to continue", plan.FailureReason);
    }

    [Fact]
    public void ValidateAndExtract_NullRequirementFormula_ReturnsValidPlan()
    {
        // Arrange: Template with no requirements (null RequirementFormula)
        Player player = CreateTestPlayer(resolve: 10);
        ChoiceTemplate template = CreateChoiceTemplate(requirement: null);

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action", player, _gameWorld);

        // Assert: Should pass validation (no requirements to check)
        Assert.True(plan.IsValid);
    }

    [Fact]
    public void ValidateAndExtract_EmptyOrPaths_ReturnsValidPlan()
    {
        // Arrange: Template with empty OrPaths (no requirements)
        Player player = CreateTestPlayer(resolve: 10);
        CompoundRequirement emptyRequirement = new CompoundRequirement
        {
            OrPaths = new List<OrPath>()  // Empty list
        };
        ChoiceTemplate template = CreateChoiceTemplate(requirement: emptyRequirement);

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action", player, _gameWorld);

        // Assert: Should pass validation (empty OrPaths = no requirements)
        Assert.True(plan.IsValid);
    }

    [Fact]
    public void ValidateAndExtract_ExactResources_ReturnsValidPlan()
    {
        // Arrange: Player has exactly the required resources (boundary case)
        Player player = CreateTestPlayer(resolve: 5, coins: 10, health: 2, stamina: 3, focus: 1);
        ChoiceTemplate template = CreateChoiceTemplate(
            resolveCost: 5,
            coinCost: 10,
            healthCost: 2,
            staminaCost: 3,
            focusCost: 1
        );

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action", player, _gameWorld);

        // Assert
        Assert.True(plan.IsValid);
    }

    [Fact]
    public void ValidateAndExtract_ConsequenceRewards_SetCorrectly()
    {
        // Arrange: Template with rewards (positive values in Consequence)
        Player player = CreateTestPlayer(resolve: 10);
        ChoiceTemplate template = CreateChoiceTemplate(
            coinReward: 15,
            resolveReward: 3
        );

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action", player, _gameWorld);

        // Assert
        Assert.True(plan.IsValid);
        Assert.NotNull(plan.Consequence);
        Assert.Equal(15, plan.Consequence.Coins);
        Assert.Equal(3, plan.Consequence.Resolve);
    }

    [Fact]
    public void ValidateAndExtract_ActionName_SetCorrectly()
    {
        // Arrange
        Player player = CreateTestPlayer(resolve: 10);
        ChoiceTemplate template = CreateChoiceTemplate();

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Investigate Scene", player, _gameWorld);

        // Assert
        Assert.True(plan.IsValid);
        Assert.Equal("Investigate Scene", plan.ActionName);
    }

    [Fact]
    public void ValidateAndExtract_IsAtmosphericAction_AlwaysFalse()
    {
        // Arrange: Scene-based actions are NEVER atmospheric
        Player player = CreateTestPlayer(resolve: 10);
        ChoiceTemplate template = CreateChoiceTemplate();

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action", player, _gameWorld);

        // Assert
        Assert.True(plan.IsValid);
        Assert.False(plan.IsAtmosphericAction);  // Scene-based, not fallback scene
    }

    [Fact]
    public void ValidateAndExtract_ActionType_SetFromTemplate()
    {
        // Arrange: Template with StartChallenge action type
        Player player = CreateTestPlayer(resolve: 10);
        ChoiceTemplate template = CreateChoiceTemplate(actionType: ChoiceActionType.StartChallenge);

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action", player, _gameWorld);

        // Assert
        Assert.True(plan.IsValid);
        Assert.Equal(ChoiceActionType.StartChallenge, plan.ActionType);
    }

    [Fact]
    public void ValidateAndExtract_ChallengeType_SetFromTemplate()
    {
        // Arrange: Template with Social challenge type
        Player player = CreateTestPlayer(resolve: 10);
        ChoiceTemplate template = CreateChoiceTemplate(
            actionType: ChoiceActionType.StartChallenge,
            challengeType: TacticalSystemType.Social
        );

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action", player, _gameWorld);

        // Assert
        Assert.True(plan.IsValid);
        Assert.Equal(TacticalSystemType.Social, plan.ChallengeType);
    }

    [Fact]
    public void ValidateAndExtract_ChallengeId_SetFromTemplate()
    {
        // Arrange: Template with challenge ID
        Player player = CreateTestPlayer(resolve: 10);
        ChoiceTemplate template = CreateChoiceTemplate(
            actionType: ChoiceActionType.StartChallenge,
            challengeId: "persuasion_challenge_01"
        );

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action", player, _gameWorld);

        // Assert
        Assert.True(plan.IsValid);
        Assert.Equal("persuasion_challenge_01", plan.ChallengeId);
    }

    [Fact]
    public void ValidateAndExtract_NavigationPayload_SetFromTemplate()
    {
        // Arrange: Template with navigation payload
        Player player = CreateTestPlayer(resolve: 10);
        Location destination = CreateTestLocation("Test Location");
        NavigationPayload payload = new NavigationPayload
        {
            Destination = destination,
            AutoTriggerScene = true
        };
        ChoiceTemplate template = CreateChoiceTemplate(
            actionType: ChoiceActionType.Navigate,
            navigationPayload: payload
        );

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action", player, _gameWorld);

        // Assert
        Assert.True(plan.IsValid);
        Assert.NotNull(plan.NavigationPayload);
        Assert.Equal(destination, plan.NavigationPayload.Destination);
        Assert.True(plan.NavigationPayload.AutoTriggerScene);
    }

    [Fact]
    public void ValidateAndExtract_TimeSegments_SetFromTemplate()
    {
        // Arrange: Template with time cost
        Player player = CreateTestPlayer(resolve: 10);
        ChoiceTemplate template = CreateChoiceTemplate(timeSegments: 3);

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action", player, _gameWorld);

        // Assert
        Assert.True(plan.IsValid);
        Assert.Equal(3, plan.TimeSegments);
    }

    [Fact]
    public void ValidateAndExtract_MultipleCostsAtOnce_ValidatesAll()
    {
        // Arrange: Template with multiple costs, player has sufficient resources
        Player player = CreateTestPlayer(
            resolve: 10,
            coins: 50,
            health: 8,
            stamina: 5,
            focus: 3,
            hunger: 20,
            maxHunger: 100
        );
        ChoiceTemplate template = CreateChoiceTemplate(
            resolveCost: 8,
            coinCost: 30,
            healthCost: 3,
            staminaCost: 4,
            focusCost: 2,
            hungerCost: 15,
            timeSegments: 2
        );

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action", player, _gameWorld);

        // Assert: All costs validated, plan is valid
        Assert.True(plan.IsValid);
        Assert.Equal(8, plan.ResolveCoins);
        Assert.Equal(30, plan.CoinsCost);
        Assert.Equal(3, plan.HealthCost);
        Assert.Equal(4, plan.StaminaCost);
        Assert.Equal(2, plan.FocusCost);
        Assert.Equal(15, plan.HungerCost);
        Assert.Equal(2, plan.TimeSegments);
    }

    [Fact]
    public void ValidateAndExtract_FreeAction_ReturnsValidPlan()
    {
        // Arrange: Template with zero costs (free scene action)
        Player player = CreateTestPlayer(resolve: 0, coins: 0);
        ChoiceTemplate template = CreateChoiceTemplate(
            resolveCost: 0,
            coinCost: 0,
            healthCost: 0,
            staminaCost: 0,
            focusCost: 0,
            hungerCost: 0
        );

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action", player, _gameWorld);

        // Assert
        Assert.True(plan.IsValid);
        Assert.Equal(0, plan.ResolveCoins);
        Assert.Equal(0, plan.CoinsCost);
    }

    // ========== HELPER METHODS ==========

    /// <summary>
    /// Create test player with specified resources
    /// </summary>
    private Player CreateTestPlayer(
        int resolve = 10,
        int coins = 100,
        int health = 10,
        int stamina = 10,
        int focus = 10,
        int hunger = 0,
        int maxHunger = 100)
    {
        Player player = new Player
        {
            Resolve = resolve,
            Coins = coins,
            Health = health,
            MaxHealth = 10,
            Stamina = stamina,
            MaxStamina = 10,
            Focus = focus,
            MaxFocus = 10,
            Hunger = hunger,
            MaxHunger = maxHunger
        };

        return player;
    }

    /// <summary>
    /// Create test GameWorld (required for CompoundRequirement evaluation)
    /// </summary>
    private GameWorld CreateTestGameWorld()
    {
        return new GameWorld();
    }

    /// <summary>
    /// Create ChoiceTemplate with specified costs and requirements
    /// DESIGN: Costs are NEGATIVE values in Consequence (Coins = -5 means pay 5)
    /// DESIGN: Rewards are POSITIVE values in Consequence (Coins = 10 means gain 10)
    /// </summary>
    private ChoiceTemplate CreateChoiceTemplate(
        CompoundRequirement requirement = null,
        int resolveCost = 0,
        int coinCost = 0,
        int healthCost = 0,
        int staminaCost = 0,
        int focusCost = 0,
        int hungerCost = 0,
        int timeSegments = 0,
        int resolveReward = 0,
        int coinReward = 0,
        ChoiceActionType actionType = ChoiceActionType.Instant,
        TacticalSystemType? challengeType = null,
        string challengeId = null,
        NavigationPayload navigationPayload = null)
    {
        return new ChoiceTemplate
        {
            Id = "test_choice_template",
            ActionTextTemplate = "Test Action",
            RequirementFormula = requirement,
            Consequence = new Consequence
            {
                Resolve = resolveReward - resolveCost,  // Negative = cost, positive = reward
                Coins = coinReward - coinCost,
                Health = -healthCost,
                Stamina = -staminaCost,
                Focus = -focusCost,
                Hunger = hungerCost,  // Positive hunger IS the cost (increases hunger)
                TimeSegments = timeSegments
            },
            ActionType = actionType,
            ChallengeType = challengeType,
            ChallengeId = challengeId,
            NavigationPayload = navigationPayload
        };
    }

    /// <summary>
    /// Create CompoundRequirement with single OR path containing one requirement
    /// Uses explicit OrPath properties per the Explicit Property Principle
    /// </summary>
    private CompoundRequirement CreateCompoundRequirement(
        string requirementType = "Resolve",
        int threshold = 10)
    {
        OrPath path = new OrPath { Label = $"{requirementType} {threshold}+" };

        switch (requirementType)
        {
            case "Resolve": path.ResolveRequired = threshold; break;
            case "Insight": path.InsightRequired = threshold; break;
            case "Rapport": path.RapportRequired = threshold; break;
            case "Authority": path.AuthorityRequired = threshold; break;
            case "Diplomacy": path.DiplomacyRequired = threshold; break;
            case "Cunning": path.CunningRequired = threshold; break;
            case "Coins": path.CoinsRequired = threshold; break;
            default: path.ResolveRequired = threshold; break;
        }

        return new CompoundRequirement { OrPaths = new List<OrPath> { path } };
    }

    /// <summary>
    /// Create test location for navigation tests
    /// </summary>
    private Location CreateTestLocation(string name)
    {
        return new Location(name)
        {
            Description = $"Test location: {name}"
        };
    }
}

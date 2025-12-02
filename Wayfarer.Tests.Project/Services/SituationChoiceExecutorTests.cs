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
        _gameWorld = new GameWorld();
        _executor = new SituationChoiceExecutor(_gameWorld);
    }

    // ========== VALIDATEANDEXTRACT (CHOICETEMPLATE) TESTS ==========

    [Fact]
    public void ValidateAndExtract_SufficientResources_ReturnsValidPlan()
    {
        // Arrange: Player with resources, template with costs
        SetupPlayer(resolve: 10, coins: 20, health: 5, stamina: 3, focus: 2);
        ChoiceTemplate template = CreateChoiceTemplate(
            resolveCost: 5,
            coinCost: 10,
            healthCost: 2,
            staminaCost: 1,
            focusCost: 1
        );

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action");

        // Assert
        Assert.True(plan.IsValid);
        Assert.True(plan.ResolveCoins > 0);
        Assert.True(plan.CoinsCost > 0);
        Assert.True(plan.HealthCost > 0);
        Assert.True(plan.StaminaCost > 0);
        Assert.True(plan.FocusCost > 0);
        Assert.False(plan.IsAtmosphericAction);
    }

    [Fact]
    public void ValidateAndExtract_RequirementNotMet_ReturnsInvalid()
    {
        // Arrange: Player doesn't meet requirement (Resolve < 15)
        SetupPlayer(resolve: 10);
        CompoundRequirement requirement = CreateCompoundRequirement(
            requirementType: "Resolve",
            threshold: 15
        );
        ChoiceTemplate template = CreateChoiceTemplate(requirement: requirement);

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action");

        // Assert
        Assert.False(plan.IsValid);
        Assert.Equal("Requirements not met", plan.FailureReason);
    }

    [Fact]
    public void ValidateAndExtract_ResolveNotCheckedForAffordability_SirBrantePattern()
    {
        // SIR BRANTE WILLPOWER PATTERN: Resolve uses gate logic (>= 0), NOT affordability (>= cost)
        // Player with 3 resolve, template costs 5 resolve
        // This should be VALID because Resolve is not checked for affordability
        // The gate check (Resolve >= 0) is handled by RequirementFormula, not here
        // See arc42/08 §8.20 and ADR-017
        SetupPlayer(resolve: 3);
        ChoiceTemplate template = CreateChoiceTemplate(resolveCost: 5);

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action");

        // Assert: Should be VALID - Resolve is not checked for affordability
        Assert.True(plan.IsValid);
    }

    [Fact]
    public void ValidateAndExtract_InsufficientCoins_ReturnsInvalid()
    {
        // BEHAVIOR: Cannot execute action when coins insufficient
        SetupPlayer(resolve: 10, coins: 5);
        ChoiceTemplate template = CreateChoiceTemplate(coinCost: 10);

        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action");

        Assert.False(plan.IsValid);
    }

    [Fact]
    public void ValidateAndExtract_InsufficientHealth_ReturnsInvalid()
    {
        // BEHAVIOR: Cannot execute action when health insufficient
        SetupPlayer(resolve: 10, health: 2);
        ChoiceTemplate template = CreateChoiceTemplate(healthCost: 4);

        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action");

        Assert.False(plan.IsValid);
    }

    [Fact]
    public void ValidateAndExtract_InsufficientStamina_ReturnsInvalid()
    {
        // BEHAVIOR: Cannot execute action when stamina insufficient
        SetupPlayer(resolve: 10, stamina: 1);
        ChoiceTemplate template = CreateChoiceTemplate(staminaCost: 3);

        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action");

        Assert.False(plan.IsValid);
    }

    [Fact]
    public void ValidateAndExtract_InsufficientFocus_ReturnsInvalid()
    {
        // BEHAVIOR: Cannot execute action when focus insufficient
        SetupPlayer(resolve: 10, focus: 0);
        ChoiceTemplate template = CreateChoiceTemplate(focusCost: 2);

        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action");

        Assert.False(plan.IsValid);
    }

    [Fact]
    public void ValidateAndExtract_HungerWouldExceedMax_ReturnsInvalid()
    {
        // BEHAVIOR: Cannot execute action when hunger would exceed max
        SetupPlayer(resolve: 10, hunger: 95, maxHunger: 100);
        ChoiceTemplate template = CreateChoiceTemplate(hungerCost: 10);

        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action");

        Assert.False(plan.IsValid);
    }

    [Fact]
    public void ValidateAndExtract_HungerAtMax_ReturnsInvalid()
    {
        // BEHAVIOR: Cannot execute action when already at max hunger
        SetupPlayer(resolve: 10, hunger: 100, maxHunger: 100);
        ChoiceTemplate template = CreateChoiceTemplate(hungerCost: 1);

        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action");

        Assert.False(plan.IsValid);
    }

    [Fact]
    public void ValidateAndExtract_NullRequirementFormula_ReturnsValidPlan()
    {
        // Arrange: Template with no requirements (null RequirementFormula)
        SetupPlayer(resolve: 10);
        ChoiceTemplate template = CreateChoiceTemplate(requirement: null);

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action");

        // Assert: Should pass validation (no requirements to check)
        Assert.True(plan.IsValid);
    }

    [Fact]
    public void ValidateAndExtract_EmptyOrPaths_ReturnsValidPlan()
    {
        // Arrange: Template with empty OrPaths (no requirements)
        SetupPlayer(resolve: 10);
        CompoundRequirement emptyRequirement = new CompoundRequirement
        {
            OrPaths = new List<OrPath>()  // Empty list
        };
        ChoiceTemplate template = CreateChoiceTemplate(requirement: emptyRequirement);

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action");

        // Assert: Should pass validation (empty OrPaths = no requirements)
        Assert.True(plan.IsValid);
    }

    [Fact]
    public void ValidateAndExtract_ExactResources_ReturnsValidPlan()
    {
        // Arrange: Player has exactly the required resources (boundary case)
        SetupPlayer(resolve: 5, coins: 10, health: 2, stamina: 3, focus: 1);
        ChoiceTemplate template = CreateChoiceTemplate(
            resolveCost: 5,
            coinCost: 10,
            healthCost: 2,
            staminaCost: 3,
            focusCost: 1
        );

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action");

        // Assert
        Assert.True(plan.IsValid);
    }

    [Fact]
    public void ValidateAndExtract_ConsequenceRewards_SetCorrectly()
    {
        // Arrange: Template with rewards (positive values in Consequence)
        SetupPlayer(resolve: 10);
        ChoiceTemplate template = CreateChoiceTemplate(
            coinReward: 15,
            resolveReward: 3
        );

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action");

        // Assert
        Assert.True(plan.IsValid);
        Assert.NotNull(plan.Consequence);
        Assert.True(plan.Consequence.Coins > 0);
        Assert.True(plan.Consequence.Resolve > 0);
    }

    [Fact]
    public void ValidateAndExtract_ActionName_SetCorrectly()
    {
        // Arrange
        SetupPlayer(resolve: 10);
        ChoiceTemplate template = CreateChoiceTemplate();

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Investigate Scene");

        // Assert
        Assert.True(plan.IsValid);
        Assert.Equal("Investigate Scene", plan.ActionName);
    }

    [Fact]
    public void ValidateAndExtract_IsAtmosphericAction_AlwaysFalse()
    {
        // Arrange: Scene-based actions are NEVER atmospheric
        SetupPlayer(resolve: 10);
        ChoiceTemplate template = CreateChoiceTemplate();

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action");

        // Assert
        Assert.True(plan.IsValid);
        Assert.False(plan.IsAtmosphericAction);  // Scene-based, not fallback scene
    }

    [Fact]
    public void ValidateAndExtract_ActionType_SetFromTemplate()
    {
        // Arrange: Template with StartChallenge action type
        SetupPlayer(resolve: 10);
        ChoiceTemplate template = CreateChoiceTemplate(actionType: ChoiceActionType.StartChallenge);

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action");

        // Assert
        Assert.True(plan.IsValid);
        Assert.Equal(ChoiceActionType.StartChallenge, plan.ActionType);
    }

    [Fact]
    public void ValidateAndExtract_ChallengeType_SetFromTemplate()
    {
        // Arrange: Template with Social challenge type
        SetupPlayer(resolve: 10);
        ChoiceTemplate template = CreateChoiceTemplate(
            actionType: ChoiceActionType.StartChallenge,
            challengeType: TacticalSystemType.Social
        );

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action");

        // Assert
        Assert.True(plan.IsValid);
        Assert.Equal(TacticalSystemType.Social, plan.ChallengeType);
    }

    [Fact]
    public void ValidateAndExtract_ChallengeId_SetFromTemplate()
    {
        // Arrange: Template with challenge ID
        SetupPlayer(resolve: 10);
        ChoiceTemplate template = CreateChoiceTemplate(
            actionType: ChoiceActionType.StartChallenge,
            challengeId: "persuasion_challenge_01"
        );

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action");

        // Assert
        Assert.True(plan.IsValid);
        Assert.Equal("persuasion_challenge_01", plan.ChallengeId);
    }

    [Fact]
    public void ValidateAndExtract_NavigationPayload_SetFromTemplate()
    {
        // Arrange: Template with navigation payload
        SetupPlayer(resolve: 10);
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
        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action");

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
        SetupPlayer(resolve: 10);
        ChoiceTemplate template = CreateChoiceTemplate(timeSegments: 3);

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action");

        // Assert
        Assert.True(plan.IsValid);
        Assert.Equal(3, plan.TimeSegments);
    }

    [Fact]
    public void ValidateAndExtract_MultipleCostsAtOnce_ValidatesAll()
    {
        // Arrange: Template with multiple costs, player has sufficient resources
        SetupPlayer(
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
        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action");

        // Assert: All costs validated, plan is valid
        Assert.True(plan.IsValid);
        Assert.True(plan.ResolveCoins > 0);
        Assert.True(plan.CoinsCost > 0);
        Assert.True(plan.HealthCost > 0);
        Assert.True(plan.StaminaCost > 0);
        Assert.True(plan.FocusCost > 0);
        Assert.True(plan.HungerCost > 0);
        Assert.True(plan.TimeSegments > 0);
    }

    [Fact]
    public void ValidateAndExtract_FreeAction_ReturnsValidPlan()
    {
        // Arrange: Template with zero costs (free scene action)
        SetupPlayer(resolve: 0, coins: 0);
        ChoiceTemplate template = CreateChoiceTemplate(
            resolveCost: 0,
            coinCost: 0,
            healthCost: 0,
            staminaCost: 0,
            focusCost: 0,
            hungerCost: 0
        );

        // Act
        ActionExecutionPlan plan = _executor.ValidateAndExtract(template, "Test Action");

        // Assert
        Assert.True(plan.IsValid);
        Assert.Equal(0, plan.ResolveCoins);
        Assert.Equal(0, plan.CoinsCost);
    }

    // ========== HELPER METHODS ==========

    /// <summary>
    /// Setup player in GameWorld with specified resources.
    /// Player accessed via _gameWorld.GetPlayer() (never passed as parameter).
    /// </summary>
    private Player SetupPlayer(
        int resolve = 10,
        int coins = 100,
        int health = 10,
        int stamina = 10,
        int focus = 10,
        int hunger = 0,
        int maxHunger = 100)
    {
        Player player = _gameWorld.GetPlayer();
        player.Resolve = resolve;
        player.Coins = coins;
        player.Health = health;
        player.MaxHealth = 10;
        player.Stamina = stamina;
        player.MaxStamina = 10;
        player.Focus = focus;
        player.MaxFocus = 10;
        player.Hunger = hunger;
        player.MaxHunger = maxHunger;
        return player;
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

/// <summary>
/// LocationActionExecutor - Validator for ATMOSPHERIC (fallback scene) LocationActions
/// NO FACADE DEPENDENCIES - Returns ActionExecutionPlan for GameOrchestrator to execute
/// FALLBACK SCENE ARCHITECTURE: Validates default/baseline actions (Travel, Work, Rest)
/// Scene-based actions validated by SituationChoiceExecutor (HIGHLANDER)
/// </summary>
public class LocationActionExecutor
{
    private readonly GameWorld _gameWorld;

    public LocationActionExecutor(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }

    /// <summary>
    /// Validate atmospheric LocationAction and extract execution plan
    /// FALLBACK SCENE: These are permanent actions that prevent soft-locks
    /// Examples: Travel, Work, Rest, IntraVenueMove
    /// GameOrchestrator applies the plan via facades
    ///
    /// HIGHLANDER: Consequence is the ONLY class for resource outcomes.
    /// Negative values = costs, Positive values = rewards.
    /// Player accessed via _gameWorld.GetPlayer() (never passed as parameter).
    /// </summary>
    public ActionExecutionPlan ValidateAndExtract(LocationAction action)
    {
        Player player = _gameWorld.GetPlayer();
        // Extract costs from Consequence (negative values become positive cost amounts)
        int coinsCost = action.Consequence.Coins < 0 ? -action.Consequence.Coins : 0;
        int staminaCost = action.Consequence.Stamina < 0 ? -action.Consequence.Stamina : 0;
        int focusCost = action.Consequence.Focus < 0 ? -action.Consequence.Focus : 0;
        int healthCost = action.Consequence.Health < 0 ? -action.Consequence.Health : 0;

        // STEP 1: Validate costs (atmospheric actions have no requirements, only costs)
        if (player.Coins < coinsCost)
        {
            return ActionExecutionPlan.Invalid($"Not enough Coins (need {coinsCost}, have {player.Coins})");
        }

        if (player.Stamina < staminaCost)
        {
            return ActionExecutionPlan.Invalid($"Not enough Stamina (need {staminaCost}, have {player.Stamina})");
        }

        if (player.Focus < focusCost)
        {
            return ActionExecutionPlan.Invalid($"Not enough Focus (need {focusCost}, have {player.Focus})");
        }

        if (player.Health < healthCost)
        {
            return ActionExecutionPlan.Invalid($"Not enough Health (need {healthCost}, have {player.Health})");
        }

        // STEP 2: Build execution plan
        ActionExecutionPlan plan = ActionExecutionPlan.Valid();
        plan.CoinsCost = coinsCost;
        plan.StaminaCost = staminaCost;
        plan.FocusCost = focusCost;
        plan.HealthCost = healthCost;
        plan.TimeSegments = action.TimeRequired;
        plan.Consequence = action.Consequence;  // HIGHLANDER: Unified consequence
        plan.ActionType = ChoiceActionType.Instant;  // Atmospheric actions are always instant
        plan.ActionName = action.Name;
        plan.IsAtmosphericAction = true;  // Atmospheric pattern (fallback scene)

        return plan;
    }

    /// <summary>
    /// Validate ATMOSPHERIC PathCard (route card with direct costs/rewards)
    /// FALLBACK SCENE: Static route path cards defined in route JSON files
    /// PathCards follow same dual-pattern as LocationActions
    /// Player accessed via _gameWorld.GetPlayer() (never passed as parameter).
    /// </summary>
    public ActionExecutionPlan ValidateAtmosphericPathCard(PathCard card)
    {
        Player player = _gameWorld.GetPlayer();
        // STEP 1: Validate costs (atmospheric PathCards have no requirements, only costs)
        if (player.Coins < card.CoinRequirement)
        {
            return ActionExecutionPlan.Invalid($"Not enough Coins (need {card.CoinRequirement}, have {player.Coins})");
        }

        if (player.Stamina < card.StaminaCost)
        {
            return ActionExecutionPlan.Invalid($"Not enough Stamina (need {card.StaminaCost}, have {player.Stamina})");
        }

        // STEP 2: Validate permit requirement
        if (card.PermitRequirement != null && !player.Inventory.Contains(card.PermitRequirement))
        {
            return ActionExecutionPlan.Invalid($"Missing required permit: {card.PermitRequirement.Name}");
        }

        // STEP 3: Validate stat requirements
        // foreach (var statReq in card.StatRequirements)
        // {
        //     int playerStatValue = GetPlayerStat(player, statReq.Key);
        //     if (playerStatValue < statReq.Value)
        //     {
        //         return ActionExecutionPlan.Invalid($"Not enough {statReq.Key} (need {statReq.Value}, have {playerStatValue})");
        //     }
        // }

        // STEP 4: Build execution plan
        ActionExecutionPlan plan = ActionExecutionPlan.Valid();
        plan.CoinsCost = card.CoinRequirement;
        plan.StaminaCost = card.StaminaCost;
        plan.TimeSegments = card.TravelTimeSegments;
        plan.HungerCost = card.HungerEffect;  // PathCards can increase hunger

        // PathCard rewards (CoinReward, StaminaRestore, HealthEffect, etc.)
        // Note: PathCard uses individual reward properties (specific to travel mechanic)
        // GameOrchestrator handles these specifically for PathCard execution
        plan.ActionType = ChoiceActionType.Instant;  // Atmospheric path cards execute instantly
        plan.ActionName = card.Name;
        plan.IsAtmosphericAction = true;  // Atmospheric pattern (fallback scene)

        return plan;
    }
}

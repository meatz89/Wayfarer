/// <summary>
/// LocationActionExecutor - Validator for ATMOSPHERIC (fallback scene) LocationActions
/// NO FACADE DEPENDENCIES - Returns ActionExecutionPlan for GameFacade to execute
/// FALLBACK SCENE ARCHITECTURE: Validates default/baseline actions (Travel, Work, Rest)
/// Scene-based actions validated by SituationChoiceExecutor (HIGHLANDER)
/// </summary>
public class LocationActionExecutor
{
    /// <summary>
    /// Validate atmospheric LocationAction and extract execution plan
    /// FALLBACK SCENE: These are permanent actions that prevent soft-locks
    /// Examples: Travel, Work, Rest, IntraVenueMove
    /// GameFacade applies the plan via facades
    /// </summary>
    public ActionExecutionPlan ValidateAndExtract(LocationAction action, Player player)
    {
        // STEP 1: Validate costs (atmospheric actions have no requirements, only costs)
        if (player.Coins < action.Costs.Coins)
        {
            return ActionExecutionPlan.Invalid($"Not enough Coins (need {action.Costs.Coins}, have {player.Coins})");
        }

        if (player.Stamina < action.Costs.Stamina)
        {
            return ActionExecutionPlan.Invalid($"Not enough Stamina (need {action.Costs.Stamina}, have {player.Stamina})");
        }

        if (player.Focus < action.Costs.Focus)
        {
            return ActionExecutionPlan.Invalid($"Not enough Focus (need {action.Costs.Focus}, have {player.Focus})");
        }

        if (player.Health < action.Costs.Health)
        {
            return ActionExecutionPlan.Invalid($"Not enough Health (need {action.Costs.Health}, have {player.Health})");
        }

        // STEP 2: Build execution plan
        ActionExecutionPlan plan = ActionExecutionPlan.Valid();
        plan.CoinsCost = action.Costs.Coins;
        plan.StaminaCost = action.Costs.Stamina;
        plan.FocusCost = action.Costs.Focus;
        plan.HealthCost = action.Costs.Health;
        plan.TimeSegments = action.TimeRequired;
        plan.DirectRewards = action.Rewards;  // Atmospheric rewards
        plan.ActionType = ChoiceActionType.Instant;  // Atmospheric actions are always instant
        plan.ActionName = action.Name;
        plan.IsAtmosphericAction = true;  // Atmospheric pattern (fallback scene)

        return plan;
    }

    /// <summary>
    /// Validate ATMOSPHERIC PathCard (route card with direct costs/rewards)
    /// FALLBACK SCENE: Static route path cards defined in route JSON files
    /// PathCards follow same dual-pattern as LocationActions
    /// </summary>
    public ActionExecutionPlan ValidateAtmosphericPathCard(PathCard card, Player player)
    {
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
        // TODO: Implement stat validation (Player uses individual properties not dictionary)
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
        // Note: PathCard uses individual reward properties, not ActionRewards object
        // GameFacade handles these specifically for PathCard execution
        plan.ActionType = ChoiceActionType.Instant;  // Atmospheric path cards execute instantly
        plan.ActionName = card.Name;
        plan.IsAtmosphericAction = true;  // Atmospheric pattern (fallback scene)

        return plan;
    }
}

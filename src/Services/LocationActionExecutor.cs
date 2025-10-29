using Wayfarer.GameState.Enums;

namespace Wayfarer.Services;

/// <summary>
/// LocationActionExecutor - PURE validator/extractor for LocationActions
/// NO FACADE DEPENDENCIES - Returns ActionExecutionPlan for GameFacade to execute
/// </summary>
public class LocationActionExecutor
{
    /// <summary>
    /// Validate LocationAction and extract execution plan
    /// GameFacade applies the plan via facades
    /// </summary>
    public ActionExecutionPlan ValidateAndExtract(LocationAction action, Player player, GameWorld gameWorld)
    {
        // UNIFIED ARCHITECTURE: ChoiceTemplate vs direct properties
        if (action.ChoiceTemplate != null)
        {
            return ValidateChoiceTemplate(action.ChoiceTemplate, action.Name, player, gameWorld);
        }
        else
        {
            return ValidateLegacyAction(action, player);
        }
    }

    private ActionExecutionPlan ValidateChoiceTemplate(ChoiceTemplate template, string actionName, Player player, GameWorld gameWorld)
    {
        // STEP 1: Validate CompoundRequirements
        if (template.RequirementFormula != null && template.RequirementFormula.OrPaths.Count > 0)
        {
            bool requirementsMet = template.RequirementFormula.IsAnySatisfied(player, gameWorld);
            if (!requirementsMet)
            {
                return ActionExecutionPlan.Invalid("Requirements not met");
            }
        }

        // STEP 2: Validate strategic costs
        if (player.Resolve < template.CostTemplate.Resolve)
        {
            return ActionExecutionPlan.Invalid($"Not enough Resolve (need {template.CostTemplate.Resolve}, have {player.Resolve})");
        }

        if (player.Coins < template.CostTemplate.Coins)
        {
            return ActionExecutionPlan.Invalid($"Not enough Coins (need {template.CostTemplate.Coins}, have {player.Coins})");
        }

        // STEP 3: Build execution plan
        ActionExecutionPlan plan = ActionExecutionPlan.Valid();
        plan.ResolveCoins = template.CostTemplate.Resolve;
        plan.CoinsCost = template.CostTemplate.Coins;
        plan.TimeSegments = template.CostTemplate.TimeSegments;
        plan.ChoiceReward = template.RewardTemplate;
        plan.ActionType = template.ActionType;
        plan.ChallengeType = template.ChallengeType;
        plan.ChallengeId = template.ChallengeId;
        plan.NavigationPayload = template.NavigationPayload;
        plan.ActionName = actionName;
        plan.IsLegacyAction = false;

        return plan;
    }

    private ActionExecutionPlan ValidateLegacyAction(LocationAction action, Player player)
    {
        // STEP 1: Validate strategic costs (legacy actions have no requirements)
        if (player.Coins < action.Costs.Coins)
        {
            return ActionExecutionPlan.Invalid($"Not enough Coins (need {action.Costs.Coins}, have {player.Coins})");
        }

        // STEP 2: Build execution plan
        ActionExecutionPlan plan = ActionExecutionPlan.Valid();
        plan.CoinsCost = action.Costs.Coins;
        plan.TimeSegments = action.TimeRequired;
        plan.LegacyRewards = action.Rewards;
        plan.ActionType = ChoiceActionType.Instant; // Legacy actions are always instant
        plan.ActionName = action.Name;
        plan.IsLegacyAction = true;

        return plan;
    }
}

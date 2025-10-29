using Wayfarer.GameState.Enums;

namespace Wayfarer.Services;

/// <summary>
/// NPCActionExecutor - PURE validator/extractor for NPCActions
/// NO FACADE DEPENDENCIES - Returns ActionExecutionPlan for GameFacade to execute
/// </summary>
public class NPCActionExecutor
{
    /// <summary>
    /// Validate NPCAction and extract execution plan
    /// GameFacade applies the plan via facades
    /// </summary>
    public ActionExecutionPlan ValidateAndExtract(NPCAction action, Player player, GameWorld gameWorld)
    {
        // UNIFIED ARCHITECTURE: ChoiceTemplate vs direct properties
        if (action.ChoiceTemplate != null)
        {
            return ValidateChoiceTemplate(action.ChoiceTemplate, action.Name, player, gameWorld);
        }
        else
        {
            return ValidateLegacyAction(action);
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

    private ActionExecutionPlan ValidateLegacyAction(NPCAction action)
    {
        // Legacy NPC actions have no costs or requirements (conversations are free)
        ActionExecutionPlan plan = ActionExecutionPlan.Valid();
        plan.ActionType = ChoiceActionType.StartChallenge; // Legacy NPC actions start conversations
        plan.ChallengeType = TacticalSystemType.Social;
        plan.ActionName = action.Name;
        plan.IsLegacyAction = true;

        return plan;
    }
}

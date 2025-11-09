/// <summary>
/// PathCardExecutor - PURE validator/extractor for PathCards
/// NO FACADE DEPENDENCIES - Returns ActionExecutionPlan for GameFacade to execute
/// </summary>
public class PathCardExecutor
{
    /// <summary>
    /// Validate PathCard and extract execution plan
    /// GameFacade applies the plan via facades
    /// </summary>
    public ActionExecutionPlan ValidateAndExtract(PathCard card, Player player, GameWorld gameWorld)
    {
        // UNIFIED ARCHITECTURE: ChoiceTemplate vs direct properties
        if (card.ChoiceTemplate != null)
        {
            return ValidateChoiceTemplate(card.ChoiceTemplate, card.Name, player, gameWorld);
        }
        else
        {
            return ValidateLegacyCard(card, player);
        }
    }

    private ActionExecutionPlan ValidateChoiceTemplate(ChoiceTemplate template, string cardName, Player player, GameWorld gameWorld)
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
        plan.ActionName = cardName;
        plan.IsLegacyAction = false;

        return plan;
    }

    private ActionExecutionPlan ValidateLegacyCard(PathCard card, Player player)
    {
        // STEP 1: Validate requirements
        if (player.Coins < card.CoinRequirement)
        {
            return ActionExecutionPlan.Invalid($"Not enough Coins (need {card.CoinRequirement}, have {player.Coins})");
        }

        // STEP 2: Validate costs (stamina is consumed when path is used)
        if (player.Stamina < card.StaminaCost)
        {
            return ActionExecutionPlan.Invalid($"Not enough Stamina (need {card.StaminaCost}, have {player.Stamina})");
        }

        // STEP 3: Build execution plan
        // Legacy PathCards have direct reward properties (StaminaRestore, HealthEffect, CoinReward, etc.)
        // GameFacade will apply these via TravelFacade or directly
        ActionExecutionPlan plan = ActionExecutionPlan.Valid();
        plan.CoinsCost = card.CoinRequirement;
        plan.TimeSegments = card.TravelTimeSegments;
        plan.ActionType = ChoiceActionType.Instant; // Legacy path cards apply effects instantly
        plan.ActionName = card.Name;
        plan.IsLegacyAction = true;

        return plan;
    }
}

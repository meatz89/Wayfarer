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
        if (card.ChoiceTemplate == null)
        {
            return ActionExecutionPlan.Invalid("PathCard missing ChoiceTemplate");
        }

        return ValidateChoiceTemplate(card.ChoiceTemplate, card.Name, player, gameWorld);
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

        return plan;
    }
}

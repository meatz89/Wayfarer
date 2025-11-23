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
        if (action.ChoiceTemplate == null)
        {
            return ActionExecutionPlan.Invalid("NPCAction missing ChoiceTemplate");
        }

        return ValidateChoiceTemplate(action.ChoiceTemplate, action.Name, player, gameWorld);
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

        // Tutorial resource validation
        if (template.CostTemplate.Health > 0 && player.Health < template.CostTemplate.Health)
        {
            return ActionExecutionPlan.Invalid($"Not enough Health (need {template.CostTemplate.Health}, have {player.Health})");
        }

        if (template.CostTemplate.Stamina > 0 && player.Stamina < template.CostTemplate.Stamina)
        {
            return ActionExecutionPlan.Invalid($"Not enough Stamina (need {template.CostTemplate.Stamina}, have {player.Stamina})");
        }

        if (template.CostTemplate.Focus > 0 && player.Focus < template.CostTemplate.Focus)
        {
            return ActionExecutionPlan.Invalid($"Not enough Focus (need {template.CostTemplate.Focus}, have {player.Focus})");
        }

        // Hunger validation: Check if adding hunger would exceed max (100)
        if (template.CostTemplate.Hunger > 0 && player.Hunger + template.CostTemplate.Hunger > player.MaxHunger)
        {
            return ActionExecutionPlan.Invalid($"Too hungry to continue (current {player.Hunger}, action adds {template.CostTemplate.Hunger}, max {player.MaxHunger})");
        }

        // STEP 3: Build execution plan
        ActionExecutionPlan plan = ActionExecutionPlan.Valid();
        plan.ResolveCoins = template.CostTemplate.Resolve;
        plan.CoinsCost = template.CostTemplate.Coins;
        plan.TimeSegments = template.CostTemplate.TimeSegments;

        // Tutorial resource costs
        plan.HealthCost = template.CostTemplate.Health;
        plan.StaminaCost = template.CostTemplate.Stamina;
        plan.FocusCost = template.CostTemplate.Focus;
        plan.HungerCost = template.CostTemplate.Hunger;

        plan.ChoiceReward = template.RewardTemplate;
        plan.ActionType = template.ActionType;
        plan.ChallengeType = template.ChallengeType;
        plan.ChallengeId = template.ChallengeId;
        plan.NavigationPayload = template.NavigationPayload;
        plan.ActionName = actionName;

        return plan;
    }
}

/// <summary>
/// SituationChoiceExecutor - UNIFIED validator for ALL ChoiceTemplate-based actions
/// Handles scene-based actions from Situations (LocationAction, NPCAction, PathCard with ChoiceTemplate)
/// HIGHLANDER: Single source of truth for ChoiceTemplate validation logic
/// FALLBACK SCENE ARCHITECTURE: Validates "active scene" actions (atmospheric = fallback scene, separate validator)
/// </summary>
public class SituationChoiceExecutor
{
    /// <summary>
    /// Validate ChoiceTemplate and extract execution plan for scene-based actions
    /// Used by: LocationAction (scene-based), NPCAction (all), PathCard (scene-based)
    /// </summary>
    public ActionExecutionPlan ValidateAndExtract(ChoiceTemplate template, string actionName, Player player, GameWorld gameWorld)
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
        plan.IsAtmosphericAction = false;  // Scene-based action (not fallback scene)

        return plan;
    }
}

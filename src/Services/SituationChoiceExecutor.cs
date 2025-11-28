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

        // STEP 2: Validate strategic costs (extract from Consequence - costs are NEGATIVE)
        // NOTE: Resolve is NOT validated here - Sir Brante Willpower Pattern.
        // Resolve uses gate logic (>= 0) via CompoundRequirement.CreateForConsequence(),
        // not affordability (>= cost). Players CAN go negative on Resolve.
        // See arc42/08 §8.20 for documentation.
        int resolveCost = template.Consequence.Resolve < 0 ? -template.Consequence.Resolve : 0;
        int coinsCost = template.Consequence.Coins < 0 ? -template.Consequence.Coins : 0;

        // Resolve intentionally NOT validated - Sir Brante pattern allows going negative
        if (player.Coins < coinsCost)
        {
            return ActionExecutionPlan.Invalid($"Not enough Coins (need {coinsCost}, have {player.Coins})");
        }

        // Tutorial resource validation (costs are NEGATIVE in Consequence)
        int healthCost = template.Consequence.Health < 0 ? -template.Consequence.Health : 0;
        int staminaCost = template.Consequence.Stamina < 0 ? -template.Consequence.Stamina : 0;
        int focusCost = template.Consequence.Focus < 0 ? -template.Consequence.Focus : 0;
        int hungerCost = template.Consequence.Hunger > 0 ? template.Consequence.Hunger : 0; // Positive hunger is a cost

        if (healthCost > 0 && player.Health < healthCost)
        {
            return ActionExecutionPlan.Invalid($"Not enough Health (need {healthCost}, have {player.Health})");
        }

        if (staminaCost > 0 && player.Stamina < staminaCost)
        {
            return ActionExecutionPlan.Invalid($"Not enough Stamina (need {staminaCost}, have {player.Stamina})");
        }

        if (focusCost > 0 && player.Focus < focusCost)
        {
            return ActionExecutionPlan.Invalid($"Not enough Focus (need {focusCost}, have {player.Focus})");
        }

        // Hunger validation: Check if adding hunger would exceed max (100)
        if (hungerCost > 0 && player.Hunger + hungerCost > player.MaxHunger)
        {
            return ActionExecutionPlan.Invalid($"Too hungry to continue (current {player.Hunger}, action adds {hungerCost}, max {player.MaxHunger})");
        }

        // STEP 3: Build execution plan
        ActionExecutionPlan plan = ActionExecutionPlan.Valid();
        plan.ResolveCoins = resolveCost;
        plan.CoinsCost = coinsCost;
        plan.TimeSegments = template.Consequence.TimeSegments;

        // Tutorial resource costs
        plan.HealthCost = healthCost;
        plan.StaminaCost = staminaCost;
        plan.FocusCost = focusCost;
        plan.HungerCost = hungerCost;

        plan.Consequence = template.Consequence;
        plan.ActionType = template.ActionType;
        plan.ChallengeType = template.ChallengeType;
        plan.ChallengeId = template.ChallengeId;
        plan.NavigationPayload = template.NavigationPayload;
        plan.ActionName = actionName;
        plan.IsAtmosphericAction = false;  // Scene-based action (not fallback scene)

        return plan;
    }
}

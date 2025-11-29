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
        // STEP 1: Validate authored CompoundRequirements (stats, items, etc.)
        if (template.RequirementFormula != null && template.RequirementFormula.OrPaths.Count > 0)
        {
            bool requirementsMet = template.RequirementFormula.IsAnySatisfied(player, gameWorld);
            if (!requirementsMet)
            {
                return ActionExecutionPlan.Invalid("Requirements not met");
            }
        }

        // STEP 2: HIGHLANDER - Validate ALL resource availability via OrPath
        // Caller builds OrPath directly - CompoundRequirement stays domain-agnostic
        // Sir Brante pattern: Resolve uses GATE (>= 0), others use AFFORDABILITY (>= cost)
        Consequence consequence = template.Consequence ?? Consequence.None();
        OrPath resourcePath = new OrPath { Label = "Resource Requirements" };
        if (consequence.Resolve < 0) resourcePath.ResolveRequired = 0;  // Gate pattern
        if (consequence.Coins < 0) resourcePath.CoinsRequired = -consequence.Coins;
        if (consequence.Health < 0) resourcePath.HealthRequired = -consequence.Health;
        if (consequence.Stamina < 0) resourcePath.StaminaRequired = -consequence.Stamina;
        if (consequence.Focus < 0) resourcePath.FocusRequired = -consequence.Focus;
        if (consequence.Hunger > 0) resourcePath.HungerCapacityRequired = consequence.Hunger;
        if (!resourcePath.IsSatisfied(player, gameWorld))
        {
            PathProjection projection = resourcePath.GetProjection(player, gameWorld);
            List<string> missing = projection.Requirements
                .Where(r => !r.IsSatisfied)
                .Select(r => $"{r.Label} (have {r.CurrentValue})")
                .ToList();
            return ActionExecutionPlan.Invalid(string.Join(", ", missing));
        }

        // Extract costs for execution plan (costs are NEGATIVE in Consequence)
        int resolveCost = consequence.Resolve < 0 ? -consequence.Resolve : 0;
        int coinsCost = consequence.Coins < 0 ? -consequence.Coins : 0;
        int healthCost = consequence.Health < 0 ? -consequence.Health : 0;
        int staminaCost = consequence.Stamina < 0 ? -consequence.Stamina : 0;
        int focusCost = consequence.Focus < 0 ? -consequence.Focus : 0;
        int hungerCost = consequence.Hunger > 0 ? consequence.Hunger : 0;

        // STEP 3: Build execution plan
        ActionExecutionPlan plan = ActionExecutionPlan.Valid();
        plan.ResolveCoins = resolveCost;
        plan.CoinsCost = coinsCost;
        plan.TimeSegments = consequence.TimeSegments;

        // Tutorial resource costs
        plan.HealthCost = healthCost;
        plan.StaminaCost = staminaCost;
        plan.FocusCost = focusCost;
        plan.HungerCost = hungerCost;

        plan.Consequence = consequence;
        plan.ActionType = template.ActionType;
        plan.ChallengeType = template.ChallengeType;
        plan.ChallengeId = template.ChallengeId;
        plan.NavigationPayload = template.NavigationPayload;
        plan.ActionName = actionName;
        plan.IsAtmosphericAction = false;  // Scene-based action (not fallback scene)

        return plan;
    }
}

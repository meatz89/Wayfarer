/// <summary>
/// SituationChoiceExecutor - UNIFIED validator for ALL ChoiceTemplate-based actions
/// Handles scene-based actions from Situations (LocationAction, NPCAction, PathCard with ChoiceTemplate)
/// HIGHLANDER: Single source of truth for ChoiceTemplate validation logic
/// FALLBACK SCENE ARCHITECTURE: Validates "active scene" actions (atmospheric = fallback scene, separate validator)
///
/// TWO-PHASE SCALING MODEL (arc42 §8.26):
/// Actions may have pre-scaled ScaledRequirement/ScaledConsequence from SceneFacade.
/// If provided, uses scaled values for BOTH validation AND execution.
/// Perfect Information: Display = Execution.
/// </summary>
public class SituationChoiceExecutor
{
    private readonly GameWorld _gameWorld;

    public SituationChoiceExecutor(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }

    /// <summary>
    /// Validate ChoiceTemplate and extract execution plan for scene-based actions
    /// Used by: LocationAction (scene-based), NPCAction (all), PathCard (scene-based)
    ///
    /// TWO-PHASE SCALING: Accepts pre-scaled requirement/consequence (nullable).
    /// If scaledRequirement/scaledConsequence non-null, uses those for validation/execution.
    /// Otherwise falls back to template values (unscaled).
    /// GameWorld accessed via _gameWorld (never passed as parameter).
    /// Player accessed via _gameWorld.GetPlayer() (never passed as parameter).
    /// HIGHLANDER: All parameters required - caller passes scaled values explicitly (even if null)
    /// </summary>
    public ActionExecutionPlan ValidateAndExtract(
        ChoiceTemplate template,
        string actionName,
        CompoundRequirement scaledRequirement,
        Consequence scaledConsequence)
    {
        Player player = _gameWorld.GetPlayer();
        // TWO-PHASE SCALING: Use pre-scaled values if provided, otherwise use template values
        // Perfect Information compliance: display = execution (arc42 §8.26)
        CompoundRequirement effectiveRequirement = scaledRequirement ?? template.RequirementFormula;
        Consequence effectiveConsequence = scaledConsequence ?? template.Consequence ?? Consequence.None();

        // STEP 1: Validate authored CompoundRequirements using EFFECTIVE values (stats, items, etc.)
        if (effectiveRequirement != null && effectiveRequirement.OrPaths.Count > 0)
        {
            bool requirementsMet = effectiveRequirement.IsAnySatisfied(player, _gameWorld);
            if (!requirementsMet)
            {
                return ActionExecutionPlan.Invalid("Requirements not met");
            }
        }

        // STEP 2: HIGHLANDER - Validate ALL resource availability via OrPath using EFFECTIVE consequence
        // Caller builds OrPath directly - CompoundRequirement stays domain-agnostic
        // Sir Brante pattern: Resolve uses GATE (>= 0), others use AFFORDABILITY (>= cost)
        Consequence consequence = effectiveConsequence;
        OrPath resourcePath = new OrPath { Label = "Resource Requirements" };
        if (consequence.Resolve < 0) resourcePath.ResolveRequired = 0;  // Gate pattern
        if (consequence.Coins < 0) resourcePath.CoinsRequired = -consequence.Coins;
        if (consequence.Health < 0) resourcePath.HealthRequired = -consequence.Health;
        if (consequence.Stamina < 0) resourcePath.StaminaRequired = -consequence.Stamina;
        if (consequence.Focus < 0) resourcePath.FocusRequired = -consequence.Focus;
        if (consequence.Hunger > 0) resourcePath.HungerCapacityRequired = consequence.Hunger;
        if (!resourcePath.IsSatisfied(player, _gameWorld))
        {
            PathProjection projection = resourcePath.GetProjection(player, _gameWorld);
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

        // STEP 3: Build execution plan with EFFECTIVE consequence
        ActionExecutionPlan plan = ActionExecutionPlan.Valid();
        plan.ResolveCoins = resolveCost;
        plan.CoinsCost = coinsCost;
        plan.TimeSegments = consequence.TimeSegments;

        // Tutorial resource costs
        plan.HealthCost = healthCost;
        plan.StaminaCost = staminaCost;
        plan.FocusCost = focusCost;
        plan.HungerCost = hungerCost;

        plan.Consequence = consequence;  // EFFECTIVE consequence (scaled if provided)
        plan.ActionType = template.ActionType;
        plan.ChallengeType = template.ChallengeType;
        plan.ChallengeId = template.ChallengeId;
        plan.NavigationPayload = template.NavigationPayload;
        plan.ActionName = actionName;
        plan.IsAtmosphericAction = false;  // Scene-based action (not fallback scene)

        return plan;
    }
}

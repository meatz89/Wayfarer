
public class ActionSystem
{
    private readonly GameState gameState;
    private readonly MessageSystem messageSystem;

    public ActionSystem(GameState gameState, MessageSystem messageSystem)
    {
        this.gameState = gameState;
        this.messageSystem = messageSystem;
    }
    public bool CanExecuteInContext(ActionImplementation basicAction)
    {
        ActionImplementation modifiedAction = ModifyAction(basicAction);
        return modifiedAction.CanExecute(gameState);
    }

    public ActionImplementation ModifyAction(ActionImplementation originalAction)
    {
        ActionImplementation modifiedAction = new ActionImplementation
        {
            ActionType = originalAction.ActionType,
            Name = originalAction.Name,
            TimeWindows = new List<TimeWindows>(originalAction.TimeWindows),
            Requirements = new List<Requirement>(originalAction.Requirements),
            EnergyCosts = new List<Outcome>(originalAction.EnergyCosts),
            OutcomeConditions = originalAction.OutcomeConditions.ToList(),
            LocationArchetype = originalAction.LocationArchetype,
            CrowdDensity = originalAction.CrowdDensity,
            LocationScale = originalAction.LocationScale,
            SpotAvailabilityConditions = new List<LocationPropertyCondition>(originalAction.SpotAvailabilityConditions)
        };

        foreach (ActionModifier modifier in GetActiveModifiers())
        {
            if (IsModifierApplicable(modifier, modifiedAction))
                modifier.ApplyModification(modifiedAction);
        }

        return modifiedAction;
    }
    public List<ActionModifier> GetActiveModifiers()
    {
        List<ActionModifier> modifiers = new();

        // Get modifiers from equipped items
        foreach (Item item in gameState.Player.Equipment.GetEquippedItems())
        {
            modifiers.AddRange(item.ActionModifiers);
        }

        // Get modifiers from worn clothing
        foreach (Item clothing in gameState.Player.Equipment.GetWornClothing())
        {
            modifiers.AddRange(clothing.ActionModifiers);
        }

        return modifiers;
    }

    public bool ShouldApplyCondition(OutcomeCondition condition, EncounterContext context)
    {
        int currentValue = condition.ValueType switch
        {
            ValueTypes.Momentum => context.CurrentValues.Momentum,
            ValueTypes.Insight => context.CurrentValues.Insight,
            ValueTypes.Resonance => context.CurrentValues.Resonance,
            ValueTypes.Outcome => context.CurrentValues.Outcome,
            ValueTypes.Pressure => context.CurrentValues.Pressure,
            _ => 0
        };

        return currentValue >= condition.MinValue && currentValue <= condition.MaxValue;
    }

    private bool IsModifierApplicable(ActionModifier modifier, ActionImplementation action)
    {
        // Check if modifier applies to this action type
        if (modifier.ApplicableActions.Count > 0 &&
            !modifier.ApplicableActions.Contains(action.ActionType))
        {
            return false;
        }

        // Add more conditions here as needed, like:
        // - Time window checks
        // - Location type checks
        // - Player status checks

        return true;
    }
}
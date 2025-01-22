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
        // Create a copy of the action to apply modifiers
        ActionImplementation modifiedAction = ApplyModifiers(basicAction);
        return modifiedAction.CanExecute(gameState);
    }

    public ActionImplementation ProcessActionOutcome(ActionImplementation basicAction)
    {
        // Apply modifiers before processing the action
        ActionImplementation modifiedAction = ApplyModifiers(basicAction);

        // Process costs first
        foreach (Outcome cost in modifiedAction.Costs)
        {
            cost.Apply(gameState.Player);
            messageSystem.AddOutcome(cost);
        }

        // Then process rewards
        foreach (Outcome reward in modifiedAction.Rewards)
        {
            reward.Apply(gameState.Player);
            messageSystem.AddOutcome(reward);
        }

        // Check if this action triggered a day change
        bool dayChange = modifiedAction.Costs.Any(o => o is DayChangeOutcome);
        return modifiedAction;
    }

    public ActionImplementation GetModifiedAction(ActionImplementation originalAction)
    {
        // Create a new action with the same base properties
        ActionImplementation modifiedAction = new ActionImplementation
        {
            ActionType = originalAction.ActionType,
            Name = originalAction.Name,

            // Create new lists to avoid modifying the original
            TimeSlots = new List<TimeSlots>(originalAction.TimeSlots),
            Requirements = new List<Requirement>(originalAction.Requirements),
            Costs = new List<Outcome>(originalAction.Costs),
            Rewards = new List<Outcome>(originalAction.Rewards)
        };

        // Get all currently active modifiers
        List<ActionModifier> activeModifiers = GetActiveModifiers();

        // Apply each applicable modifier
        foreach (ActionModifier modifier in activeModifiers)
        {
            if (IsModifierApplicable(modifier, modifiedAction))
            {
                modifier.ApplyModification(modifiedAction);
            }
        }

        return modifiedAction;
    }

    private ActionImplementation ApplyModifiers(ActionImplementation originalAction)
    {
        // Create a new action with the same base properties
        ActionImplementation modifiedAction = new ActionImplementation
        {
            ActionType = originalAction.ActionType,
            Name = originalAction.Name,

            // Create new lists to avoid modifying the original
            TimeSlots = new List<TimeSlots>(originalAction.TimeSlots),
            Costs = new List<Outcome>(originalAction.Costs),
            Rewards = new List<Outcome>(originalAction.Rewards)
        };

        // Get all currently active modifiers
        List<ActionModifier> activeModifiers = GetActiveModifiers();

        // Apply each applicable modifier
        foreach (ActionModifier modifier in activeModifiers)
        {
            if (IsModifierApplicable(modifier, modifiedAction))
            {
                modifier.ApplyModification(modifiedAction);
            }
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
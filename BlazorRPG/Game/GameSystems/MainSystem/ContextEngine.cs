
public class ContextEngine
{
    private readonly GameState gameState;
    private readonly MessageSystem messageSystem;

    public ContextEngine(GameState gameState, MessageSystem messageSystem)
    {
        this.gameState = gameState;
        this.messageSystem = messageSystem;
    }

    public bool CanExecuteInContext(BasicAction basicAction)
    {
        // Create a copy of the action to apply modifiers
        BasicAction modifiedAction = ApplyModifiers(basicAction);
        return modifiedAction.CanExecute(gameState.Player);
    }

    public BasicAction ProcessActionOutcome(BasicAction basicAction)
    {
        // Apply modifiers before processing the action
        BasicAction modifiedAction = ApplyModifiers(basicAction);

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

    public BasicAction GetModifiedAction(BasicAction originalAction)
    {
        // Create a new action with the same base properties
        BasicAction modifiedAction = new BasicAction
        {
            ActionType = originalAction.ActionType,
            Name = originalAction.Name,
            TimeInvestment = originalAction.TimeInvestment,

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

    private BasicAction ApplyModifiers(BasicAction originalAction)
    {
        // Create a new action with the same base properties
        BasicAction modifiedAction = new BasicAction
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

    private bool IsModifierApplicable(ActionModifier modifier, BasicAction action)
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
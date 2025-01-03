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
        return basicAction.CanExecute(gameState.Player);
    }

    public bool ProcessActionOutcome(BasicAction basicAction)
    {
        // First apply all regular outcomes
        foreach (Outcome cost in basicAction.Costs)
        {
            cost.Apply(gameState.Player);
        }


        // First apply all regular outcomes
        foreach (Outcome reward in basicAction.Rewards)
        {
            reward.Apply(gameState.Player);
        }

        // Return whether this action should trigger day change
        return basicAction.Costs.Any(o => o is DayChangeOutcome) || basicAction.Rewards.Any(o => o is DayChangeOutcome);
    }
}
public class ActionValidator
{
    private readonly GameState gameState;

    public ActionValidator(GameState gameState)
    {
        this.gameState = gameState;
    }

    public bool CanExecuteAction(ActionImplementation action)
    {
        return action.CanExecute(gameState.Player);
    }

    public List<string> GetBlockingRequirements(ActionImplementation action)
    {
        List<string> blocking = new();
        foreach (Requirement req in action.Requirements)
        {
            if (!req.IsSatisfied(gameState.Player))
            {
                blocking.Add(req.GetDescription());
            }
        }
        return blocking;
    }
}
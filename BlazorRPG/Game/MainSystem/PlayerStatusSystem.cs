public class PlayerStatusSystem
{
    public PlayerStatusSystem(GameState gameState)
    {
        GameState = gameState;
    }

    private List<PlayerNegativeStatus> activeStatusList = new() {
        PlayerNegativeStatus.Cold,
        PlayerNegativeStatus.Exhausted,
    };

    public GameState GameState { get; }

    public void ApplyStatus(PlayerStatusTypes status)
    {

    }

    public void RemoveStatus(PlayerStatusTypes status)
    {

    }

    public bool HasStatus(PlayerStatusTypes status)
    {
        return true;
    }
}
